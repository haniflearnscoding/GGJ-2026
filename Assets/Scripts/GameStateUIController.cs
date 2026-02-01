using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GameStateUIController : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement winScreen;
    private VisualElement loseScreen;
    private Button restartBtn;
    private Button restartBtnLose;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();

        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            winScreen = root.Q<VisualElement>("win-screen");
            loseScreen = root.Q<VisualElement>("lose-screen");
            restartBtn = root.Q<Button>("restart-btn");
            restartBtnLose = root.Q<Button>("restart-btn-lose");

            if (restartBtn != null)
                restartBtn.clicked += RestartGame;
            if (restartBtnLose != null)
                restartBtnLose.clicked += RestartGame;

            // Hide both screens initially
            if (winScreen != null) winScreen.style.display = DisplayStyle.None;
            if (loseScreen != null) loseScreen.style.display = DisplayStyle.None;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.IsGameOver())
        {
            if (GameManager.Instance.HasWon())
            {
                ShowWinScreen();
            }
            else
            {
                ShowLoseScreen();
            }
        }
    }

    void ShowWinScreen()
    {
        if (winScreen != null)
            winScreen.style.display = DisplayStyle.Flex;
    }

    void ShowLoseScreen()
    {
        if (loseScreen != null)
            loseScreen.style.display = DisplayStyle.Flex;
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
