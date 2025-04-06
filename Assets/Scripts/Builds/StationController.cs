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
        connectionManager = transform.parent.GetComponent<ConnectionsManager>();
        if (connectionManager == null)
        {
            connectionManager = FindFirstObjectByType<ConnectionsManager>();
        }

        BoxCollider2D boxCollider2D = effectRange.AddComponent<BoxCollider2D>();
        boxCollider2D.size = new Vector2(searchRadius.x, searchRadius.y);
        boxCollider2D.isTrigger = true;

        LineRenderer lr = effectRange.GetComponent<LineRenderer>();
        SetLRCorners(lr);

        if (gameObject.tag == "Instantiated")
        {
            gameObject.tag = "EnergyStorage";
            DetectNearbyStructures();
        }        
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
            if (collider.TryGetComponent<DrillController>(out DrillController drillController))
            {
                connectionManager.CreateNewConnection(collider, gameObject, drillController.drilling,true);
            }
            else {
                connectionManager.CreateNewConnection(collider, gameObject, OreNames.Default);
            }
            DefineVariableStates(collider);
        }

    }
}
