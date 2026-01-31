using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.2f;
    private float nextFireTime;
    
    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 mousePos;
    private Camera mainCam;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
    }

    void Update()
    {
        // Get movement input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
        // Get mouse position for aiming (need Z depth for 2D)
        mousePos = mainCam.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            -mainCam.transform.position.z
        ));
        // Shooting
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void FixedUpdate()
    {
        // Move player
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        
        // Rotate toward mouse
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
    }
    void Shoot()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}
