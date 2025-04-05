using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
}
public class ConnectionsManager : MonoBehaviour
{
    public List<KeyValuePairSerializable<OreNames,Material>> connectionMat = new List<KeyValuePairSerializable<OreNames,Material>>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}


