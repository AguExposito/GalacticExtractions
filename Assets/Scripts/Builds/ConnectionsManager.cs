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

    // Método para establecer un nuevo valor
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


    public void CreateNewConnection(Collider2D collider, GameObject currentGO, OreNames ore = OreNames.Default, BuildingConnectionType buildingConnectionType = BuildingConnectionType.Default, bool isInverse =false)
    {

        // Crear un nuevo objeto para la conexión
        GameObject lrContainer = new GameObject("ConnectionLine");
        lrContainer.transform.SetParent(transform);

        LineRenderer lr = lrContainer.AddComponent<LineRenderer>();
        lr.startWidth = 0.15f;
        lr.endWidth = 0.15f;
        lr.sortingOrder = 8;
        lr.positionCount = 2;
        lr.material = connectionMat[0].Value;

        // Definir los puntos de conexión

        Vector3[] points;
        if (isInverse) {
            points= new Vector3[]
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

        // Guardar la conexión nueva en la lista
        drillStationConnections.Add(new KeyValuePairSerializable<GameObject, GameObject>(collider.gameObject, lrContainer));
        drillStationConnections.Add(new KeyValuePairSerializable<GameObject, GameObject>(currentGO, lrContainer));
        
        if (buildingConnectionType == BuildingConnectionType.Default)
        {
            AssignConnectionMaterial(currentGO, ore);
        }
        else {
            AssignConnectionMaterial(currentGO, ore, buildingConnectionType);
        }
    }

    public void AssignConnectionMaterial(GameObject structure, OreNames ore = OreNames.Default, BuildingConnectionType buildingConnectionType = BuildingConnectionType.Default)
    {
        if (structure == null) return;
        Material mat;
        if (buildingConnectionType == BuildingConnectionType.Default)
        {
            mat = connectionMat.Find(kvp => kvp.Key.Equals(ore))?.Value;
        }
        else {
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
                lr.material = mat;
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

        // Filtramos todas las entradas donde la clave sea la estructura que se va a destruir
        var toRemove = drillStationConnections
            .Where(kvp => kvp.Key == structure)
            .ToList();

        foreach (var connection in toRemove)
        {
            GameObject connectionLine = connection.Value;
            // Buscar la otra estructura conectada a esta misma línea
            GameObject otherStructure = drillStationConnections
                .Where(kvp => kvp.Value == connectionLine && kvp.Key != structure)
                .Select(kvp => kvp.Key)
                .FirstOrDefault();


            //if (otherStructure != null)
            //{ 
            //    otherStructure.GetComponent<Building>().RemoveSupply(structure);
            //}


            drillStationConnections.Remove(connection);
            if (connection.Value != null)
                Destroy(connection.Value);
        }
    }

}


