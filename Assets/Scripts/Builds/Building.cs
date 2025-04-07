using System.Collections.Generic;
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
    public List<GameObject> energySupply = new List<GameObject>();
    public List<GameObject> storageSupply = new List<GameObject>();

    public void DefineVariableStates(Collider2D collider)
    {
        // Obtenemos la estructura fuente a partir del collider.
        Building sourceBuilding = collider.GetComponent<Building>();
        if (sourceBuilding == null) return;

        // Helper local para agregar de forma única
        void AddUnique(List<GameObject> list, GameObject go)
        {
            if (!list.Contains(go))
                list.Add(go);
        }

        // --- Parte 1: La estructura receptora (this) recibe estado del source ---
        switch (collider.tag)
        {
            case "EnergyStorage":
                {
                    // Si el source tiene energía, se activa la energía en this
                    if (sourceBuilding.hasEnergy)
                    {
                        AddUnique(energySupply, sourceBuilding.gameObject);
                        if (!hasEnergy)
                        {
                            hasEnergy = true;
                            EnableEnergy(true);
                        }
                    }
                    // Si el source tiene almacenamiento, se activa en this
                    if (sourceBuilding.hasStorage)
                    {
                        AddUnique(storageSupply, sourceBuilding.gameObject);
                        if (!hasStorage)
                        {
                            hasStorage = true;
                            EnableStorage(true);
                        }
                    }
                }
                break;

            case "Energy":
                {
                    if (sourceBuilding.hasEnergy)
                    {
                        AddUnique(energySupply, sourceBuilding.gameObject);
                        if (!hasEnergy)
                        {
                            hasEnergy = true;
                            EnableEnergy(true);
                        }
                    }
                }
                break;

            case "Storage":
                {
                    if (sourceBuilding.hasStorage)
                    {
                        AddUnique(storageSupply, sourceBuilding.gameObject);
                        if (!hasStorage)
                        {
                            hasStorage = true;
                            EnableStorage(true);
                        }
                    }
                }
                break;
        }

        // --- (Opcional) Parte 2: Transmisión de estado de this al source ---
        // Si querés que la conexión sea bidireccional (por ejemplo, para que si this es fuente se "transmita"
        // a la estructura detectada), podés habilitar este bloque.
        
        switch (tag)
        {
            case "EnergyStorage":
                {
                    if (hasEnergy)
                    {
                        AddUnique(sourceBuilding.energySupply, gameObject);
                        if (!sourceBuilding.hasEnergy)
                        {
                            sourceBuilding.hasEnergy = true;
                            sourceBuilding.EnableEnergy(true);
                        }
                    }
                    if (hasStorage)
                    {
                        AddUnique(sourceBuilding.storageSupply, gameObject);
                        if (!sourceBuilding.hasStorage)
                        {
                            sourceBuilding.hasStorage = true;
                            sourceBuilding.EnableStorage(true);
                        }
                    }
                }
                break;
            case "Energy":
                {
                    if (hasEnergy)
                    {
                        AddUnique(sourceBuilding.energySupply, gameObject);
                        if (!sourceBuilding.hasEnergy)
                        {
                            sourceBuilding.hasEnergy = true;
                            sourceBuilding.EnableEnergy(true);
                        }
                    }
                }
                break;
            case "Storage":
                {
                    if (hasStorage)
                    {
                        AddUnique(sourceBuilding.storageSupply, gameObject);
                        if (!sourceBuilding.hasStorage)
                        {
                            sourceBuilding.hasStorage = true;
                            sourceBuilding.EnableStorage(true);
                        }
                    }
                }
                break;
        }
       

        Debug.Log($"{gameObject.name} ({tag}) recibió de {sourceBuilding.name} ({collider.tag}) | hasEnergy: {hasEnergy}, hasStorage: {hasStorage}");
    }

    public bool HasSource()
    {
        // Un edificio "EnergyStorage" es fuente raíz
        if (tag == "EnergyStorage")
            return true;

        // Recorremos la lista de supply, y si alguno es fuente válida, también lo somos
        foreach (GameObject supplier in energySupply)
        {
            if (supplier == null) continue;

            Building supplierBuilding = supplier.GetComponent<Building>();
            if (supplierBuilding != null && supplierBuilding.HasSource())
            {
                return true;
            }
        }

        return false;
    }
    public void RefreshEnergyStatus()
    {
        if (!HasSource())
        {
            hasEnergy = false;
            EnableEnergy(false);
        }
    }

    public void RemoveSupply(GameObject supply) {
        if (energySupply.Count > 0)
        {
            foreach (GameObject item in energySupply)
            {
                if (item.GetInstanceID() == supply.GetInstanceID())
                {
                    energySupply.Remove(item);
                    if (energySupply.Count <= 0 && gameObject.tag != "EnergyStorage")
                    {
                        RefreshEnergyStatus();
                        hasEnergy = false;
                    }
                    break;
                }
            }
        }
        if (storageSupply.Count > 0)
        {
            foreach (GameObject item in storageSupply)
            {
                if (item.GetInstanceID() == supply.GetInstanceID())
                {
                    storageSupply.Remove(item);
                    if (storageSupply.Count <= 0 && gameObject.tag != "EnergyStorage")
                    {
                        RefreshEnergyStatus();
                        hasStorage = false;
                    }
                    break;
                }
            }
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
