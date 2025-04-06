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

    public void DetectNearbyStructures(bool selfConnection=false)
    {
        if (searchRadius == Vector2.zero) return; //Esto evita que una estructura con rango 0 intente buscar conexiones.

        int buildingsLayerMask = 1 << 8; //Es lo mismo que LayerMask.GetMask("BuildingsLayer");

        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, searchRadius, 0f, buildingsLayerMask);
        foreach (Collider2D collider in colliders)
        {
            if (gameObject.tag == "Drill" && collider.tag=="Drill") continue; //Esto conexiones taladro-taladro
            if (!selfConnection)
                if (collider.gameObject == gameObject) continue; // Evitar auto-conexión

            Building otherBuilding = collider.GetComponent<Building>();
            if (otherBuilding == null) continue;

            Vector2 offset = collider.transform.position - transform.position;
            Vector2 otherRadius = collider.GetComponent<Building>().searchRadius;

            if (Mathf.Abs(offset.x) > otherRadius.x / 2f || Mathf.Abs(offset.y) > otherRadius.y / 2f )
            {
                continue; // No está dentro del rango cuadrado del otro objeto
            }


            if (collider.TryGetComponent<DrillController>(out DrillController drillController))
            {
                connectionManager.CreateNewConnection(collider, gameObject, drillController.drilling, true);
            }
            else
            {
                connectionManager.CreateNewConnection(collider, gameObject, OreNames.Default);
            }
            DefineVariableStates(collider);
        }

    }
}
