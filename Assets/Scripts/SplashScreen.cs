using UnityEngine;
using UnityEngine.UIElements;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] private Sprite splashSprite;
    [SerializeField] private Sprite startTextSprite;
    [SerializeField] private float fadeSpeed = 2f;

    private GameObject splashObject;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer textSpriteRenderer;
    private GameObject textObject;
    private bool isShowing = true;
    private bool fadingOut = false;

    // UI references to hide
    private UIDocument[] uiDocuments;

    void Awake()
    {
        HideUI();
        CreateSplash();
        CreateBlinkingText();
        PauseGame();
    }

    void HideUI()
    {
        uiDocuments = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
        foreach (var doc in uiDocuments)
        {
            if (doc.rootVisualElement != null)
                doc.rootVisualElement.style.display = DisplayStyle.None;
        }
    }

    void ShowUI()
    {
        foreach (var doc in uiDocuments)
        {
            if (doc != null && doc.rootVisualElement != null)
                doc.rootVisualElement.style.display = DisplayStyle.Flex;
        }
    }

    void CreateSplash()
    {
        // Create splash object
        splashObject = new GameObject("SplashImage");
        spriteRenderer = splashObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = splashSprite;
        spriteRenderer.sortingOrder = 1000;

        // Scale to fit screen
        if (Camera.main != null && splashSprite != null)
        {
            float screenHeight = Camera.main.orthographicSize * 2f;
            float screenWidth = screenHeight * Camera.main.aspect;

            float spriteHeight = splashSprite.bounds.size.y;
            float spriteWidth = splashSprite.bounds.size.x;

            float scaleX = screenWidth / spriteWidth;
            float scaleY = screenHeight / spriteHeight;
            float scale = Mathf.Min(scaleX, scaleY); // Fit instead of fill

            splashObject.transform.localScale = Vector3.one * scale;
            splashObject.transform.position = Camera.main.transform.position + Vector3.forward * 5f;
        }
    }

    void CreateBlinkingText()
    {
        textObject = new GameObject("StartText");
        textSpriteRenderer = textObject.AddComponent<SpriteRenderer>();
        textSpriteRenderer.sprite = startTextSprite;
        textSpriteRenderer.sortingOrder = 1001;

        // Position at bottom of screen
        if (Camera.main != null)
        {
            Vector3 pos = Camera.main.transform.position + Vector3.forward * 5f;
            pos.y -= Camera.main.orthographicSize * 0.7f;
            textObject.transform.position = pos;
            textObject.transform.localScale = Vector3.one * 0.2f;
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (!isShowing) return;

        // Blink text
        if (textSpriteRenderer != null && !fadingOut)
        {
            float alpha = (Mathf.Sin(Time.unscaledTime * 4f) + 1f) / 2f;
            textSpriteRenderer.color = new Color(1f, 1f, 1f, alpha);
        }

        if (Input.anyKeyDown && !fadingOut)
        {
            fadingOut = true;
            StartCoroutine(FadeOut());
        }
    }

    System.Collections.IEnumerator FadeOut()
    {
        float alpha = 1f;

        while (alpha > 0f)
        {
            alpha -= Time.unscaledDeltaTime * fadeSpeed;

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = alpha;
                spriteRenderer.color = c;
            }

            if (textSpriteRenderer != null)
            {
                textSpriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            }

            yield return null;
        }

        isShowing = false;
        ShowUI();
        ResumeGame();
        Destroy(splashObject);
        Destroy(textObject);
        Destroy(this);
    }
}
