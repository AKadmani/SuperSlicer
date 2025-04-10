using UnityEngine;
using TMPro; // Add this for TextMeshPro

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Singleton pattern

    public TextMeshProUGUI scoreText; // Assign ScoreText in Inspector
    public TextMeshProUGUI timerText; // Assign TimerText in Inspector
    public BallSpawner ballSpawner; // Assign BallSpawner GameObject in Inspector

    public float gameDuration = 0.0f;

    private int score = 0;
    private float currentTime;
    private bool gameRunning = false;

    void Awake()
    {
        // Simple Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // Initialize UI
        UpdateScoreUI();
        currentTime = gameDuration;
        UpdateTimerUI();

        // Start the game automatically (or trigger it via button press etc.)
        StartGame();
    }

    void Update()
    {
        if (gameRunning)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                EndGame();
            }
            UpdateTimerUI();
        }
    }

    public void StartGame()
    {
        score = 0;
        currentTime = gameDuration;
        gameRunning = true;
        UpdateScoreUI();
        UpdateTimerUI();

        if (ballSpawner != null)
        {
            ballSpawner.StartSpawning();
        }
        else
        {
            Debug.LogError("GameManager needs a reference to the BallSpawner!");
        }

        Debug.Log("Game Started!");
    }

    public void EndGame()
    {
        gameRunning = false;
        currentTime = 0;
        UpdateTimerUI();

        if (ballSpawner != null)
        {
            ballSpawner.StopSpawning();
        }

        Debug.Log($"Game Over! Final Score: {score}");
        // Add logic for game over screen, restart button, etc. here
    }


    public void IncrementScore()
    {
        if (!gameRunning) return; // Don't score if game isn't running

        score++;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // Format time nicely (e.g., 00)
            timerText.text = "Time: " + Mathf.Max(0, Mathf.CeilToInt(currentTime)).ToString("00");
        }
    }
}