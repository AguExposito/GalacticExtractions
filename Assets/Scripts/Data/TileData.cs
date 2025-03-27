using UnityEngine;
using UnityEngine.Tilemaps;

public class TileData
{
    public enum tileType { wall, ore }
    public enum oreType { basic ,soft, semihard, hard }
    public Vector3Int position;
    public TileBase tile;
    public int health;
    public int maxHealth;
    public tileType type;
    public oreType ore;

    public TileData(Vector3Int pos, TileBase tileBase, tileType tileType)
    {
        position = pos;
        tile = tileBase;
        type = tileType;

        switch (tileBase.name)
        {
            case "fliotexTile": { ore = oreType.soft; } break;
            case "polarnyxTile": { ore = oreType.semihard; } break;
            case "trevonitaTile": { ore = oreType.hard; } break;
            default: { ore = oreType.basic; } break;
        }

        switch (this.ore)
        {
            case oreType.soft: { health = 100; } break;
            case oreType.semihard: { health = 200; } break;
            case oreType.hard: { health = 500; } break;
            case oreType.basic: { health = 5; } break;
            default: { Debug.Log("Didn't found wall tile type: " + tileBase.name + "hp set to 5"); health = 5; } break;
        }
        maxHealth = health;
    }
    //Use mapgenerator dictionary
    public TileData GetTileData(Vector3Int pos) {
        if (this.position == pos) { 
            return this;
        }
        return null;
    }
    
}
