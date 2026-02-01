using UnityEngine;
using System.Collections;

public class NPC : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.1f;
    [SerializeField] private bool isDiseased = true;
    [SerializeField] private bool trackPlayer = true;

    [Header("Sprites")]
    [SerializeField] private Sprite spriteDown;
    [SerializeField] private Sprite spriteUp;
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private Sprite spriteRight;
    [SerializeField] private Sprite[] walkDown;
    [SerializeField] private Sprite[] walkUp;
    [SerializeField] private Sprite[] walkLeft;
    [SerializeField] private Sprite[] walkRight;
    [SerializeField] private float walkAnimSpeed = 0.15f;

    private Transform playerTarget;
    private float walkAnimTimer;
    private int walkFrame;
    private Vector2 lastDirection = Vector2.down;

    [Header("Cure Effect")]
    [SerializeField] private float cureEffectDuration = 0.3f;
    [SerializeField] private Sprite curedSprite;

    [Header("Damage")]
    [SerializeField] private int damageToPlayer = 1;
    [SerializeField] private float damageCooldown = 2f;
    private float lastDamageTime;

    [Header("Cough Effect")]
    [SerializeField] private float coughInterval = 3f;
    [SerializeField] private int coughParticleCount = 5;
    [SerializeField] private Sprite coughSprite; // Assign Z_cough1 in Inspector
    [SerializeField] private float coughParticleSpeed = 2f;
    [SerializeField] private float coughParticleLifetime = 1.5f;

    private SpriteRenderer spriteRenderer;
    private float coughTimer;
    private bool isCuring = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        coughTimer = Random.Range(0f, coughInterval); // Stagger coughs
    }

    void Start()
    {
        UpdateVisuals();

        // Find player
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            playerTarget = player.transform;
        }
    }

    void Update()
    {
        if (isCuring) return; // Stop movement during cure effect

        bool isMoving = false;

        // Track player
        if (trackPlayer && playerTarget != null && isDiseased)
        {
            Vector2 direction = (playerTarget.position - transform.position).normalized;
            transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
            isMoving = true;

            // Update facing direction
            if (Mathf.Abs(direction.y) >= Mathf.Abs(direction.x))
            {
                lastDirection = direction.y > 0 ? Vector2.up : Vector2.down;
            }
            else
            {
                lastDirection = direction.x > 0 ? Vector2.right : Vector2.left;
            }
        }

        // Update sprite animation
        UpdateSpriteDirection(isMoving);

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

    void UpdateSpriteDirection(bool isMoving)
    {
        if (spriteRenderer == null) return;

        // Handle walk animation
        if (isMoving)
        {
            walkAnimTimer += Time.deltaTime;
            if (walkAnimTimer >= walkAnimSpeed)
            {
                walkAnimTimer = 0f;
                walkFrame = (walkFrame + 1) % 2;
            }
        }
        else
        {
            walkFrame = 0;
            walkAnimTimer = 0f;
        }

        // Set sprite based on direction
        Sprite[] currentWalkSprites = null;
        Sprite idleSprite = null;

        if (lastDirection == Vector2.up)
        {
            idleSprite = spriteUp;
            currentWalkSprites = walkUp;
        }
        else if (lastDirection == Vector2.down)
        {
            idleSprite = spriteDown;
            currentWalkSprites = walkDown;
        }
        else if (lastDirection == Vector2.left)
        {
            idleSprite = spriteLeft;
            currentWalkSprites = walkLeft;
        }
        else if (lastDirection == Vector2.right)
        {
            idleSprite = spriteRight;
            currentWalkSprites = walkRight;
        }

        if (isMoving && currentWalkSprites != null && currentWalkSprites.Length > 0)
        {
            spriteRenderer.sprite = currentWalkSprites[walkFrame % currentWalkSprites.Length];
        }
        else if (idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
        }
    }

    void Cough()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound("cough");

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
        sr.sprite = coughSprite;
        sr.sortingOrder = 5;

        // Varying sizes for natural look
        float size = Random.Range(0.8f, 1.2f);
        particle.transform.localScale = Vector3.one * size;

        // Add collider for damage
        CircleCollider2D col = particle.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        // Add cough particle behavior
        CoughParticle cp = particle.AddComponent<CoughParticle>();

        // AIM AT PLAYER with spread
        Vector2 toPlayer = Vector2.up; // Default if no player
        if (playerTarget != null)
        {
            toPlayer = ((Vector2)playerTarget.position - (Vector2)transform.position).normalized;
        }

        // Add spread angle (wide cone toward player)
        float spreadAngle = Random.Range(-60f, 60f) * Mathf.Deg2Rad;
        float cos = Mathf.Cos(spreadAngle);
        float sin = Mathf.Sin(spreadAngle);
        Vector2 dir = new Vector2(
            toPlayer.x * cos - toPlayer.y * sin,
            toPlayer.x * sin + toPlayer.y * cos
        );

        cp.Initialize(dir, coughParticleSpeed, coughParticleLifetime, damageToPlayer, this);
    }

    void UpdateVisuals()
    {
        // Sprites handle visuals now
    }

    public void Cure()
    {
        if (!isDiseased || isCuring) return;
        isDiseased = false;
        isCuring = true;

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound("cure");

        StartCoroutine(CureEffect());
    }

    IEnumerator CureEffect()
    {
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;

        // Show cured sprite
        if (curedSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = curedSprite;
        }

        // Stay visible for a moment
        yield return new WaitForSeconds(0.5f);

        // Fade out and shrink
        while (elapsed < cureEffectDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / cureEffectDuration;

            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
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
