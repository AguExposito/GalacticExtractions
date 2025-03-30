using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class DrillController : MonoBehaviour
{
    public int cellsToDrill = 10;
    public int extractionDamage= 10;
    public float timePerDmgTick= 0.2f;
    public float tubeTime;
    public GameObject drillHead;
    public GameObject drillTubePivot;
    Vector3Int nextCellPos;
    Vector3 initialHeadScale;
    bool isExtending;

    Tilemap oreTilemap;
    Tilemap wallsTilemap;
    DamageTile damageTile;
    Coroutine dmgCoroutine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        oreTilemap = GameObject.FindGameObjectWithTag("OreTilemap").GetComponent<Tilemap>();
        wallsTilemap = GameObject.FindGameObjectWithTag("WallsTilemap").GetComponent<Tilemap>();
        damageTile = FindFirstObjectByType<DamageTile>();
        initialHeadScale = drillHead.transform.localScale;

        if (CheckForDrillingSpots() && gameObject.tag=="Building") //Building tag means that is instantiated
        {
            ExtendTube();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isExtending || gameObject.tag != "Building") { return; }

        if (damageTile.oreGenerator.oreTileData.ContainsKey(nextCellPos) || damageTile.mapGenerator.wallTileData.ContainsKey(nextCellPos))
        {
            if (dmgCoroutine == null)
            {
                dmgCoroutine=StartCoroutine(DealDamageOverTime(nextCellPos, extractionDamage, timePerDmgTick));
            }
        }
        else {
            StopCoroutine("DealDamageOverTime");
            dmgCoroutine=null;
            if (CheckForDrillingSpots())
            {
                ExtendTube();
            }
            else
            {
                isExtending = true;
                Debug.Log("No hay más Tiles para destruir");
            }
        }
    }

    bool CheckForDrillingSpots() {
        for (int i = 0; i < cellsToDrill; i++)
        {
            Vector3Int cellPosition = oreTilemap.WorldToCell(drillHead.transform.position)+Vector3Int.up*i;
            if (oreTilemap.HasTile(cellPosition) || wallsTilemap.HasTile(cellPosition)) {
                cellsToDrill-=i;
                nextCellPos = cellPosition;
                return true;
            }
        }
        return false;
    }

    void ExtendTube() {
        isExtending = true;
        Debug.Log("WaitForTubeExtension");
        StartCoroutine(WaitForTubeExtension());
    }

    IEnumerator WaitForTubeExtension()
    {
        Vector3Int cellPosition = oreTilemap.WorldToCell(drillHead.transform.position);
        int distance = (int)Mathf.Abs(cellPosition.y - nextCellPos.y);

        if (distance == 0) {
            isExtending = false; 
            Debug.Log("Distance==0"); 
            yield break; 
        }

        float totalTime = tubeTime * distance;
        float elapsedTime = 0f;

        Vector3 initialScale = drillTubePivot.transform.localScale;
        Vector3 targetScale = initialScale + new Vector3(0, distance, 0); // Solo escala en Y

        while (elapsedTime < totalTime)
        {
            drillTubePivot.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / totalTime);
            drillHead.transform.localScale = new Vector3(drillHead.transform.localScale.x, initialHeadScale.y / drillTubePivot.transform.localScale.y, drillHead.transform.localScale.z);
            elapsedTime += Time.deltaTime;
            yield return null; // Espera al siguiente frame
        }

        // Asegurar que llegue exactamente a la escala deseada
        drillTubePivot.transform.localScale = targetScale;
        isExtending = false;
        Debug.Log("Finished Smooth Tube Extension");
    }

    IEnumerator DealDamageOverTime(Vector3Int targetCell, int damagePerTick, float timePerTick)
    {
        while (damageTile.oreGenerator.oreTileData.ContainsKey(targetCell) ||
               damageTile.mapGenerator.wallTileData.ContainsKey(targetCell))
        {
            damageTile.DamageTileData(targetCell, damagePerTick);

            yield return new WaitForSeconds(timePerTick); // Espera antes de aplicar el siguiente daño
        }
        
    }

}
