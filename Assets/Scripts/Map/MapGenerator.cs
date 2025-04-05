using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("Tilemap Settings")]
    public Tilemap groundTilemap;
    public Tilemap wallsTilemap;
    public Tilemap bedrockTilemap;
    public TileBase bedrock;
    public TileBase[] groundTiles;
    public TileBase[] wallTiles;
    public Dictionary<TileBase, TileBase> biomeTiles = new Dictionary<TileBase, TileBase>();
    public Gradient gradientX;
    public Gradient gradientY;

    [Header("Terrain Settings")]
    public int width = 50;
    public int height = 20;
    public float scale = 10f;
    public float heightMultiplier = 5f;
    public int groundLevel = 10;
    public float caveThreshold = 0.4f;
        
    [Header("Biome Settings")]
    [Range(0.15f, 0.25f)]
    public float minBiomeSize = 0.2f; // 20% del mapa
    [Range(0.25f, 0.35f)]
    public float maxBiomeSize = 0.4f; // 40% del mapa
    [Range(0.01f, 0.2f)]
    public float noiseStrength = 0.4f; // 40% del mapa

    private float offsetX;
    private float offsetY;

    [Header("Surface Settings")]
    public float caveWidthMultiplier = 1.5f;

    public Dictionary<Vector3Int, TileData> wallTileData = new Dictionary<Vector3Int, TileData>();

    PlayerSpawner spawner;

    void Start()
    {
        AssignTilesToDictionary();
        RandomizeSeed();
        ShuffleTileBaseArray(groundTiles);
        RandomizeBiome();
        GenerateGround();
        GenerateTerrain();
        GenerateGrutas();
        GenerateTerrainLimits();
        spawner=FindAnyObjectByType<PlayerSpawner>();
        spawner.SpawnPlayer();
        groundTilemap.RefreshAllTiles();
        wallsTilemap.RefreshAllTiles();
    }
    //Asigna los tiles en un diccionario para no perder las referencias entre ellos
    void AssignTilesToDictionary() {
        for (int i = 0; i < wallTiles.Length; i++) {
            TileBase walltile = wallTiles[i];
            TileBase groundtile = groundTiles[i];
            biomeTiles.Add(groundtile, walltile);
        }
    }
    //Hace que cada mapa sea diferente
    void RandomizeSeed()
    {
        offsetX = Random.Range(0f, 10000f);
        offsetY = Random.Range(0f, 10000f);
    }
    //Genera las grutas/suelo en el terreno
    void GenerateGrutas()
    {
        int numGrutas = width / 8; // Número de grutas ajustado para buena distribución
        for (int i = 0; i < numGrutas; i++)
        {
            int startX = Random.Range(5, width - 5);
            int startY = groundLevel + Mathf.FloorToInt(Mathf.PerlinNoise(startX * 0.1f, offsetY) * 4f); // Iniciar en la superficie

            int maxDepth = Random.Range(height / 4, height / 3); // Limitar la profundidad para no eliminar la cueva principal
            int x = startX;
            int y = startY;

            for (int j = 0; j < maxDepth; j++)
            {
                // Solo eliminar si estamos en la superficie o ligeramente por debajo
                if (y >= groundLevel - 5)
                {
                    int caveWidth = Mathf.FloorToInt(Random.Range(1, 3) * caveWidthMultiplier); // Grutas más anchas

                    for (int w = -caveWidth; w <= caveWidth; w++)
                    {
                        wallsTilemap.SetTile(new Vector3Int(x + w, y, 0), null);
                    }
                }
                else
                {
                    break; // Detener la eliminación al llegar a la cueva
                }

                y--;
                if (Random.value > 0.5f) x += Random.Range(-1, 2);
            }
        }
    }
    //genera el terreno
    void GenerateTerrain()
    {
        wallsTilemap.ClearAllTiles();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var (groundTile, _) = DetermineBiome(x, y);
                TileBase wallTile = biomeTiles.ContainsKey(groundTile) ? biomeTiles[groundTile] : wallTiles[0];

                // Generar la altura del terreno base
                float noiseValue = Mathf.PerlinNoise((x + offsetX) / scale, offsetY) * heightMultiplier;
                int terrainHeight = Mathf.FloorToInt(groundLevel + noiseValue);

                if (y < terrainHeight)
                {
                    // Verificar si es parte de la superficie de las cuevas antes de generar cuevas
                    float caveNoise = Mathf.PerlinNoise((x + offsetX) / scale, (y + offsetY) / scale);
                    if (caveNoise > caveThreshold && wallsTilemap.GetTile(new Vector3Int(x, y, 0)) == null)
                    {
                        wallsTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);                        
                        wallTileData.Add(new Vector3Int(x, y, 0), new TileData(new Vector3Int(x, y, 0), wallTile, TileData.TileType.wall));
                    }
                }
            }
        }
    }
    void GenerateTerrainLimits() {
        for (int x = -3; x < width+2; x++) {
            for (int y = -3; y < height+2; y++)
            {
                if (x <= 0 || y <= 0 || x >= width-1 || y >= height-1) { 
                    bedrockTilemap.SetTile(new Vector3Int(x, y, 0), bedrock);
                    wallsTilemap.SetTile(new Vector3Int(x, y, 0),null);
                    groundTilemap.SetTile(new Vector3Int(x, y, 0),null);
                }
            }
        }
    }
    (TileBase wallTile, TileBase floorTile) DetermineBiome(int x, int y)
    {
        float normalizedX = Mathf.Clamp01((float)x / width);
        float normalizedY = Mathf.Clamp01((float)y / height);

        Color colorX = gradientX.Evaluate(normalizedX);
        Color colorY = gradientY.Evaluate(normalizedY);

        // Incorporar ruido Perlin en la mezcla
        float noise = Mathf.PerlinNoise(x * noiseStrength, y * noiseStrength);
        float blend = Mathf.Lerp(0f, 1f, noise);

        // Mezcla los colores de los gradientes de forma más orgánica
        Color mixedColor = Color.Lerp(colorX, colorY, blend);

        // Determinar el bioma más cercano al color generado
        int closestIndex = 0;
        float minDistance = float.MaxValue;
        for (int i = 0; i < groundTiles.Length; i++)
        {
            float distance = Vector3.Distance(new Vector3(mixedColor.r, mixedColor.g, mixedColor.b),
                                               new Vector3(gradientX.colorKeys[i].color.r, gradientX.colorKeys[i].color.g, gradientX.colorKeys[i].color.b));
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }
        return (groundTiles[closestIndex], wallTiles[closestIndex]);
    }

    void RandomizeBiome()
    {
        GradientColorKey[] colorKeysX = new GradientColorKey[groundTiles.Length];
        GradientColorKey[] colorKeysY = new GradientColorKey[groundTiles.Length];
        Color[] colors = new Color[groundTiles.Length];

        float[] biomeSizes = new float[groundTiles.Length];
        float remainingSpace = 1f;

        // Distribuir tamaños de biomas asegurando que respeten los mínimos
        for (int i = 0; i < groundTiles.Length; i++)
        {
            float maxAvailable = Mathf.Min(maxBiomeSize, remainingSpace - (minBiomeSize * (groundTiles.Length - (i + 1))));
            biomeSizes[i] = Random.Range(minBiomeSize, maxAvailable);
            remainingSpace -= biomeSizes[i];
        }

        // Asegurar que el último bioma use el espacio restante
        biomeSizes[groundTiles.Length - 1] += remainingSpace;

        //float currentPositionX = 0f;
        //float currentPositionY = 0f;
        float currentPosition = 0f;
        for (int i = 0; i < groundTiles.Length; i++)
        {
            colors[i] = Random.ColorHSV();
            //float timeX = currentPositionX + Random.Range(biomeSizes[i] / 3, biomeSizes[i] * (1 - minBiomeSize));
            //float timeY = currentPositionY + Random.Range(biomeSizes[i] / 3, biomeSizes[i] * (1 - minBiomeSize));
            float time = Mathf.Lerp(currentPosition, currentPosition + biomeSizes[i], Mathf.PerlinNoise(i * 0.1f, offsetY));

            colorKeysX[i] = new GradientColorKey(colors[i], Mathf.Clamp01(time));
            colorKeysY[i] = new GradientColorKey(colors[i], Mathf.Clamp01(time));

            //currentPositionX += biomeSizes[i];
            //currentPositionY += biomeSizes[i];
            currentPosition += biomeSizes[i];
        }

        gradientX.colorKeys = colorKeysX;
        gradientY.colorKeys = colorKeysY;
    }
    void GenerateGround()
    {
        groundTilemap.ClearAllTiles();
        for (int x = 0; x < width; x++)
        {
            int ySurface = Mathf.Clamp(groundLevel + Mathf.FloorToInt(Mathf.PerlinNoise((x + offsetX) / scale, offsetY) * heightMultiplier),0, height - 1);

            for (int y = 0; y < ySurface; y++) // Se genera el suelo hasta la superficie del terreno
            {
                var (floorTile, _) = DetermineBiome(x, y);
                groundTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }
    }
    void ShuffleTileBaseArray(TileBase[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);  // Genera un índice aleatorio
            TileBase temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}
