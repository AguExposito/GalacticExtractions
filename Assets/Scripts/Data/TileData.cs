using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
public enum OreType { basic, soft, semihard, hard }
public enum OreNames { Default, Fliotex, Polarnyx, Trevonita }
public class OreData {
    public OreType ore;
    public OreNames oreName;

    public int oreAmount;
    public int extractedOres;
    public int healthPerOre;
}
public class TileData:OreData
{
    public enum TileType { wall, ore }
    public enum BiomeType { ground ,dirt, desert, snow }

    public Vector3Int position;
    public TileBase tile;
    public string tileName;
    public int health;
    public int maxHealth;
    public TileType type;
    public BiomeType biome;
    public GameObject biomeResources;
    public TextMeshProUGUI biomeResourcesText;
    public MapGenerator mapGenerator;
    public ResourceManager resourceManager;

    public TileData(Vector3Int pos, TileBase tileBase, TileType tileType)
    {
        position = pos;
        tile = tileBase;
        tileName = tileBase.name;
        type = tileType;

        AssignBiomeAndOreType();

        resourceManager = GameObject.FindFirstObjectByType<ResourceManager>();

        switch (this.ore)
        {
            case OreType.soft: { health = 100; oreAmount = Random.Range(75,150); } break;
            case OreType.semihard: { health = 200; oreAmount = Random.Range(50, 100); } break;
            case OreType.hard: { health = 500; oreAmount = Random.Range(25, 50); } break;
            case OreType.basic: { health = 5; } break;
            default: { Debug.Log("Didn't found wall tile type: " + tileBase.name + "hp set to 5"); health = 5; } break;
        }
        maxHealth = health;
        if(oreAmount!=0) healthPerOre = maxHealth/oreAmount;
    }

    bool AssignBiomeAndOreType() {
        mapGenerator = GameObject.FindFirstObjectByType<MapGenerator>();
        TileBase biomeGroundTile=null;
        if (type == TileType.wall)
        {
            biomeGroundTile = mapGenerator.biomeTiles.FirstOrDefault(x => x.Value == tile).Key; //Se obtiene la key del diccionario a traves del valor
        }
        else if (type == TileType.ore)
        {
            mapGenerator.wallTileData.TryGetValue(position,out TileData walltile); //Se navega a este diccionario para obtener la wall debajo del ore
            biomeGroundTile = mapGenerator.biomeTiles.FirstOrDefault(x => x.Value == walltile.tile).Key; //Se obtiene la key del diccionario a traves del valor
        }
        
        biomeResources = GameObject.FindGameObjectWithTag("BiomeResourcesHUD");

        switch (biomeGroundTile.name) {
            case "GroundTile": {
                    biomeResources = biomeResources.transform.GetChild(0).gameObject;
                    biome = BiomeType.ground; 
                } break;
            case "DirtTile": {
                    biomeResources = biomeResources.transform.GetChild(1).gameObject;
                    biome = BiomeType.dirt; 
                } break;
            case "DesertTile": {
                    biomeResources = biomeResources.transform.GetChild(2).gameObject;
                    biome = BiomeType.desert; 
                } break;
            case "SnowTile": {
                    biomeResources = biomeResources.transform.GetChild(3).gameObject;
                    biome = BiomeType.snow; 
                } break;
            default: {
                    biomeResources = biomeResources.transform.GetChild(0).gameObject;
                    Debug.LogWarning("SE AUTOASIGNO BIOMA A GROUND, REVISAR SI FALTA AÑADIR UN BIOMA AL SWITCH"); 
                    biome = BiomeType.ground; 
                } break;
        }        


        for (int i = 0; i < biomeResources.transform.childCount; i++)
        {
            switch (tileName)
            {
                case "fliotexTile": {
                        if (biomeResources.transform.GetChild(i).name == "Fliotex")
                        {
                            biomeResourcesText= biomeResources.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
                            ore = OreType.soft;
                            oreName = OreNames.Fliotex;
                        }
                    } break;
                case "polarnyxTile": {
                        if (biomeResources.transform.GetChild(i).name == "Polarnyx")
                        {
                            biomeResourcesText = biomeResources.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
                            ore = OreType.semihard;
                            oreName = OreNames.Polarnyx;
                        }
                    } break;
                case "trevonitaTile": {
                        if (biomeResources.transform.GetChild(i).name == "Trevonita")
                        {
                            biomeResourcesText = biomeResources.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
                            ore = OreType.hard;
                            oreName = OreNames.Trevonita;
                        }
                    } break;
                default: {
                        biomeResourcesText = biomeResources.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
                        ore = OreType.basic;
                        oreName = OreNames.Default;
                    } break;
            }
        }
        return true;
    }

    public int ExtractOre()
    {
        if (extractedOres >= oreAmount || health <= 0 || type == TileType.wall) return 0; // Ya está agotado

        int expectedExtractedOres = oreAmount - Mathf.FloorToInt((health / (float)maxHealth) * oreAmount);

        int newExtractedOres = expectedExtractedOres - extractedOres;
        extractedOres = expectedExtractedOres;
        Debug.Log("ExtractedOres: " + newExtractedOres + " ExtractedOresTotal" + extractedOres);

        resourceManager.SetResources(newExtractedOres, biomeResourcesText);

        return newExtractedOres;
    }
    //Use mapgenerator dictionary to get tiledata
    public TileData GetTileData(Vector3Int pos) {
        if (this.position == pos) { 
            return this;
        }
        return null;
    }
    
}
