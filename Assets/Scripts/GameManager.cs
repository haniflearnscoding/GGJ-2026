using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private bool gameOver = false;
    [SerializeField] private bool hasWon = false;

    [Header("Shift Timer")]
    [SerializeField] private float shiftStartHour = 9f;  // 9 AM
    [SerializeField] private float shiftEndHour = 17f;   // 5 PM
    [SerializeField] private float realSecondsPerGameHour = 15f; // 15 real seconds = 1 game hour

    private float currentGameHour;

    [Header("UI References (assign in Inspector)")]
    [SerializeField] private GameObject winUI;
    [SerializeField] private GameObject loseUI;

    private PlayerController player;

    public event System.Action<float> OnTimeChanged; // current hour

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        currentGameHour = shiftStartHour;

        if (winUI != null) winUI.SetActive(false);
        if (loseUI != null) loseUI.SetActive(false);

        OnTimeChanged?.Invoke(currentGameHour);
    }

    void Update()
    {
        if (gameOver) return;

        UpdateShiftTimer();
        CheckWinCondition();
        CheckLoseCondition();
    }

    void UpdateShiftTimer()
    {
        // Progress time
        float hoursPerSecond = 1f / realSecondsPerGameHour;
        currentGameHour += hoursPerSecond * Time.deltaTime;

        OnTimeChanged?.Invoke(currentGameHour);
    }

    void CheckWinCondition()
    {
        // Win if shift is over (5 PM)
        if (currentGameHour >= shiftEndHour)
        {
            Win();
        }
    }

    void CheckLoseCondition()
    {
        if (player == null) return;

        if (player.GetCurrentLives() <= 0)
        {
            Lose();
        }
    }

    void Win()
    {
        gameOver = true;
        hasWon = true;
        Debug.Log("YOU WIN!");

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound("win");

        if (winUI != null) winUI.SetActive(true);

        // Optional: Pause game
        Time.timeScale = 0f;
    }

    void Lose()
    {
        gameOver = true;
        hasWon = false;
        Debug.Log("GAME OVER!");

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound("lose");

        if (loseUI != null) loseUI.SetActive(true);

        // Optional: Pause game
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

    public bool HasWon()
    {
        return hasWon;
    }

    public float GetCurrentHour()
    {
        return currentGameHour;
    }

    public string GetTimeFormatted()
    {
        int hour = Mathf.FloorToInt(currentGameHour);
        int minutes = Mathf.FloorToInt((currentGameHour - hour) * 60f);
        string ampm = hour >= 12 ? "PM" : "AM";
        int displayHour = hour > 12 ? hour - 12 : (hour == 0 ? 12 : hour);
        return $"{displayHour}:{minutes:D2} {ampm}";
    }
}
