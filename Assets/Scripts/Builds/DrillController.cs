
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Tilemaps;

public class DrillController : Building
{
    public int cellsToDrill = 10;
    public int extractionDamage = 10;
    public float timePerDmgTick = 0.2f;
    public float tubeTime;
    public GameObject drillHead;
    public GameObject drillTube;
    [Space]
    public OreNames drilling;

    Vector3Int nextCellPos;
    Vector3 initialHeadPos;
    bool isExtending;
    bool isBeingDestroyed;
    Tilemap oreTilemap;
    Tilemap wallsTilemap;
    DamageTile damageTile;
    Coroutine dmgCoroutine;
    InputSystem_Actions controls;

    private void Awake()
    {
        // Aseguramos que el sistema de input esté inicializado antes de usarlo
        controls = new InputSystem_Actions();
        EnhancedTouchSupport.Enable();

        Vector3[] corners = new Vector3[]
        {
            new Vector3(-0.5f,-0.5f,0),//BL
            new Vector3(0.5f,-0.5f,0),//BR
            new Vector3(0.5f,0.5f+cellsToDrill,0),//TR
            new Vector3(-0.5f,0.5f+cellsToDrill,0),//TL
        };
        effectReach.GetComponent<LineRenderer>().SetPositions(corners);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        connectionManager = transform.parent.GetComponent<ConnectionsManager>();
        if (connectionManager == null)
        {
            connectionManager = FindFirstObjectByType<ConnectionsManager>();
        }

        oreTilemap = GameObject.FindGameObjectWithTag("OreTilemap").GetComponent<Tilemap>();
        wallsTilemap = GameObject.FindGameObjectWithTag("WallsTilemap").GetComponent<Tilemap>();
        damageTile = FindFirstObjectByType<DamageTile>();
        initialHeadPos = drillHead.transform.position;

        ExtendTube();

        if (gameObject.tag == "Instantiated")
        {
            gameObject.tag = structureType.ToString();
            DetectNearbyStructures();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isExtending || gameObject.tag != "Drill" ||  !hasEnergy || !hasStorage) { return; }

        if (damageTile.oreGenerator.oreTileData.ContainsKey(nextCellPos) || damageTile.mapGenerator.wallTileData.ContainsKey(nextCellPos))
        {
            if (dmgCoroutine == null)
            {
                dmgCoroutine = StartCoroutine(DealDamageOverTime(nextCellPos, extractionDamage, timePerDmgTick));
            }
            if (damageTile.oreGenerator.oreTileData.TryGetValue(nextCellPos, out TileData tileData))
            {
                drilling = tileData.oreName;
                if (connectionManager != null && this != null && !this.Equals(null))
                {
                    connectionManager.AssignConnectionMaterial(gameObject, drilling);
                }

            }
        }
        else
        {
            StopCoroutine("DealDamageOverTime");
            dmgCoroutine = null;
            drilling=OreNames.Default;
            if (connectionManager != null && this != null && !this.Equals(null))
            {
                connectionManager.AssignConnectionMaterial(gameObject, drilling);
            }

            ExtendTube();
        }
    }
    private void OnDestroy()
    {
        isBeingDestroyed = true;
        if (dmgCoroutine != null)
        {
            StopCoroutine(dmgCoroutine);
            dmgCoroutine = null;
        }
    }
    bool CheckForDrillingSpots()
    {
        for (int i = 0; i < cellsToDrill; i++)
        {
            Vector3Int cellPosition = oreTilemap.WorldToCell(drillHead.transform.position) + GetDrillDirection() * i;
            if (oreTilemap.HasTile(cellPosition) || wallsTilemap.HasTile(cellPosition))
            {
                cellsToDrill -= i;
                nextCellPos = cellPosition;
                return true;
            }
        }
        isExtending = true;
        Debug.Log("No hay más Tiles para destruir");
        return false;
    }
    Vector3Int GetDrillDirection()
    {
        float angle = transform.eulerAngles.z;

        if (angle >= 315 || angle < 45) return Vector3Int.up;      // Apunta hacia arriba
        if (angle >= 45 && angle < 135) return Vector3Int.left;    // Apunta hacia la izquierda
        if (angle >= 135 && angle < 225) return Vector3Int.down;    // Apunta hacia abajo
        if (angle >= 225 && angle < 315) return Vector3Int.right;   // Apunta hacia la derecha

        return Vector3Int.up; // Valor por defecto
    }

    public void ExtendTube()
    {
        if (isExtending || !hasEnergy || !hasStorage || !CheckForDrillingSpots()) return;
        isExtending = true;
        Debug.Log("WaitForTubeExtension");
        StartCoroutine(WaitForTubeExtension());
    }

    IEnumerator WaitForTubeExtension()
    {
        Vector3Int cellPosition = oreTilemap.WorldToCell(drillHead.transform.position);
        int distance = (int)Vector3.Distance(cellPosition, nextCellPos);

        Vector3Int drillDir = GetDrillDirection();

        if (distance == 0)
        {
            isExtending = false;
            Debug.Log("Distance==0");
            yield break;
        }

        float totalTime = tubeTime * distance;
        float elapsedTime = 0f;

        Vector3 initialPos = drillHead.transform.localPosition;
        Vector3 targetPos = initialPos + new Vector3(0, distance, 0); // Solo escala en Y


        SpriteRenderer sr = drillTube.GetComponent<SpriteRenderer>();
        Vector2 srSize = sr.size;
        float initialTubeHeight = srSize.y;

        while (elapsedTime < totalTime)
        {
            drillHead.transform.localPosition = Vector3.Lerp(initialPos, targetPos, elapsedTime / totalTime);

            float currentHeight = Mathf.Lerp(initialPos.y, targetPos.y, elapsedTime / totalTime);
            srSize.y = initialTubeHeight + Mathf.Abs(currentHeight - initialPos.y);
            sr.size = srSize;
            elapsedTime += Time.deltaTime;
            yield return null; // Espera al siguiente frame
        }

        // Asegurar que llegue exactamente a la escala deseada
        drillHead.transform.localPosition = targetPos;
        srSize.y = initialTubeHeight + Mathf.Abs(targetPos.y - initialPos.y);
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