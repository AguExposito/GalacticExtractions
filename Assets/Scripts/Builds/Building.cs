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

    void Awake()
    {
        StructureNetworkManager.Instance?.RegisterBuilding(this);
    }

    void OnDestroy()
    {
        StructureNetworkManager.Instance?.UnregisterBuilding(this);
    }

    public void OnDestroyBuilding()
    {
        foreach (Building station in StructureNetworkManager.Instance?.stationBuildings)
        {
            StructureNetworkManager.Instance?.RecalculateNetworksFromStation();
        }
    }

    public void DetectNearbyStructures(bool selfConnection = false)
    {
        if (searchRadius == Vector2.zero) return;

        int buildingsLayerMask = 1 << 8;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, searchRadius, 0f, buildingsLayerMask);

        foreach (Collider2D collider in colliders)
        {
            if (!selfConnection && collider.gameObject == gameObject) continue;
            if (gameObject.tag == "Drill" && collider.tag == "Drill") continue;

            Building otherBuilding = collider.GetComponent<Building>();
            if (otherBuilding == null) continue;

            Vector2 offset = collider.transform.position - transform.position;
            Vector2 otherRadius = otherBuilding.searchRadius;

            if (Mathf.Abs(offset.x) > otherRadius.x / 2f || Mathf.Abs(offset.y) > otherRadius.y / 2f)
                continue;

            if (collider.TryGetComponent<DrillController>(out DrillController drillController))
            {
                connectionManager.CreateNewConnection(collider, gameObject, drillController.drilling, BuildingConnectionType.Default, true);
            }
            else
            {
                DetermineConnectionMat(collider);
            }
        }

        // Recalcula las redes globales (energía y almacenamiento)
        StructureNetworkManager.Instance?.RecalculateNetworks();
    }

    void DetermineConnectionMat(Collider2D collider)
    {
        switch (collider.tag)
        {
            case "Energy":
                connectionManager.CreateNewConnection(collider, gameObject, OreNames.Default, BuildingConnectionType.Energy, true);
                break;
            default:
                connectionManager.CreateNewConnection(collider, gameObject, OreNames.Default);
                break;
        }
    }

    public void EnableEnergy(bool state)
    {
        hasEnergy = state;
        if (state) onEnergyEnabled.Invoke();
        else onEnergyDisabled.Invoke();
        if (gameObject.TryGetComponent<Animator>(out Animator animator)) {
            animator.SetBool("SwitchActiveState", state);
        }
    }

    public void EnableStorage(bool state)
    {
        hasStorage = state;
        if (state) onStorageEnabled.Invoke();
        else onStorageDisabled.Invoke();
        if (gameObject.TryGetComponent<Animator>(out Animator animator))
        {
            animator.SetBool("SwitchActiveState", state);
        }
    }

    public void ResetNetworkFlags()
    {
        EnableEnergy(false);
        EnableStorage(false);
    }

    public bool IsEnergySource()
    {
        return CompareTag("EnergyStorage") || CompareTag("Energy");
    }

    public bool IsStorageSource()
    {
        return CompareTag("Storage") || CompareTag("EnergyStorage");
    }

    public List<GameObject> GetConnectedBuildings()
    {
        return connectionManager.GetConnectedStructures(gameObject);
    }
}
