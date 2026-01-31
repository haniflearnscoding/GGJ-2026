using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private int spawnCount = 5;
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-6f, -3f);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(6f, 3f);

    void Start()
    {
        if (npcPrefab == null) return;  

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 pos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );
            Instantiate(npcPrefab, pos, Quaternion.identity);
        }
    }
}
