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

    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection;
    private float timer;
    private bool isCuring = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"NPC OnTriggerEnter2D: {other.name}");
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log($"NPC OnTriggerStay2D: {other.name}, isDiseased={isDiseased}, isCuring={isCuring}");

        if (!isDiseased || isCuring) return;
        if (Time.time < lastDamageTime + damageCooldown) return;

        PlayerController player = other.GetComponent<PlayerController>();
        Debug.Log($"PlayerController found: {player != null}");
        if (player != null)
        {
            Debug.Log("Dealing damage to player!");
            player.TakeDamage(damageToPlayer);
            lastDamageTime = Time.time;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"NPC OnCollisionEnter2D: {collision.gameObject.name}");
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log($"NPC OnCollisionStay2D: {collision.gameObject.name}, isDiseased={isDiseased}, isCuring={isCuring}");

        if (!isDiseased || isCuring) return;
        if (Time.time < lastDamageTime + damageCooldown) return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        Debug.Log($"PlayerController found: {player != null}");
        if (player != null)
        {
            Debug.Log("Dealing damage to player!");
            player.TakeDamage(damageToPlayer);
            lastDamageTime = Time.time;
        }
    }
}
