using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Building : MonoBehaviour
{
    public enum StructureType { Drill , Energy, Storage, EnergyStorage, Undefined}
    public StructureType structureType;
    public bool hasEnergy = false;
    public bool hasStorage = false;
    public Vector2 searchRadius = new Vector2();
    public GameObject effectReach;
    public ConnectionsManager connectionManager;
    public UnityEvent onEnergyEnabled;
    public UnityEvent onEnergyDisabled;
    public UnityEvent onStorageEnabled;
    public UnityEvent onStorageDisabled;

    void Awake()
    {
        StructureNetworkManager.Instance?.RegisterBuilding(this);
        if (connectionManager == null)
        {
            connectionManager = FindAnyObjectByType<ConnectionsManager>();
        }
    }

    void OnDestroy()
    {
        StructureNetworkManager.Instance?.UnregisterBuilding(this);
    }
    private void OnDisable()
    {
        StructureNetworkManager.Instance?.UnregisterBuilding(this);
    }

    public void OnDestroyBuilding()
    {
        foreach (Building station in StructureNetworkManager.Instance?.stationBuildings)
        {
            StructureNetworkManager.Instance?.RecalculateNetworksFromStation();
        }
        StructureNetworkManager.Instance?.UnregisterBuilding(this);
    }

    public void DetectNearbyStructures(bool selfConnection = false)
    {
        if (searchRadius == Vector2.zero) return;

        int buildingsLayerMask = 1 << 8;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, searchRadius, 0f, buildingsLayerMask);


        foreach (Collider2D collider in colliders)
        {
            Debug.Log("AAAAAAAAAAAA");
            if (!selfConnection && collider.gameObject == gameObject) continue;
            if (gameObject.tag == "Drill" && collider.tag == "Drill") continue;

            Building otherBuilding = collider.GetComponent<Building>();
            if (otherBuilding == null) continue;

            Debug.Log("BBBBBBBBBBBB");

            if (collider.TryGetComponent<DrillController>(out DrillController drillController))
            {
                connectionManager.CreateNewConnection(collider, gameObject, drillController.drilling, BuildingConnectionType.Default, true);
                
            }
            else
            {
                if (gameObject.tag == "Drill")
                {
                    Debug.Log("CCCCCCCCCCCC");
                    // Si soy drill, veo si el otro tiene reach y me intersecta
                    var otherReach = otherBuilding.effectReach?.GetComponent<BoxCollider2D>();
                    if (otherReach == null) continue;

                    if (!otherReach.bounds.Intersects(GetComponent<Collider2D>().bounds)) continue;
                    //Vector2 offset = collider.transform.position - transform.position;
                    //Vector2 otherRadius = otherBuilding.searchRadius;

                    //if (Mathf.Abs(offset.x) > otherRadius.x / 2f || Mathf.Abs(offset.y) > otherRadius.y / 2f)
                    //    continue;
                }
                else
                {
                    bool intersects = effectReach.GetComponent<BoxCollider2D>().bounds.Intersects(collider.bounds);
                    bool intersects2 = collider.GetComponent<Building>().effectReach.GetComponent<BoxCollider2D>().bounds.Intersects(gameObject.GetComponent<Collider2D>().bounds);
                    if (!intersects || !intersects2) continue;
                }
            }
            DetermineConnectionMat(collider);
            
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
            case "Storage":
                connectionManager.CreateNewConnection(collider, gameObject, OreNames.Default, BuildingConnectionType.Storage, true);
                break;
            default:
                connectionManager.CreateNewConnection(collider, gameObject, OreNames.Default, BuildingConnectionType.Default, true);
                break;
        }
    }

    public void EnableEnergy(bool state)
    {
        hasEnergy = state;
        if (state) onEnergyEnabled.Invoke();
        else onEnergyDisabled.Invoke();
        if (gameObject.TryGetComponent<Animator>(out Animator animator)) {
            switch (gameObject.tag) {
                case "Energy":
                    {
                        animator.SetBool("SwitchActiveState", state);
                    }
                    break;
                case "Storage":
                    {
                        //animator.SetBool("SwitchActiveState", state);
                    }
                    break;
                case "EnergyStorage":
                    {
                        //animator.SetBool("SwitchActiveState", state);
                    }
                    break;
                case "Drill":
                    {
                        //animator.SetBool("SwitchActiveState", state);
                    }
                    break;
                default:
                    {
                        //animator.SetBool("SwitchActiveState", state);
                    }
                    break;
            }
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
