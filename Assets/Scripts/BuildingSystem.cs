using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public Tilemap wallTilemap; // Tilemap de paredes para validar la colocación
    public Tilemap oreTilemap; // Tilemap de ores para validar la colocación
    public Tilemap bedrockTilemap; // Tilemap de bedrock para validar la colocación
    public Tilemap groundTilemap; // Tilemap de bedrock para validar la colocación
    public Transform previewParent; // Parent para manejar la preview
    public Color validColor = new Color(0, 1, 0, 0.5f);
    public Color invalidColor = new Color(1, 0, 0, 0.5f);

    private GameObject currentPreview;
    private Vector3Int cellPosition;
    private bool canPlace = false;
    public bool isPlacing = false;
    private GameObject selectedStructure;
    InputSystem_Actions controls;
    int fingerIndex;
    bool touchOverUI = false;
    private void Awake()
    {
        // Aseguramos que el sistema de input esté inicializado antes de usarlo
        controls = new InputSystem_Actions();
        controls.BuildingSystem.PlaceStructure.performed += ctx => TryPlaceStructure();
        controls.BuildingSystem.DestroyPreview.performed += ctx => DestroyPreview();
        EnhancedTouchSupport.Enable();
    }

    void OnEnable() 
    { 
        controls.Enable();
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += finger =>
        {
            fingerIndex = finger.index;
            touchOverUI = !EventSystem.current.IsPointerOverGameObject(fingerIndex);
            Debug.Log(touchOverUI);
        };
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp += finger =>
        {
            if (canPlace && !!touchOverUI)
            {
                TryPlaceStructure();
            }
            else
            {
                DestroyPreview();
            }
        };
    }

    private void DestroyPreview()
    {
        if (currentPreview!=null)
        {
            Destroy(currentPreview);
            currentPreview = null;
            isPlacing = false;
        }
        
    }

    void OnDisable() 
    { 
        controls.Disable();
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= finger =>
        {
            fingerIndex = finger.index;
            touchOverUI = !EventSystem.current.IsPointerOverGameObject(fingerIndex);
            Debug.Log(touchOverUI);
        };
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp -= finger =>
        {
            if (canPlace && !!touchOverUI)
            {
                TryPlaceStructure();
            }
            else
            {
                Destroy(currentPreview);
                currentPreview = null;
                isPlacing = false;
            }
        };
    }
    
    void Update()
    {
        if (!isPlacing) { return; }


        Vector3 worldPosition = GetMouseOrTouchPosition();
        worldPosition.z = 0;
        cellPosition = groundTilemap.WorldToCell(worldPosition);
        Vector3 snappedPosition = groundTilemap.GetCellCenterWorld(cellPosition);

        if (currentPreview != null)
        {
            currentPreview.transform.position = snappedPosition;
            canPlace = ValidatePosition(cellPosition, selectedStructure);

            SpriteRenderer sr = currentPreview.GetComponent<SpriteRenderer>();
            sr.color = canPlace ? validColor : invalidColor;

        }
        
    }

    public void SelectStructure(GameObject selectedStructure)
    {
        if (!isPlacing)
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
            }
            this.selectedStructure = selectedStructure;
            currentPreview = Instantiate(selectedStructure, previewParent);
            currentPreview.GetComponent<SpriteRenderer>().color = validColor;
            isPlacing = true;
        }
    }

    bool ValidatePosition(Vector3Int cellPosition, GameObject structure)
    {
        Bounds structureBounds = structure.GetComponent<SpriteRenderer>().bounds;
        Vector3Int size = new Vector3Int(Mathf.CeilToInt(structureBounds.size.x), Mathf.CeilToInt(structureBounds.size.y), 1);

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int checkPos = cellPosition + new Vector3Int(x, y, 0);
                if (wallTilemap.HasTile(checkPos) || oreTilemap.HasTile(checkPos) || bedrockTilemap.HasTile(checkPos) || !groundTilemap.HasTile(checkPos))
                {
                    return false; // Hay colisión con una pared
                }
            }
        }
        return true;
    }

    void TryPlaceStructure()
    {
        if (currentPreview != null && canPlace)
        {
            Instantiate(selectedStructure, currentPreview.transform.position, Quaternion.identity, previewParent);
            Destroy(currentPreview);
            isPlacing = false;
        }
    }
    Vector3 GetMouseOrTouchPosition()
    {
        if (Touchscreen.current != null && (Touchscreen.current.primaryTouch.press.isPressed || Touchscreen.current.primaryTouch.press.wasReleasedThisFrame))
        {
            return Camera.main.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
        }
            
        return Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

}
