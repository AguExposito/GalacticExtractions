using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public List<KeyValuePairSerializable<GameObject,GameObject>> drillStationConnections = new List<KeyValuePairSerializable<GameObject, GameObject>>();
    public List<GameObject> connections;

    public void CreateNewConnection(Collider2D collider, GameObject currentGO, OreNames ore, bool isInverse =false)
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
        KeyValuePairSerializable<GameObject, GameObject> newConnection = new KeyValuePairSerializable<GameObject, GameObject>(collider.gameObject, lrContainer);
        drillStationConnections.Add(newConnection);
        KeyValuePairSerializable<GameObject, GameObject> newConnection2 = new KeyValuePairSerializable<GameObject, GameObject>(currentGO, lrContainer);
        drillStationConnections.Add(newConnection2);

        AssignConnectionMaterial(ore, lrContainer);
    }

    public void AssignConnectionMaterial(OreNames ore, GameObject structure)
    {
        Material mat = connectionMat.Find(kvp => kvp.Key.Equals(ore))?.Value;
        if (mat == null) return;

        List<GameObject> connections = GetAllConnections(structure);
        foreach (GameObject conn in connections)
        {
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


    void ClearConnections(GameObject structure)
    {
        var toRemove = drillStationConnections
            .Where(kvp => kvp.Key == structure)
            .ToList();

        foreach (var connection in toRemove)
        {
            drillStationConnections.Remove(connection);
            if (connection.Value != null)
                Destroy(connection.Value);
        }
    }

}


