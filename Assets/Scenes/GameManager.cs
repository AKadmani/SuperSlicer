using UnityEngine;
using UnityEngine.XR;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI hintText;          // optional

    [Header("Gameplay")]
    public arcSpawner ballSpawner;
    public float preRoundDelay = 3f;
    public float roundDuration = 60f;

    int score;
    float timeLeft;
    bool roundRunning;

    /* ---------- SINGLETON (keeps IncrementScore() simple) ---------- */
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => StartCoroutine(GameLoop());

    /* ---------- MAIN LOOP ---------- */
    IEnumerator GameLoop()
    {
        // 1. fade-in music once when app launches
        FindFirstObjectByType<player>()?.PlayLoop();

        while (true)                    // endless sessions
        {
            yield return Countdown(preRoundDelay);   // 2. countdown

            StartRound();                              // 3.
            while (timeLeft > 0f)                      // 3a. tick timer
            {
                timeLeft -= Time.deltaTime;
                UpdateTimerUI();
                yield return null;
            }
            EndRound();                                // 3b.

            hintText?.SetText("Pull right trigger to restart");
            yield return WaitForTrigger();             // 4.
            hintText?.SetText(string.Empty);
        }
    }

    /* ---------- ROUND CONTROL ---------- */
    void StartRound()
    {
        score = 0;
        timeLeft = roundDuration;
        roundRunning = true;
        UpdateScoreUI();
        UpdateTimerUI();
        ballSpawner?.StartSpawning();   // ArcSpawner already has these
    }

    void EndRound()
    {
        roundRunning = false;
        ballSpawner?.StopSpawning();
    }

    public void IncrementScore()
    {
        if (!roundRunning) return;
        score++;
        UpdateScoreUI();
    }

    /* ---------- UI HELPERS ---------- */
    IEnumerator Countdown(float secs)
    {
        for (float t = secs; t > 0f; t -= 1f)
        {
            timerText?.SetText(Mathf.CeilToInt(t).ToString("0"));
            yield return new WaitForSeconds(1f);
        }
        timerText?.SetText(string.Empty);
    }

    void UpdateScoreUI() => scoreText?.SetText($"Score: {score}");
    void UpdateTimerUI() => timerText?.SetText($"Time: {Mathf.Max(0, Mathf.CeilToInt(timeLeft)):00}");

    /* ---------- RESTART INPUT (no obsolete API) ---------- */
    IEnumerator WaitForTrigger()
    {
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);  // gets / updates a device :contentReference[oaicite:0]{index=0}
        bool prevState = false;

        while (true)
        {
            // In play-mode a controller can reconnect; refresh if needed
            if (!rightHand.isValid)                       // validity test :contentReference[oaicite:1]{index=1}
                rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed) && pressed)   // feature query :contentReference[oaicite:2]{index=2}
            {
                if (!prevState) break;    // trigger was just pressed
                prevState = true;
            }
            else
            {
                prevState = false;
            }

#if UNITY_EDITOR        // convenience: hit Space in the editor
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space)) break;
#endif
            yield return null;
        }
    }
}
