using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BuildingConnectionType { Energy, Storage, Default}

[System.Serializable]
public class KeyValuePairSerializable<TKey, TValue>
{
    public TKey Key;
    public TValue Value;

    public KeyValuePairSerializable(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }

    public TValue GetValue(TKey key)
    {
        TValue retValue= default(TValue);
        if (Key.ToString() == key.ToString())
        {
            retValue = Value;
        }
        if (key is GameObject gameObject && Key is GameObject gameObjectKey) {
            if (gameObjectKey.GetInstanceID() == gameObject.GetInstanceID())
            {
                retValue= Value;  // Si la clave coincide, devuelve el valor
            }
        }
        return retValue; 
    }

    public TKey GetKey(TValue value)
    {
        TKey retKey = default(TKey);
        if (Value.ToString() == value.ToString())
        {
            retKey= Key;
        }
        if (value is GameObject gameObject && value is GameObject gameObjectValue)
        {
            if (gameObject.GetInstanceID() == gameObjectValue.GetInstanceID())
            {
                retKey= Key;  // Si la clave coincide, devuelve el valor
            }
        }
        return retKey;
    }
    public void SetKey(TKey newKey)
    {
        Key = newKey;
    }

    // M�todo para establecer un nuevo valor
    public void SetValue(TValue newValue)
    {
        Value = newValue;
    }
}
public class ConnectionsManager : MonoBehaviour
{
    public List<KeyValuePairSerializable<OreNames,Material>> connectionMat = new List<KeyValuePairSerializable<OreNames,Material>>();
    public List<KeyValuePairSerializable<BuildingConnectionType,Material>> buildingConnectionMat = new List<KeyValuePairSerializable<BuildingConnectionType, Material>>();
    public List<KeyValuePairSerializable<GameObject,GameObject>> drillStationConnections = new List<KeyValuePairSerializable<GameObject, GameObject>>();


    public void CreateNewConnection(Collider2D collider, GameObject currentGO, OreNames ore = OreNames.Default, BuildingConnectionType buildingConnectionType = BuildingConnectionType.Default, bool isInverse = false)
    {
        // Check if connection already exists
        if (ConnectionExists(collider.gameObject, currentGO))
        {
            return;
        }

        // Create a new object for the connection
        GameObject lrContainer = new GameObject("ConnectionLine");
        lrContainer.transform.SetParent(transform);

        LineRenderer lr = lrContainer.AddComponent<LineRenderer>();
        lr.startWidth = 0.15f;
        lr.endWidth = 0.15f;
        lr.sortingOrder = 8;
        lr.positionCount = 2;

        // Determine correct direction based on building types
        Vector3[] points;
        bool shouldInverse = ShouldInverseConnection(collider.gameObject, currentGO);
        
        if (shouldInverse) {
            points = new Vector3[]
            {
                currentGO.transform.position,
                collider.gameObject.transform.position
            };
        }
        else
        {
            points = new Vector3[]
            {
                collider.gameObject.transform.position,
                currentGO.transform.position
            };
        }
        
        lr.SetPositions(points);

        // Calculate direction for shader
        Vector3 direction = (points[1] - points[0]).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Assign material based on connection type
        Material connectionMaterial;
        if (buildingConnectionType == BuildingConnectionType.Default)
        {
            connectionMaterial = connectionMat.Find(kvp => kvp.Key.Equals(ore))?.Value;
        }
        else 
        {
            connectionMaterial = buildingConnectionMat.Find(kvp => kvp.Key.Equals(buildingConnectionType))?.Value;
        }

        if (connectionMaterial != null)
        {
            Material instanceMaterial = new Material(connectionMaterial);
            // Rotar el material para que coincida con la dirección de la conexión
            instanceMaterial.SetFloat("_Rotation", -angle); // Negamos el ángulo para que coincida con la dirección correcta
            // Ajustar el tiling para que la textura se repita correctamente
            float distance = Vector3.Distance(points[0], points[1]);
            instanceMaterial.SetFloat("_Tiling", distance * 2); // Multiplicamos por 2 para que la textura se repita más veces
            lr.material = instanceMaterial;
        }

        // Save the connection
        drillStationConnections.Add(new KeyValuePairSerializable<GameObject, GameObject>(collider.gameObject, lrContainer));
        drillStationConnections.Add(new KeyValuePairSerializable<GameObject, GameObject>(currentGO, lrContainer));
    }

