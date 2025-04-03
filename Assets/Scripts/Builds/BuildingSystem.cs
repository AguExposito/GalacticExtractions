using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public Tilemap[] tilemaps;
    public Tilemap groundTilemap; // Tilemap de bedrock para validar la colocación
    public Transform previewParent; // Parent para manejar la preview
    public Color validColor = new Color(0, 1, 0, 0.5f);
    public Color invalidColor = new Color(1, 0, 0, 0.5f);
    public RadialMenuController radialMenu;

    private GameObject currentPreview;
    private Vector3Int cellPosition;
    private bool canPlace = false;
    private bool isPlacing = false;
    private GameObject selectedStructure;
    InputSystem_Actions controls;
    int fingerIndex;
    bool touchOverUI = false;
    Vector3 lastTocuhPosition;
    private void Awake()
    {
        // Aseguramos que el sistema de input esté inicializado antes de usarlo
        controls = new InputSystem_Actions();
        EnhancedTouchSupport.Enable();
    }
    void OnEnable() 
    { 
        controls.Enable();

        //PC
        controls.BuildingSystem.PlaceStructure.performed += ctx =>
        {
            if (canPlace)
            {
                TryPlaceStructure();
            }
        };
        controls.BuildingSystem.DestroyPreview.performed += ctx => DestroyPreview();
        controls.BuildingSystem.RotatePreview.performed += ctx => RotatePreview();
        controls.BuildingSystem.ShowReach.performed += ctx =>
        {
            EnableLR();
        }; 

        //Android
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += HandleFingerDown;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += HandleFingerDown;

    }

    void OnDisable() 
    { 
        controls.Disable();
        //PC
        controls.BuildingSystem.PlaceStructure.performed -= ctx =>
        {
            if (canPlace)
            {
                TryPlaceStructure();
            }
        };
        controls.BuildingSystem.DestroyPreview.performed -= ctx => DestroyPreview();
        controls.BuildingSystem.RotatePreview.performed -= ctx => RotatePreview();
        controls.BuildingSystem.ShowReach.performed -= ctx =>
        {
            EnableLR();
        };

        //Android
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove -= HandleFingerDown;
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= HandleFingerDown;
    }
    private void HandleFingerDown(Finger finger)
    {
        // Verifica si el toque está sobre UI
        touchOverUI = IsTouchOverUI(finger.screenPosition);
        Debug.Log("Touch Over UI: " + touchOverUI);
        if (!touchOverUI)
        {
            lastTocuhPosition = finger.screenPosition;
            PositioningBuilding();
        }
    }
    bool state=true;
    public void EnableLR() {
        GameObject[] go = GameObject.FindGameObjectsWithTag("EffectReach");
        foreach (GameObject gameObject in go)
        {
            gameObject.GetComponent<LineRenderer>().enabled = state;
        }
        state = !state;
        Debug.Log("ShowStarted " + go.Length);
    }
    private bool IsTouchOverUI(Vector2 touchPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = touchPosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
    void Update()
    {
        if (!isPlacing) { return; }
#if !PLATFORM_ANDROID
        PositioningBuilding();
#endif
        if (currentPreview != null)
        {
            canPlace = ValidatePosition() && ValidatePositionBuilding();

            UpdateLineRenderer(); 

            SpriteRenderer sr = currentPreview.GetComponent<SpriteRenderer>();
            LineRenderer ln=currentPreview.GetComponent<LineRenderer>();
            ln.startColor = canPlace ? validColor : invalidColor;
            ln.endColor = canPlace ? validColor : invalidColor;
            sr.color = canPlace? validColor : invalidColor;
        }
    }
    public void PositioningBuilding()
    {
        Vector3 worldPosition;
        worldPosition = GetMouseOrTouchPosition();
        worldPosition.z = 0;


        cellPosition = groundTilemap.WorldToCell(worldPosition);
        Vector3 snappedPosition = groundTilemap.GetCellCenterWorld(cellPosition);

        if (currentPreview != null)
        {
            currentPreview.transform.position = snappedPosition;
        }
    }
    public void DestroyPreview()
    {
#if PLATFORM_ANDROID
        radialMenu.gameObject.SetActive(false);
        radialMenu.ReparentAndResize(null);
#endif
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
            isPlacing = false;
        }

    }
    public void RotatePreview()
    {
        if (currentPreview == null) return;

        float currentRotation = currentPreview.transform.eulerAngles.z; // Obtiene la rotación en Z

        float rotationStep = 90f; // Rotar en pasos de 90°
        float newRotation=0;
#if !PLATFORM_ANDROID
        if (controls.BuildingSystem.RotatePreview.ReadValue<float>() > 0)
        {
            newRotation = Mathf.Repeat(currentRotation + rotationStep, 360); // Asegura que esté en 0-360°
        }
        else if (controls.BuildingSystem.RotatePreview.ReadValue<float>() < 0)
        {
            newRotation = Mathf.Repeat(currentRotation - rotationStep, 360); // Asegura que esté en 0-360°
        }
#else
        newRotation = Mathf.Repeat(currentRotation - rotationStep, 360); // Asegura que esté en 0-360°
        
#endif
        currentPreview.transform.rotation = Quaternion.Euler(0, 0, newRotation); // Aplica la rotación en Z
        radialMenu.transform.rotation = Quaternion.Euler(0, 0, 0);
    }


    public void SelectStructure(GameObject selectedStructure)
    {
        if (!isPlacing)
        {
            if (currentPreview != null)
            {
                DestroyPreview();
            }
            this.selectedStructure = selectedStructure;
            currentPreview = Instantiate(selectedStructure, previewParent);
            currentPreview.GetComponent<SpriteRenderer>().color = validColor;
            currentPreview.GetComponent<SpriteRenderer>().sortingOrder = 20;
#if PLATFORM_ANDROID
            radialMenu.ReparentAndResize(currentPreview.transform);
            radialMenu.gameObject.SetActive(true);
#endif
            currentPreview.GetComponent<LineRenderer>().enabled=true;
            UpdateLineRenderer();
            isPlacing = true;
        }
    }
    void UpdateLineRenderer() {
        LineRenderer border = currentPreview.GetComponent<LineRenderer>();
        if (border == null) return;
        // Obtener el tamaño y la posición del sprite
        Bounds bounds = currentPreview.GetComponent<SpriteRenderer>().bounds;

        // Definir las esquinas del borde
        Vector3[] corners = new Vector3[]
        {
            new Vector3(-0.5f,-0.5f,0),
            new Vector3(0.5f,-0.5f,0),
            new Vector3(0.5f,0.5f,0),
            new Vector3(-0.5f,0.5f,0),
        };

        border.SetPositions(corners);
    }

    bool ValidatePosition()
    {
        if (currentPreview == null) return false;

        Bounds bounds = currentPreview.GetComponent<SpriteRenderer>().bounds;
        Vector3Int minCell = groundTilemap.WorldToCell(bounds.min);
        Vector3Int maxCell = groundTilemap.WorldToCell(bounds.max);

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector3Int checkPos = new Vector3Int(x, y, 0);
                foreach (Tilemap tilemap in tilemaps)
                {
                    if (tilemap.HasTile(checkPos) || !groundTilemap.HasTile(checkPos))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }


    bool ValidatePositionBuilding()
    {
        Vector2 mousePosition = GetMouseOrTouchPosition2D();

        Vector3Int cellPosition = groundTilemap.WorldToCell(mousePosition);
        Vector2 cellCenterPosition = groundTilemap.GetCellCenterWorld(cellPosition);


        Vector2 size = currentPreview.transform.localScale - new Vector3(1,1,0); 
        Collider2D[] colliders = Physics2D.OverlapBoxAll(mousePosition, size, 0f);

        // Recorremos todos los colliders que hemos detectado
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Building") || collider.CompareTag("Player"))
            {
                Debug.Log("Se ha detectado un objeto 'Building' en la posición del mouse");
                return false;
            }
        }

        return true;
    }

    public void TryPlaceStructure()
    {

        if (currentPreview != null && canPlace)
        {
            GameObject building = Instantiate(selectedStructure, currentPreview.transform.position, Quaternion.identity, previewParent);
            building.transform.rotation = currentPreview.transform.rotation;
            building.GetComponent<Collider2D>().enabled = true;
            building.tag = "Building";

            DestroyPreview();
        }
    }
    Vector3 GetMouseOrTouchPosition()
    {
#if PLATFORM_ANDROID
        if (lastTocuhPosition == null)
        {
            return Vector3.zero;
        }
        return Camera.main.ScreenToWorldPoint(lastTocuhPosition);
#else
        return Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
#endif
    }
    Vector2 GetMouseOrTouchPosition2D()
    {
#if PLATFORM_ANDROID
        if (Application.isMobilePlatform && lastTocuhPosition == null)
        {
            return Vector3.zero;
        }
        return Camera.main.ScreenToWorldPoint(lastTocuhPosition);
#else
        return Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
#endif
    }

}
