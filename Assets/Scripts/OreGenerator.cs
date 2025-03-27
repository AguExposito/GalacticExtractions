using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class OreGenerator : MonoBehaviour
{
    MapGenerator mapGenerator;
    public Tilemap oreTilemap;

    [Header("Ore Settings")]
    public TileBase[] oreTilesDesert; // Diferentes tipos de minerales
    public TileBase[] oreTilesGround; // Diferentes tipos de minerales
    public TileBase[] oreTilesSnow; // Diferentes tipos de minerales
    public int minClusters = 10;
    public int maxClusters = 20;
    public int minClusterSize = 8;
    public int maxClusterSize = 15;

    private int mapWidth;
    private int mapHeight;
    public Dictionary<Vector3Int, TileData> tileHealthData = new Dictionary<Vector3Int, TileData>();

    void Start()
    {
        mapGenerator = GetComponent<MapGenerator>();
        BoundsInt bounds = mapGenerator.wallsTilemap.cellBounds;
        mapWidth = bounds.size.x;
        mapHeight = bounds.size.y;

        GenerateOres();
    }

    void GenerateOres()
    {
        int numClusters = Random.Range(minClusters, maxClusters);

        for (int i = 0; i < numClusters; i++)
        {
            Vector3Int seedPosition = Vector3Int.zero;
            bool foundValidTile = false;
            int attempts = 0;

            while (attempts < 100)
            {
                seedPosition = new Vector3Int(Random.Range(0, mapWidth), Random.Range(0, mapHeight), 0);

                if (mapGenerator.wallsTilemap.HasTile(seedPosition))
                {
                    foundValidTile = true;
                    break;
                }
                attempts++;
            }

            if (!foundValidTile)
            {
                Debug.LogWarning("No valid tile found for ore cluster after max attempts.");
                continue; // Saltar esta iteración si no se encontró un tile válido
            }

            TileBase oreType = null;
            switch (mapGenerator.wallsTilemap.GetTile(seedPosition).name)
            {
                case "DesertWallTile": { oreType = oreTilesDesert[Random.Range(0, oreTilesDesert.Length)]; } break;
                case "GroundWallTile": { oreType = oreTilesGround[Random.Range(0, oreTilesGround.Length)]; } break;
                case "SnowWallTile": { oreType = oreTilesSnow[Random.Range(0, oreTilesSnow.Length)]; } break;
                default:{
                        oreType = oreTilesDesert[Random.Range(0, oreTilesDesert.Length)]; 
                        Debug.Log("Didn't found wall tile type: "+ mapGenerator.wallsTilemap.GetTile(seedPosition).name+" put oretiledesert by default"); 
                    }break;
            }
            CreateOreCluster(seedPosition, oreType);
        }
    }
    void CreateOreCluster(Vector3Int start, TileBase oreType)
    {
        int clusterSize = Random.Range(minClusterSize, maxClusterSize);
        Queue<Vector3Int> openSet = new Queue<Vector3Int>();
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();

        openSet.Enqueue(start);

        while (openSet.Count > 0 && closedSet.Count < clusterSize)
        {
            Vector3Int current = openSet.Dequeue();
            if (closedSet.Contains(current) || !mapGenerator.wallsTilemap.HasTile(current)) continue;

            oreTilemap.SetTile(current, oreType);
            if (!tileHealthData.ContainsKey(current))
            {
                tileHealthData.Add(current, new TileData(current, oreType, TileData.tileType.ore));
            }
            closedSet.Add(current);

            foreach (Vector3Int neighbor in GetNeighbors(current))
            {
                if (!closedSet.Contains(neighbor) && Random.value > 0.3f) // Probabilidad de expansión
                {
                    openSet.Enqueue(neighbor);
                }
                
            }
        }
    }
    List<Vector3Int> GetNeighbors(Vector3Int position)
    {
        return new List<Vector3Int>
        {
            position + Vector3Int.right,
            position + Vector3Int.left,
            position + Vector3Int.up,
            position + Vector3Int.down
        };
    }
    
}
