using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

    private GameObject currentPreview;
    private Vector3Int cellPosition;
    private bool canPlace = false;
    private bool isPlacing = false;
    private GameObject selectedStructure;
    InputSystem_Actions controls;
    int fingerIndex;
    bool touchOverUI = false;
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

        //Android
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

        //Android
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
            canPlace = ValidatePosition() && ValidatePositionBuilding();

            UpdateLineRenderer(); 

            SpriteRenderer sr = currentPreview.GetComponent<SpriteRenderer>();
            LineRenderer ln=currentPreview.GetComponent<LineRenderer>();
            ln.startColor = canPlace ? validColor : invalidColor;
            ln.endColor = canPlace ? validColor : invalidColor;
            sr.color = canPlace? validColor : invalidColor;
        }
        
    }

    private void DestroyPreview()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
            isPlacing = false;
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
            currentPreview.GetComponent<SpriteRenderer>().sortingOrder = 20;


            //LineRenderer border = currentPreview.AddComponent<LineRenderer>();
            //border.useWorldSpace = false;
            //border.startWidth = 0.1f;
            //border.endWidth = 0.1f; 
            //border.loop = true;
            //border.positionCount = 4; // 4 esquinas

            //// Material para que no se vuelva invisible
            //Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
            //border.material = lineMaterial;
            //border.startColor = Color.white;
            //border.endColor = Color.white;
            //border.sortingOrder = 30; // Asegurar que esté visible sobre el sprite
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

    void TryPlaceStructure()
    {
        if (currentPreview != null && canPlace)
        {
            GameObject building = Instantiate(selectedStructure, currentPreview.transform.position, Quaternion.identity, previewParent);
            Destroy(currentPreview);
            isPlacing = false;
            building.GetComponent<Collider2D>().enabled = true;
            building.tag = "Building";
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
    Vector2 GetMouseOrTouchPosition2D()
    {
        if (Touchscreen.current != null && (Touchscreen.current.primaryTouch.press.isPressed || Touchscreen.current.primaryTouch.press.wasReleasedThisFrame))
        {
            return Camera.main.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
        }

        return Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

}
