using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerSpawner : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public GameObject playerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Vector3 SpawnPlayerPosition()
    {
        BoundsInt bounds = mapGenerator.groundTilemap.cellBounds;
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            if (position.y >= mapGenerator.height / 3 && position.y <= mapGenerator.height * 2 / 3 && position.x >= mapGenerator.width / 3 && position.x <= mapGenerator.width * 2 / 3)
            {
                TileBase groundTile = mapGenerator.groundTilemap.GetTile(position);
                TileBase wallTile = mapGenerator.wallsTilemap.GetTile(position);
                TileBase bedrock = mapGenerator.bedrockTilemap.GetTile(position);

                if (groundTile != null && wallTile == null && bedrock == null)
                {
                    return mapGenerator.groundTilemap.GetCellCenterWorld(position);  // Devuelve la posición en coordenadas del mundo
                }
            }
        }
        return Vector3.zero;
    }

    public void SpawnPlayer() {
        Vector3 spawnPosition = SpawnPlayerPosition();
        if (spawnPosition != Vector3.zero)
        {
            Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogError("No se encontró una posición válida para el jugador.");
        }
    }
}
