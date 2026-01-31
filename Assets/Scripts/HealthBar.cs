using UnityEngine;
using UnityEngine.UIElements;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    private UIDocument uiDocument;
    private VisualElement healthBarFill;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        Debug.Log($"HealthBar Start - UIDocument: {uiDocument != null}");

        if (uiDocument != null)
        {
            healthBarFill = uiDocument.rootVisualElement.Q<VisualElement>("health-bar-fill");
            Debug.Log($"HealthBar - healthBarFill found: {healthBarFill != null}");
        }

        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }
        Debug.Log($"HealthBar - Player found: {player != null}");

        if (player != null)
        {
            player.OnHealthChanged += UpdateHealthBar;
            Debug.Log("HealthBar - Subscribed to OnHealthChanged");
            UpdateHealthBar(player.GetCurrentHealth(), player.GetMaxHealth());
        }
    }

    void OnDestroy()
    {
        if (player != null)
        {
            player.OnHealthChanged -= UpdateHealthBar;
        }
    }

    void UpdateHealthBar(int current, int max)
    {
        Debug.Log($"HealthBar UpdateHealthBar called: {current}/{max}, healthBarFill: {healthBarFill != null}");
        if (healthBarFill == null) return;

        float percent = (float)current / max * 100f;
        Debug.Log($"HealthBar setting width to {percent}%");
        healthBarFill.style.width = new Length(percent, LengthUnit.Percent);

        // Change color based on health
        if (percent > 60)
            healthBarFill.style.backgroundColor = new Color(0.3f, 0.69f, 0.31f); // Green
        else if (percent > 30)
            healthBarFill.style.backgroundColor = new Color(1f, 0.76f, 0.03f); // Yellow
        else
            healthBarFill.style.backgroundColor = new Color(0.96f, 0.26f, 0.21f); // Red
    }
}
