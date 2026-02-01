using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    private UIDocument uiDocument;
    private Label timeLabel;
    private List<VisualElement> hearts = new List<VisualElement>();

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();

        if (uiDocument != null)
        {
            timeLabel = uiDocument.rootVisualElement.Q<Label>("time-label");

            // Get all hearts
            hearts.Add(uiDocument.rootVisualElement.Q<VisualElement>("heart-1"));
            hearts.Add(uiDocument.rootVisualElement.Q<VisualElement>("heart-2"));
            hearts.Add(uiDocument.rootVisualElement.Q<VisualElement>("heart-3"));
        }

        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
        }

        if (player != null)
        {
            player.OnLivesChanged += UpdateLives;
            UpdateLives(player.GetCurrentLives(), player.GetMaxLives());
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimeChanged += UpdateTime;
        }
    }

    void OnDestroy()
    {
        if (player != null)
        {
            player.OnLivesChanged -= UpdateLives;
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimeChanged -= UpdateTime;
        }
    }

    void UpdateTime(float hour)
    {
        if (timeLabel == null || GameManager.Instance == null) return;
        timeLabel.text = GameManager.Instance.GetTimeFormatted();
    }

    void UpdateLives(int current, int max)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            if (hearts[i] == null) continue;

            if (i < current)
            {
                hearts[i].RemoveFromClassList("heart-empty");
                hearts[i].AddToClassList("heart-full");
            }
            else
            {
                hearts[i].RemoveFromClassList("heart-full");
                hearts[i].AddToClassList("heart-empty");
            }
        }
    }
}
