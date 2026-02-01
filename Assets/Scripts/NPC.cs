using UnityEngine;
using System.Collections;

public class NPC : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.1f;
        [SerializeField] private float changeDirectionTime = 2f;
    [SerializeField] private bool isDiseased = true;
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color diseasedColor = Color.red;
    [SerializeField] private Vector2 boundsMin = new Vector2(-8f, -4f);
    [SerializeField] private Vector2 boundsMax = new Vector2(8f, 4f);

    [Header("Cure Effect")]
    [SerializeField] private float cureEffectDuration = 0.5f;
    [SerializeField] private Color flashColor = Color.white;

    [Header("Damage")]
    [SerializeField] private int damageToPlayer = 10;
    [SerializeField] private float damageCooldown = 1f;
    private float lastDamageTime;

    [Header("Cough Effect")]
    [SerializeField] private float coughInterval = 2f;
    [SerializeField] private float coughRadius = 1.5f;
    [SerializeField] private int coughParticleCount = 5;
    [SerializeField] private Color coughColor = new Color(0.6f, 0.8f, 0.3f, 0.7f);
    [SerializeField] private float coughParticleSpeed = 2f;
    [SerializeField] private float coughParticleLifetime = 0.5f;

    private SpriteRenderer spriteRenderer;
    private float coughTimer;
    private Vector2 moveDirection;
    private float timer;
    private bool isCuring = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        coughTimer = Random.Range(0f, coughInterval); // Stagger coughs
    }

    void Start()
    {
        PickRandomDirection();
        UpdateVisuals();

        var col = GetComponent<Collider2D>();
        var rb = GetComponent<Rigidbody2D>();
        Debug.Log($"NPC Start - Collider: {col != null}, IsTrigger: {(col != null ? col.isTrigger : false)}, Rigidbody2D: {rb != null}");
    }

    void Update()
    {
        if (isCuring) return; // Stop movement during cure effect

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            PickRandomDirection();
        }

        // Move
        Vector3 pos = transform.position;
        pos += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);

        // Bounce off bounds
        if (pos.x < boundsMin.x || pos.x > boundsMax.x)
        {
            moveDirection.x *= -1;
            pos.x = Mathf.Clamp(pos.x, boundsMin.x, boundsMax.x);
        }
        if (pos.y < boundsMin.y || pos.y > boundsMax.y)
        {
            moveDirection.y *= -1;
            pos.y = Mathf.Clamp(pos.y, boundsMin.y, boundsMax.y);
        }

        transform.position = pos;

        // Cough periodically
        if (isDiseased)
        {
            coughTimer -= Time.deltaTime;
            if (coughTimer <= 0)
            {
                Cough();
                coughTimer = coughInterval;
            }
        }
    }

    void Cough()
    {
        for (int i = 0; i < coughParticleCount; i++)
        {
            SpawnCoughParticle();
        }
    }

    void SpawnCoughParticle()
    {
        GameObject particle = new GameObject("CoughParticle");
        particle.transform.position = transform.position;

        SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(16);
        sr.color = coughColor;
        sr.sortingOrder = 5;
        particle.transform.localScale = Vector3.one * Random.Range(0.1f, 0.25f);

        // Add collider for damage
        CircleCollider2D col = particle.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        // Add cough particle behavior
        CoughParticle cp = particle.AddComponent<CoughParticle>();

        // Random direction with slight upward bias (like a real cough)
        Vector2 dir = new Vector2(
            Random.Range(-1f, 1f),
            Random.Range(-0.3f, 1f)
        ).normalized;

        cp.Initialize(dir, coughParticleSpeed, coughParticleLifetime, damageToPlayer, this);
    }

    Sprite CreateCircleSprite(int size = 64)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist < radius)
                {
                    float alpha = 1f - (dist / radius);
                    alpha = alpha * alpha;
                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    colors[y * size + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    void PickRandomDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        timer = changeDirectionTime;
    }

    void UpdateVisuals()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = isDiseased ? diseasedColor : healthyColor;
    }

    public void Cure()
    {
        if (!isDiseased || isCuring) return;
        isDiseased = false;
        isCuring = true;
        StartCoroutine(CureEffect());
    }

    IEnumerator CureEffect()
    {
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;

        // Flash white and pop up
        if (spriteRenderer != null)
            spriteRenderer.color = flashColor;
        transform.localScale = originalScale * 1.3f;

        yield return new WaitForSeconds(0.1f);

        // Fade out and shrink
        float fadeTime = cureEffectDuration - 0.1f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;

            // Shrink
            transform.localScale = Vector3.Lerp(originalScale * 1.3f, Vector3.zero, t);

            // Fade
            if (spriteRenderer != null)
            {
                Color c = healthyColor;
                c.a = 1f - t;
                spriteRenderer.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    public bool IsDiseased()
    {
        return isDiseased;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!isDiseased || isCuring) return;
        if (Time.time < lastDamageTime + damageCooldown) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damageToPlayer);
            lastDamageTime = Time.time;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!isDiseased || isCuring) return;
        if (Time.time < lastDamageTime + damageCooldown) return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damageToPlayer);
            lastDamageTime = Time.time;
        }
    }
}
