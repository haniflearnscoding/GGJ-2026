using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 3f;

    [Header("Spin Animation")]
    [SerializeField] private float spinSpeed = 720f;
    [SerializeField] private Sprite[] spinSprites;
    [SerializeField] private float spriteAnimSpeed = 0.1f;

    [Header("Fallback (if no spin sprites)")]
    [SerializeField] private Sprite defaultSprite;

    private SpriteRenderer spriteRenderer;
    private float animTimer;
    private int spriteFrame;
    private Vector3 moveDirection;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        moveDirection = transform.up;

        // Set initial sprite
        if (spriteRenderer != null)
        {
            if (spinSprites != null && spinSprites.Length > 0)
            {
                spriteRenderer.sprite = spinSprites[0];
            }
            else if (defaultSprite != null)
            {
                spriteRenderer.sprite = defaultSprite;
            }
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move in direction
        transform.position += moveDirection * speed * Time.deltaTime;

        // Spin rotation
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

        // Sprite animation (if sprites assigned)
        if (spinSprites != null && spinSprites.Length > 0 && spriteRenderer != null)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= spriteAnimSpeed)
            {
                animTimer = 0f;
                spriteFrame = (spriteFrame + 1) % spinSprites.Length;
                spriteRenderer.sprite = spinSprites[spriteFrame];
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            NPC npc = other.GetComponent<NPC>();
            if (npc != null)
            {
                npc.Cure();

                // Camera shake on successful cure
                if (CameraShake.Instance != null)
                    CameraShake.Instance.ShakeLight();
            }
            Destroy(gameObject);
        }
    }
}
