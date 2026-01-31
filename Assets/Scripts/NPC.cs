using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float changeDirectionTime = 2f;
    [SerializeField] private bool isDiseased = true;
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color diseasedColor = Color.red;
    [SerializeField] private Vector2 boundsMin = new Vector2(-8f, -4f);
    [SerializeField] private Vector2 boundsMax = new Vector2(8f, 4f);

    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection;
    private float timer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        PickRandomDirection();
        UpdateVisuals();
    }

    void Update()
    {
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
        if (!isDiseased) return;
        isDiseased = false;
        UpdateVisuals();
    }

    public bool IsDiseased()
    {
        return isDiseased;
    }
}
