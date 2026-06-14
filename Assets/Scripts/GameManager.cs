using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;
    [SerializeField] private Text timerText;


    private Player player;
    private Invaders invaders;
    private MysteryShip mysteryShip;
    private Bunker[] bunkers;

    public int score { get; private set; } = 0;
    public int lives { get; private set; } = 3;

[SerializeField] private float maxGameTime = 600f;
private float elapsedTime = 0f;
[SerializeField] private int lifeCost = 1000;

private bool isGameOver = false;


    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();
        invaders = FindObjectOfType<Invaders>();
        mysteryShip = FindObjectOfType<MysteryShip>();
        bunkers = FindObjectsOfType<Bunker>();

        NewGame();
    }

    private void Update()
    {
        // if game over only adress the new setting of the game, otherwise update the game state

    if (isGameOver)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            NewGame();
        }
        return;
    }

    //public timer

    elapsedTime += Time.deltaTime;

    //renew the timer text every frame
    if (timerText != null)
    {
        float timeLeft = Mathf.Max(0f, maxGameTime - elapsedTime);
        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // time over, the game time is up game over even if there are still lives
    if (elapsedTime >= maxGameTime)
    {
        GameOver();
        return;
    }

    //only allow buying life if the player has at least 1 life left, otherwise the game is over and no more lives can be bought
    //push B to buy a life
    if (lives > 0 && Input.GetKeyDown(KeyCode.B))
    {
        TryBuyLife();
    }
    }

    private void NewGame()
    {
        gameOverUI.SetActive(false);

    isGameOver = false;
    elapsedTime = 0f;

        SetScore(0);
        SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
        invaders.ResetInvaders();
        invaders.gameObject.SetActive(true);

        for (int i = 0; i < bunkers.Length; i++) {
            bunkers[i].ResetBunker();
        }

        Respawn();
    }

    private void Respawn()
    {
        Vector3 position = player.transform.position;
        position.x = 0f;
        player.transform.position = position;
        player.gameObject.SetActive(true);
    }

    private void GameOver()
    {
      if (isGameOver) return;   // aviod calling GameOver multiple times

    isGameOver = true;

    gameOverUI.SetActive(true);
    invaders.gameObject.SetActive(false);

    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(4, '0');
    }

    private void SetLives(int lives)
    {
        this.lives = Mathf.Max(lives, 0);
        livesText.text = this.lives.ToString();
    }

    // 1000 points (lifeCost can be set in Inspector)
private void TryBuyLife()
{
    // don't allow purchasing a life when the player has 0 or fewer lives (but Update already checks for lives>0)
    if (lives <= 0) return;

    // not enough points to buy a life
    if (score < lifeCost) return;

    // really buy: deduct points add life
    SetScore(score - lifeCost);
    SetLives(lives + 1);
}


    public void OnPlayerKilled(Player player)
    {
        SetLives(lives - 1);

        player.gameObject.SetActive(false);
    
        if (lives > 0) {
            Invoke(nameof(NewRound), 1f);
        } else {
            GameOver();
        }
    }

    public void OnInvaderKilled(Invader invader)
    {
        invader.gameObject.SetActive(false);

        SetScore(score + invader.score);

        if (invaders.GetAliveCount() == 0) {
            NewRound();
        }
    }

    public void OnMysteryShipKilled(MysteryShip mysteryShip)
    {
        SetScore(score + mysteryShip.score);
    }

    public void OnBoundaryReached()
    {
        if (invaders.gameObject.activeSelf)
        {
            invaders.gameObject.SetActive(false);
            OnPlayerKilled(player);
        }
    }

}
