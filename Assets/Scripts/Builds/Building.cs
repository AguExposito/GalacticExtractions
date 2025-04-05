using UnityEngine;

public class Building : MonoBehaviour
{
    public bool hasEnergy = false;
    public bool hasStorage = false;
    public Vector2 searchRadius = new Vector2();
    private void DefineVariableStates(Collider2D collider)
    {
        switch (collider.tag)
        {
            case "EnergyStorage": { hasEnergy = true; hasStorage = true; } break;
            case "Storage": { hasStorage = true; } break;
            case "Energy": { hasEnergy = true; } break;
            default: { Debug.Log("No coincidio con los tags definidos " + collider.tag); } break;
        }
    }
}
