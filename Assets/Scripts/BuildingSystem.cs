using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public GameObject[] structures; // Array de estructuras que se pueden colocar
    public Tilemap wallTilemap; // Tilemap de paredes para validar la colocación
    public Tilemap oreTilemap; // Tilemap de ores para validar la colocación
    public Tilemap bedrockTilemap; // Tilemap de bedrock para validar la colocación
    public Tilemap groundTilemap; // Tilemap de bedrock para validar la colocación
    public Transform previewParent; // Parent para manejar la preview
    public Color validColor = new Color(0, 1, 0, 0.5f);
    public Color invalidColor = new Color(1, 0, 0, 0.5f);

    private GameObject currentPreview;
    private Vector3Int currentCell;
    private bool canPlace = false;
    private GameObject selectedStructure;
    InputSystem_Actions controls;
    private void Awake()
    {
        // Aseguramos que el sistema de input esté inicializado antes de usarlo
        controls = new InputSystem_Actions();
        controls.Player.PlaceStructure.performed += ctx => TryPlaceStructure();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        if (currentPreview != null)
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            worldPosition.z = 0;
            currentCell = wallTilemap.WorldToCell(worldPosition);
            Vector3 snappedPosition = wallTilemap.GetCellCenterWorld(currentCell);

            currentPreview.transform.position = snappedPosition;
            canPlace = ValidatePosition(currentCell, selectedStructure);

            SpriteRenderer sr = currentPreview.GetComponent<SpriteRenderer>();
            sr.color = canPlace ? validColor : invalidColor;

           
        }
    }

    public void SelectStructure(int index)
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }

        selectedStructure = structures[index];
        currentPreview = Instantiate(selectedStructure, previewParent);
        currentPreview.GetComponent<SpriteRenderer>().color = validColor;
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
            Instantiate(selectedStructure, currentPreview.transform.position, Quaternion.identity,previewParent);
            Destroy(currentPreview);
        }
    }


}
