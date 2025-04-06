using UnityEngine;
using UnityEngine.Events;

public class Building : MonoBehaviour
{
    public bool hasEnergy = false;
    public bool hasStorage = false;
    public Vector2 searchRadius = new Vector2();
    public ConnectionsManager connectionManager;
    public UnityEvent onEnergyEnabled;
    public UnityEvent onEnergyDisabled;
    public UnityEvent onStorageEnabled;
    public UnityEvent onStorageDisabled;

    public void DefineVariableStates(Collider2D collider)
    {
        Building currentBuilding = collider.GetComponent<Building>();
        switch (collider.tag)
        {
            case "EnergyStorage": 
                { 
                    hasEnergy = true; 
                    hasStorage = true; 
                    EnableEnergy(true);
                    EnableStorage(true);
                } break;
            case "Energy": 
                { 
                    hasEnergy = true; 
                    EnableEnergy(true); 
                } break;
            case "Storage": 
                { 
                    hasStorage = true;
                    EnableStorage(true); 
                } break;
        }
        switch (gameObject.tag)
        {
            case "EnergyStorage": 
                { 
                    currentBuilding.hasEnergy = true;
                    currentBuilding.hasStorage = true;
                    currentBuilding.EnableEnergy(true);
                    currentBuilding.EnableStorage(true);
                } break;
            case "Energy": 
                { 
                    currentBuilding.hasEnergy = true; 
                    currentBuilding.EnableEnergy(true); 
                } break;
            case "Storage": 
                { 
                    currentBuilding.hasStorage = true; 
                    currentBuilding.EnableStorage(true); 
                } break;
        }
    }

    public void EnableEnergy(bool state) {
        if (state) { onEnergyEnabled.Invoke(); }
        else { onEnergyDisabled.Invoke(); }
    }
    public void EnableStorage(bool state) {
        if (state) { onStorageEnabled.Invoke(); }
        else { onStorageDisabled.Invoke(); }
    }
}
