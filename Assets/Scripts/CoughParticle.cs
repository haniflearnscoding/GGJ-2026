using UnityEngine;

public class CoughParticle : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float lifetime;
    private float elapsed;
    private int damage;
    private NPC sourceNPC;
    private SpriteRenderer spriteRenderer;
    private bool hasDamaged = false;

    public void Initialize(Vector2 dir, float spd, float life, int dmg, NPC source)
    {
        direction = dir;
        speed = spd;
        lifetime = life;
        damage = dmg;
        sourceNPC = source;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        // Move outward
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // Slow down over time
        speed *= 0.98f;

        // Fade out and grow slightly
        if (spriteRenderer != null)
        {
            float t = elapsed / lifetime;
            Color c = spriteRenderer.color;
            c.a = Mathf.Lerp(0.7f, 0f, t);
            spriteRenderer.color = c;

            // Grow slightly as it disperses
            transform.localScale *= 1.01f;
        }

        // Destroy when lifetime is up
        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasDamaged) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
            hasDamaged = true;

            // Destroy immediately after dealing damage
            Destroy(gameObject);
        }
    }
}
