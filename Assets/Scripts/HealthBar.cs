using UnityEngine;
using UnityEngine.UIElements;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    private UIDocument uiDocument;
    private VisualElement healthBarFill;
    private VisualElement healthBarBg;
    private Label timeLabel;
    private Label healthText;

    private float flashTimer;
    private bool isFlashing;
    private bool flashOn;
    private int lastHealth;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();

        if (uiDocument != null)
        {
            healthBarFill = uiDocument.rootVisualElement.Q<VisualElement>("health-bar-fill");
            healthBarBg = uiDocument.rootVisualElement.Q<VisualElement>("health-bar-bg");
            timeLabel = uiDocument.rootVisualElement.Q<Label>("time-label");
            healthText = uiDocument.rootVisualElement.Q<Label>("health-text");
        }

        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }

        if (player != null)
        {
            player.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(player.GetCurrentHealth(), player.GetMaxHealth());
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimeChanged += UpdateTime;
        }
    }

    void Update()
    {
        // Flash effect when low health
        if (isFlashing)
        {
            flashTimer += Time.deltaTime;
            if (flashTimer >= 0.15f)
            {
                flashTimer = 0f;
                flashOn = !flashOn;
                if (healthBarFill != null)
                {
                    healthBarFill.style.backgroundColor = flashOn
                        ? new Color(1f, 0.2f, 0.2f)
                        : new Color(0.6f, 0f, 0f);
                }
            }
        }
    }

    void OnDestroy()
    {
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHealthBar;
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

    void UpdateHealthBar(int current, int max)
    {
        if (healthBarFill == null) return;

        float percent = (float)current / max * 100f;
        healthBarFill.style.width = new Length(percent, LengthUnit.Percent);

        // Update health text
        if (healthText != null)
        {
            healthText.text = $"{current}/{max}";
        }

        // Damage flash effect
        if (current < lastHealth && healthBarBg != null)
        {
            StartCoroutine(DamageFlash());
        }
        lastHealth = current;

        // Change color and enable flashing based on health
        isFlashing = false;
        if (percent > 60)
        {
            healthBarFill.style.backgroundColor = new Color(0f, 0.85f, 0f); // Bright green
        }
        else if (percent > 30)
        {
            healthBarFill.style.backgroundColor = new Color(1f, 0.8f, 0f); // Yellow
        }
        else
        {
            healthBarFill.style.backgroundColor = new Color(1f, 0.2f, 0.2f); // Red
            isFlashing = true; // Flash when low
        }
    }

    System.Collections.IEnumerator DamageFlash()
    {
        if (healthBarBg == null) yield break;

        // Flash background
        healthBarBg.style.backgroundColor = new Color(0.4f, 0.1f, 0.1f);
        yield return new WaitForSeconds(0.1f);
        healthBarBg.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
    }
}
