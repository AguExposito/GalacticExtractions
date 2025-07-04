using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class DamageTile : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public OreGenerator oreGenerator;
    TileHealthUI tileHealthUI;
    InputSystem_Actions controls;
    public int damage=1;
    int distance;
    private void Awake()
    {
        // Aseguramos que el sistema de input est� inicializado antes de usarlo
        controls = new InputSystem_Actions();
    }
    private void Start()
    {
        tileHealthUI = FindAnyObjectByType<TileHealthUI>();
        mapGenerator = FindAnyObjectByType<MapGenerator>();
        oreGenerator = FindAnyObjectByType<OreGenerator>();
        EnhancedTouchSupport.Enable(); // Activa el soporte para toques en m�viles
    }
    void Update()
    {
        
    }
    public void DamageTileData(Vector3Int position, int damage)
    {
        
        if (oreGenerator.oreTileData.ContainsKey(position)) {
            oreGenerator.oreTileData[position].health -= damage;
            oreGenerator.oreTileData[position].ExtractOre();

            float remainingHealth = (float)oreGenerator.oreTileData[position].health / (float)oreGenerator.oreTileData[position].maxHealth;
            tileHealthUI.ShowHealthBar(position, remainingHealth);

            if (oreGenerator.oreTileData[position].health <= 0)
            {
                tileHealthUI.ShowHealthBar(position, 0);
                //destroys tile
                oreGenerator.oreTilemap.SetTile(position, null);
                oreGenerator.oreTileData.Remove(position); // Remueve el tile del diccionario

                if (mapGenerator.wallsTilemap.GetTile(position)!=null) // Remueve el walltile debajo del ore
                {
                    mapGenerator.wallsTilemap.SetTile(position, null);
                }
            }
        }
        else if (mapGenerator.wallTileData.ContainsKey(position))
        {
            mapGenerator.wallTileData[position].health -= damage;

            float remainingHealth = (float)mapGenerator.wallTileData[position].health / (float)mapGenerator.wallTileData[position].maxHealth;
            tileHealthUI.ShowHealthBar(position, remainingHealth);

            if (mapGenerator.wallTileData[position].health <= 0)
            {
                tileHealthUI.ShowHealthBar(position, 0);
                //destroys tile
                mapGenerator.wallsTilemap.SetTile(position, null);
                mapGenerator.wallTileData.Remove(position); // Remueve el tile del diccionario
            }

        }
    }

    
    Vector3Int GetTilePositionOnClick(Vector2 screenPosition)
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        return mapGenerator.wallsTilemap.WorldToCell(worldPoint);
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

    private void OnEnable()
    {
        controls.Player.Enable();

        // **PC: Detectar clic derecho**
        controls.Player.Attack.performed += ctx =>
        {
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            if (!IsTouchOverUI(screenPosition))
            {
                DamageTileData(GetTilePositionOnClick(screenPosition), damage);
            }
        };

        // **M�viles: Detectar toque en la pantalla**
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += finger =>
        {
            Vector2 touchPosition = finger.screenPosition;
            if (!IsTouchOverUI(touchPosition))
            {
                DamageTileData(GetTilePositionOnClick(touchPosition), damage);
            }
        };
    }


    private void OnDisable()
    {
        controls.Player.Disable();
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= finger =>
        {
            Vector2 touchPosition = finger.screenPosition;
            DamageTileData(GetTilePositionOnClick(touchPosition), damage);
        };
    }
}
