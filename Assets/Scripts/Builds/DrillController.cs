using System;
using System.Collections;
using System.Drawing;
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
    public GameObject drillTube;
    Vector3Int nextCellPos;
    Vector3 initialHeadPos;
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
        initialHeadPos = drillHead.transform.position;

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
        if (isExtending) return;
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

        Vector3 initialPos = drillHead.transform.position;
        Vector3 targetPos = initialPos + new Vector3(0, distance, 0); // Solo escala en Y


        SpriteRenderer sr = drillTube.GetComponent<SpriteRenderer>();
        Vector2 srSize = sr.size;
        float initialTubeHeight = srSize.y;

        while (elapsedTime < totalTime)
        {
            drillHead.transform.position = Vector3.Lerp(initialPos, targetPos, elapsedTime / totalTime);

            float currentHeight = Mathf.Lerp(initialPos.y, targetPos.y, elapsedTime / totalTime);
            //srSize = new Vector2(srSize.x, Mathf.Abs(currentHeight - initialPos.y)); // Usar la distancia entre la posición inicial y la actual
            srSize.y = initialTubeHeight+ Mathf.Abs(currentHeight - initialPos.y);
            sr.size = srSize;
            elapsedTime += Time.deltaTime;
            yield return null; // Espera al siguiente frame
        }

        // Asegurar que llegue exactamente a la escala deseada
        drillHead.transform.position = targetPos;
        srSize.y= initialTubeHeight+Mathf.Abs(targetPos.y - initialPos.y);
        sr.size = srSize;
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