    private bool ConnectionExists(GameObject building1, GameObject building2)
    {
        var connections1 = GetAllConnections(building1);
        var connections2 = GetAllConnections(building2);
        
        return connections1.Any(conn1 => connections2.Contains(conn1));
    }

    private bool ShouldInverseConnection(GameObject building1, GameObject building2)
    {
        Building b1 = building1.GetComponent<Building>();
        Building b2 = building2.GetComponent<Building>();
        
        if (b1 == null || b2 == null) return false;

        // Energy buildings should be the source
        if (b1.structureType == Building.StructureType.Energy && b2.structureType == Building.StructureType.Drill)
            return false;
        if (b2.structureType == Building.StructureType.Energy && b1.structureType == Building.StructureType.Drill)
            return true;

        // Storage buildings should be the destination for drills
        if (b1.structureType == Building.StructureType.Drill && 
            (b2.structureType == Building.StructureType.Storage || b2.structureType == Building.StructureType.EnergyStorage))
            return false;
        if (b2.structureType == Building.StructureType.Drill && 
            (b1.structureType == Building.StructureType.Storage || b1.structureType == Building.StructureType.EnergyStorage))
            return true;

        // Default case: use the original isInverse parameter
        return false;
    }

    public void AssignConnectionMaterial(GameObject structure, OreNames ore = OreNames.Default, BuildingConnectionType buildingConnectionType = BuildingConnectionType.Default)
    {
        if (structure == null) return;

        Material mat;
        if (buildingConnectionType == BuildingConnectionType.Default)
        {
            mat = connectionMat.Find(kvp => kvp.Key.Equals(ore))?.Value;
        }
        else 
        {
            mat = buildingConnectionMat.Find(kvp => kvp.Key.Equals(buildingConnectionType))?.Value;
        }

        if (mat == null) return;

        List<GameObject> connections = GetAllConnections(structure);
        foreach (GameObject conn in connections)
        {
            if (conn == null || conn.Equals(null)) continue;

            LineRenderer lr = conn.GetComponent<LineRenderer>();
            if (lr != null)
            {
                // Calculate direction for shader
                Vector3[] positions = new Vector3[2];
                lr.GetPositions(positions);
                Vector3 direction = (positions[1] - positions[0]).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                // Create a new material instance
                Material instanceMaterial = new Material(mat);
                // Rotar el material para que coincida con la dirección de la conexión
                instanceMaterial.SetFloat("_Rotation", -angle); // Negamos el ángulo para que coincida con la dirección correcta
                // Ajustar el tiling para que la textura se repita correctamente
                float distance = Vector3.Distance(positions[0], positions[1]);
                instanceMaterial.SetFloat("_Tiling", distance * 2); // Multiplicamos por 2 para que la textura se repita más veces
                lr.material = instanceMaterial;
            }
        }
    }

    public List<GameObject> GetAllConnections(GameObject structure)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (var kvp in drillStationConnections)
        {
            if (kvp.Key == structure)
            {
                result.Add(kvp.Value);
            }
        }
        return result;
    }
    public List<GameObject> GetConnectedStructures(GameObject structure)
    {
        return drillStationConnections
            .Where(kvp => kvp.Key == structure)
            .Select(kvp => kvp.Value)
            .Where(conn => conn != null)
            .Select(conn =>
                drillStationConnections.FirstOrDefault(kvp => kvp.Value == conn && kvp.Key != structure)?.Key
            )
            .Where(other => other != null)
            .ToList();
    }


    public void ClearConnections(GameObject structure)
    {
        if (structure == null) return;

        var toRemove = drillStationConnections
            .Where(kvp => kvp.Key == structure)
            .ToList();

        foreach (var connection in toRemove)
        {
            GameObject connectionLine = connection.Value;
            if (connectionLine != null)
            {
                // Remove all references to this connection
                drillStationConnections.RemoveAll(kvp => kvp.Value == connectionLine);
                Destroy(connectionLine);
            }
        }
    }

}


