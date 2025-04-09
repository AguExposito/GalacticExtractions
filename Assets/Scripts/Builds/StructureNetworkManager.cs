using System.Collections.Generic;
using UnityEngine;

public class StructureNetworkManager : MonoBehaviour
{
    public static StructureNetworkManager Instance { get; private set; }

    public List<Building> allBuildings = new List<Building>();
    public List<Building> stationBuildings = new List<Building>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void RegisterBuilding(Building b)
    {
        if (!allBuildings.Contains(b))
            allBuildings.Add(b);
        if (b.CompareTag("EnergyStorage")) {
            stationBuildings.Add (b);
        }
    }

    public void UnregisterBuilding(Building b)
    {
        allBuildings.Remove(b);
    }

    public void RecalculateNetworks()
    {
        // Reset all statuses
        foreach (var b in allBuildings)
        {
            if(!b.CompareTag("EnergyStorage"))
            b.ResetNetworkFlags();
        }

        // Energía
        foreach (var b in allBuildings)
        {
            if (b.IsEnergySource())
                Propagate(b, supplyType: "energy");
        }

        // Storage
        foreach (var b in allBuildings)
        {
            if (b.IsStorageSource())
                Propagate(b, supplyType: "storage");
        }
    }

    private void Propagate(Building origin, string supplyType)
    {
        Queue<Building> queue = new Queue<Building>();
        HashSet<Building> visited = new HashSet<Building>();
        queue.Enqueue(origin);
        visited.Add(origin);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            //if (current!=origin)
            //{
            //    if (supplyType == "energy" && !current.hasEnergy)
            //        current.EnableEnergy(true);
            //    else if (supplyType == "storage" && !current.hasStorage)
            //        current.EnableStorage(true);
            //}

            foreach (var neighborGO in current.GetConnectedBuildings())
            {
                var neighbor = neighborGO.GetComponent<Building>();
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    if (current.hasEnergy)
                    {
                         neighbor.EnableEnergy(true);
                    }

                    if (current.hasStorage)
                    {
                        neighbor.EnableStorage(true);
                    }


                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
    }
}
