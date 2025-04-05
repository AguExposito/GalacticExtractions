using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;

public class StationController : Building
{
    public GameObject effectRange;
    public Material storeCable;
    public List<GameObject> connections = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (gameObject.tag == "Instantiated") 
        {
            gameObject.tag = "EnergyStorage";
            BoxCollider2D boxCollider2D = effectRange.AddComponent<BoxCollider2D>();
            boxCollider2D.size = new Vector2(searchRadius.x, searchRadius.y);
            boxCollider2D.isTrigger = true;
        }
        LineRenderer lr= effectRange.GetComponent<LineRenderer>();
        SetLRCorners(lr);
        DetectNearbyStructures();
    }

    Vector3[] GetLRCorners(LineRenderer lr) {
        Vector3[] corners = new Vector3[lr.positionCount];
        for (int i = 0; i < lr.positionCount; i++)
        {
            corners[i] = lr.GetPosition(i);
        }
        return corners;
    }

    void SetLRCorners(LineRenderer lr) {
        float rangeX = ((float)searchRadius.x / 2);
        float rangeY = ((float)searchRadius.y / 2);
        Vector3[] corners = new Vector3[]
        {
            new Vector3(-rangeX,-rangeY,0),//BL
            new Vector3(rangeX,-rangeY,0),//BR
            new Vector3(rangeX,rangeY,0),//TR
            new Vector3(-rangeX,rangeY,0),//TL
        };
        lr.SetPositions(corners);
    }

    public void DetectNearbyStructures() {
        int buildingsLayerMask = 1 << 8; //Es lo mismo que LayerMask.GetMask("BuildingsLayer");
        Collider2D[] colliders = Physics2D.OverlapBoxAll(effectRange.transform.position,searchRadius,0f,buildingsLayerMask);
        foreach (Collider2D collider in colliders)
        {
            CreateNewConnection(collider);
            DefineVariableStates(collider);
        }

    }

    public void CreateNewConnection(Collider2D collider)
    {
        if (collider.gameObject == gameObject || collider.transform.parent.gameObject == gameObject) return; // No conectar consigo mismo

        // Crear un nuevo objeto para la conexión
        GameObject lrContainer = new GameObject("ConnectionLine");
        lrContainer.transform.SetParent(transform);

        LineRenderer lr = lrContainer.AddComponent<LineRenderer>();
        lr.startWidth = 0.15f;
        lr.endWidth = 0.15f;
        lr.material = storeCable;
        lr.sortingOrder = 8;
        lr.positionCount = 2;

        // Definir los puntos de conexión
        Vector3[] points = new Vector3[]
        {
                transform.position,
                collider.gameObject.transform.position
        };
        lr.SetPositions(points);

        // Guardar la conexión en la lista
        connections.Add(lrContainer);
    }

    void ClearConnections()
    {
        foreach (GameObject connection in connections)
        {
            Destroy(connection);
        }
        connections.Clear();
    }
}
