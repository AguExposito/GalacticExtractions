
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Tilemaps;

public class DrillController : MonoBehaviour
{
    public int cellsToDrill = 10;
    public int extractionDamage= 10;
    public float timePerDmgTick= 0.2f;
    public float tubeTime;
    public GameObject drillHead;
    public GameObject drillTube;
    public GameObject effectReach;
    Vector3Int nextCellPos;
    Vector3 initialHeadPos;
    bool isExtending;

    Tilemap oreTilemap;
    Tilemap wallsTilemap;
    DamageTile damageTile;
    Coroutine dmgCoroutine;
    InputSystem_Actions controls;
    private void Awake()
    {
        // Aseguramos que el sistema de input est� inicializado antes de usarlo
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
                Debug.Log("No hay m�s Tiles para destruir");
            }
        }
    }

    bool CheckForDrillingSpots() {
        for (int i = 0; i < cellsToDrill; i++)
        {
            Vector3Int cellPosition = oreTilemap.WorldToCell(drillHead.transform.position)+GetDrillDirection()*i;
            if (oreTilemap.HasTile(cellPosition) || wallsTilemap.HasTile(cellPosition)) {
                cellsToDrill-=i;
                nextCellPos = cellPosition;
                return true;
            }
        }
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

    void ExtendTube() {
        if (isExtending) return;
        isExtending = true;
        Debug.Log("WaitForTubeExtension");
        StartCoroutine(WaitForTubeExtension());
    }

    IEnumerator WaitForTubeExtension()
    {
        Vector3Int cellPosition = oreTilemap.WorldToCell(drillHead.transform.position);
        int distance = (int)Vector3.Distance( cellPosition, nextCellPos);

        Vector3Int drillDir = GetDrillDirection();

        if (distance == 0) {
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
            srSize.y = initialTubeHeight+ Mathf.Abs(currentHeight - initialPos.y);
            sr.size = srSize;
            elapsedTime += Time.deltaTime;
            yield return null; // Espera al siguiente frame
        }

        // Asegurar que llegue exactamente a la escala deseada
        drillHead.transform.localPosition = targetPos;
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

            yield return new WaitForSeconds(timePerTick); // Espera antes de aplicar el siguiente da�o
        }
        
    }


    void OnEnable()
    {
        controls.Enable();
        
        //PC
        controls.BuildingSystem.ShowReach.performed += ctx =>
        {

            if(effectReach!=null)
                effectReach.SetActive(true);
        };
        controls.BuildingSystem.ShowReach.canceled += ctx =>
        {
            if(effectReach!=null)
                effectReach.SetActive(false);
        };
        //Android
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += finger =>
        {
            if (effectReach != null && UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count>=2)
                effectReach.SetActive(true);
        };
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp += finger =>
        {
            if (effectReach != null)
                effectReach.SetActive(false);
        };
    }

    void OnDisable()
    {
        controls.Disable();

        //PC
        controls.BuildingSystem.ShowReach.performed -= ctx =>
        {

            if (effectReach != null)
                effectReach.SetActive(true);
        };
        controls.BuildingSystem.ShowReach.canceled -= ctx =>
        {
            if (effectReach != null)
                effectReach.SetActive(false);
        };
        //Android
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= finger =>
        {
            if (effectReach != null && UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count >= 2)
                effectReach.SetActive(true);
        };
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp -= finger =>
        {
            if (effectReach != null)
                effectReach.SetActive(false);
        };
    }

}
