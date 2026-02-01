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

        // Move in direction at speed
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);

        // Only fade in the last 10% of lifetime
        if (spriteRenderer != null)
        {
            float fadeStart = lifetime * 0.9f;
            if (elapsed > fadeStart)
            {
                float fadeProgress = (elapsed - fadeStart) / (lifetime - fadeStart);
                Color c = spriteRenderer.color;
                c.a = Mathf.Lerp(1f, 0f, fadeProgress);
                spriteRenderer.color = c;
            }
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
