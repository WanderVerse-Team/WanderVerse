using UnityEngine;

public class InfiniteOceanTiler : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform[] tiles; // Drag OceanTile_0,1,2 here in order (top to bottom)

    [Header("Tile Settings")]
    public float tileHeight = 10f; // Set this correctly (see steps below)

    void LateUpdate()
    {
        if (cam == null || tiles == null || tiles.Length == 0) return;

        // Camera top and bottom in world space
        float camTop = cam.transform.position.y + cam.orthographicSize;
        float camBottom = cam.transform.position.y - cam.orthographicSize;

        // Find current topmost and bottommost tile
        Transform topTile = tiles[0];
        Transform bottomTile = tiles[0];

        for (int i = 1; i < tiles.Length; i++)
        {
            if (tiles[i].position.y > topTile.position.y) topTile = tiles[i];
            if (tiles[i].position.y < bottomTile.position.y) bottomTile = tiles[i];
        }

        // If the top tile is completely above the camera view, move it below the bottom tile
        // (This makes the ocean infinite when scrolling down)
        if (topTile.position.y - (tileHeight * 0.5f) > camTop)
        {
            Vector3 newPos = topTile.position;
            newPos.y = bottomTile.position.y - tileHeight;
            topTile.position = newPos;
        }

        // If the bottom tile is completely below the camera view, move it above the top tile
        // (This makes the ocean infinite when scrolling up)
        if (bottomTile.position.y + (tileHeight * 0.5f) < camBottom)
        {
            Vector3 newPos = bottomTile.position;
            newPos.y = topTile.position.y + tileHeight;
            bottomTile.position = newPos;
        }
    }
}