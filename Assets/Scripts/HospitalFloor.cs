using UnityEngine;

public class HospitalFloor : MonoBehaviour
{
    [Header("Floor Settings")]
    [SerializeField] private Sprite tileSprite;
    [SerializeField] private int tilesX = 30;
    [SerializeField] private int tilesY = 20;
    [SerializeField] private float tileSize = 1f;

    void Start()
    {
        GenerateFloor();
    }

    void GenerateFloor()
    {
        if (tileSprite == null) return;

        GameObject floorParent = new GameObject("FloorTiles");
        floorParent.transform.parent = transform;

        // Calculate offset to center the floor
        float offsetX = (tilesX * tileSize) / 2f - tileSize / 2f;
        float offsetY = (tilesY * tileSize) / 2f - tileSize / 2f;

        for (int y = 0; y < tilesY; y++)
        {
            for (int x = 0; x < tilesX; x++)
            {
                GameObject tile = new GameObject($"Tile_{x}_{y}");
                tile.transform.parent = floorParent.transform;

                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = tileSprite;
                sr.sortingOrder = -100;

                float posX = x * tileSize - offsetX;
                float posY = y * tileSize - offsetY;
                tile.transform.position = new Vector3(posX, posY, 0);
                tile.transform.localScale = Vector3.one * (tileSize / tileSprite.bounds.size.x);
            }
        }
    }
}
