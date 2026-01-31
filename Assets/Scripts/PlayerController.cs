using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;

    [Header("Sprites (assign from sliced sprite sheet)")]
    [SerializeField] private Sprite spriteDown;
    [SerializeField] private Sprite spriteUp;
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private Sprite spriteRight;

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
        if (movement == Vector2.zero) return; // Keep last direction when idle

        // Prioritize vertical if moving more vertically, else horizontal
        if (Mathf.Abs(movement.y) >= Mathf.Abs(movement.x))
        {
            if (movement.y > 0 && spriteUp != null)
                spriteRenderer.sprite = spriteUp;
            else if (movement.y < 0 && spriteDown != null)
                spriteRenderer.sprite = spriteDown;
        }
        else
        {
            if (movement.x > 0 && spriteRight != null)
                spriteRenderer.sprite = spriteRight;
            else if (movement.x < 0 && spriteLeft != null)
                spriteRenderer.sprite = spriteLeft;
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
