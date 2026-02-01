using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [SerializeField] private GameObject npcPrefab;

    [Header("Initial Spawn")]
    [SerializeField] private int initialSpawnCount = 3;

    [Header("Continuous Spawn")]
    [SerializeField] private float spawnIntervalStart = 1.5f;  // At 9 AM
    [SerializeField] private float spawnIntervalEnd = 0.3f;    // At 5 PM
    [SerializeField] private int maxNPCsStart = 15;            // At 9 AM
    [SerializeField] private int maxNPCsEnd = 30;              // At 5 PM

    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-6f, -3f);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(6f, 3f);
    [SerializeField] private float minDistanceFromPlayer = 3f;

    private float spawnTimer;
    private Transform playerTransform;

    void Start()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Initial spawn
        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnNPC();
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver()) return;

        // Calculate time progress (0 at 9AM, 1 at 5PM)
        float currentHour = GameManager.Instance.GetCurrentHour();
        float timeProgress = Mathf.InverseLerp(9f, 17f, currentHour);

        // Spawn faster as day progresses
        float currentInterval = Mathf.Lerp(spawnIntervalStart, spawnIntervalEnd, timeProgress);
        int currentMaxNPCs = Mathf.RoundToInt(Mathf.Lerp(maxNPCsStart, maxNPCsEnd, timeProgress));

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= currentInterval)
        {
            spawnTimer = 0f;

            // Count current NPCs
            int currentCount = FindObjectsByType<NPC>(FindObjectsSortMode.None).Length;
            if (currentCount < currentMaxNPCs)
            {
                SpawnNPC();
            }
        }
    }

    void SpawnNPC()
    {
        if (npcPrefab == null) return;

        Vector2 pos = GetSpawnPosition();
        Instantiate(npcPrefab, pos, Quaternion.identity);
    }

    Vector2 GetSpawnPosition()
    {
        Vector2 pos;
        int attempts = 0;

        do
        {
            pos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );
            attempts++;
        }
        while (playerTransform != null &&
               Vector2.Distance(pos, playerTransform.position) < minDistanceFromPlayer &&
               attempts < 20);

        return pos;
    }
}
