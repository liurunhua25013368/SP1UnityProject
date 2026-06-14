using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
    public float speed = 5f;

    [Header("Arduino Serial")]
    public SerialControllerTemplate serial;

    [Header("Shooting")]
    public float shootOffsetRange = 0.5f;
    public Projectile laserPrefab;
    private Projectile laser;

    private void Update()
    {
        Vector3 position = transform.position;


        float move = 0f;

        if (serial != null)
        {
            move = serial.Move; // -1/1
        }
        else
        {
            // fallback keyboard
            if (Input.GetKey(KeyCode.LeftArrow)) move = -1f;
            else if (Input.GetKey(KeyCode.RightArrow)) move = 1f;
        }

        position.x += move * speed * Time.deltaTime;

        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);
        position.x = Mathf.Clamp(position.x, leftEdge.x, rightEdge.x);

        transform.position = position;


        bool piezoShoot = (serial != null && serial.ShootDown);

        bool shootPressed =
            piezoShoot ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetMouseButtonDown(0);

       
        // bool shootPressed = piezoShoot;

        if (laser == null && shootPressed)
        {
            float offsetX = Random.Range(-shootOffsetRange, shootOffsetRange);
            Vector3 spawnPosition = transform.position + new Vector3(offsetX, 0f, 0f);
            laser = Instantiate(laserPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Missile") ||
            other.gameObject.layer == LayerMask.NameToLayer("Invader"))
        {
            GameManager.Instance.OnPlayerKilled(this);
        }
    }
}

