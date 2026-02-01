using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;

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

    private float walkAnimTimer;
    private int walkFrame;
    private Vector2 lastDirection = Vector2.down;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    public event System.Action<int, int> OnHealthChanged; // current, max

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private float nextFireTime;
    private Vector2 movement;
    private Vector2 mousePos;
    private Camera mainCam;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        mainCam = Camera.main;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"Player TakeDamage called: {damage} damage, health: {currentHealth} -> {currentHealth - damage}");
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        // TODO: Add death logic (game over screen, respawn, etc.)
        Debug.Log("Player died!");
        gameObject.SetActive(false);
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;

    void Update()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
            return;
        }

        // Movement input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Update sprite direction based on movement
        UpdateSpriteDirection();

        // Mouse position for shooting
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // Shooting
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void UpdateSpriteDirection()
    {
        if (spriteRenderer == null) return;

        bool isMoving = movement != Vector2.zero;

        // Update direction when moving
        if (isMoving)
        {
            if (Mathf.Abs(movement.y) >= Mathf.Abs(movement.x))
            {
                lastDirection = movement.y > 0 ? Vector2.up : Vector2.down;
            }
            else
            {
                lastDirection = movement.x > 0 ? Vector2.right : Vector2.left;
            }
        }

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

        // Set sprite based on direction and animation frame
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

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Calculate rotation toward mouse
        Vector2 shootDir = mousePos - (Vector2)transform.position;
        float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg - 90f;
        Quaternion bulletRotation = Quaternion.Euler(0, 0, angle);

        Instantiate(bulletPrefab, firePoint.position, bulletRotation);
    }
}
