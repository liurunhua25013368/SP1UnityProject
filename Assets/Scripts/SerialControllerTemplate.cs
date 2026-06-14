using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System.Collections.Concurrent;
using System.Globalization;
using System;

public class SerialControllerTemplate : MonoBehaviour
{
    [Header("Serial Settings")]
    [SerializeField] private string portName = "COM6";
    [SerializeField] private int baudRate = 9600;

    [Header("Movement Feel")]
    [SerializeField] private float moveSmooth = 12f;
    [SerializeField] private float moveDeadzone = 0.06f;

    private SerialPort serialPort;
    private Thread serialThread;
    private bool running;
    private readonly ConcurrentQueue<string> queue = new();

    // Outputs
    public float Move { get; private set; } = 0f;     // -1..1
    public float Left { get; private set; } = 0f;     // 0..1
    public float Right { get; private set; } = 0f;    // 0..1

    // Shoot pulse
    public bool ShootDown { get; private set; } = false;

    public string LastLine { get; private set; } = "";

    void Start()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate)
            {
                ReadTimeout = 200,
                NewLine = "\n",
                DtrEnable = true,
                RtsEnable = true
            };
            serialPort.Open();
            Thread.Sleep(1200);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to open serial port {portName}: {e.Message}");
            return;
        }

        running = true;
        serialThread = new Thread(ReadSerial) { IsBackground = true };
        serialThread.Start();
    }

    void Update()
    {
        ShootDown = false; // not pressed by default

        //capture the latest line 
        string newLine = null;
        while (queue.TryDequeue(out string line)) newLine = line;
        if (string.IsNullOrEmpty(newLine)) return;

        newLine = newLine.Trim();
        if (newLine.Length == 0) return;

        LastLine = newLine;

        //move left right shoot
        string[] parts = newLine.Split(',');
        if (parts.Length < 4) return;

        if (!TryParse(parts[0], out float m)) return;
        if (!TryParse(parts[1], out float l)) return;
        if (!TryParse(parts[2], out float r)) return;
        if (!TryParse(parts[3], out float s)) return;

        m = Mathf.Clamp(m, -1f, 1f);
        l = Mathf.Clamp01(l);
        r = Mathf.Clamp01(r);

        Left = l;
        Right = r;

        if (Mathf.Abs(m) < moveDeadzone) m = 0f;

        Move = Mathf.Lerp(Move, m, Time.deltaTime * moveSmooth);

        //  0 / 1
        ShootDown = (s >= 0.5f);
    }

    bool TryParse(string s, out float v)
    {
        return float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v);
    }

    void OnDestroy()
    {
        running = false;
        try { if (serialThread != null && serialThread.IsAlive) serialThread.Join(200); } catch { }
        try { if (serialPort != null && serialPort.IsOpen) serialPort.Close(); } catch { }
    }

    void ReadSerial()
    {
        while (running && serialPort != null && serialPort.IsOpen)
        {
            try
            {
                string line = serialPort.ReadLine();
                if (!string.IsNullOrEmpty(line)) queue.Enqueue(line);
            }
            catch (TimeoutException) { }
            catch { break; }
        }
    }
}
