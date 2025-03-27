using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class DamageTile : MonoBehaviour
{
    MapGenerator mapGenerator;
    OreGenerator oreGenerator;
    TileHealthUI tileHealthUI;
    InputSystem_Actions controls;
    public int damage=1;
    int distance;
    private void Awake()
    {
        // Aseguramos que el sistema de input esté inicializado antes de usarlo
        controls = new InputSystem_Actions();
    }
    private void Start()
    {
        tileHealthUI = FindAnyObjectByType<TileHealthUI>();
        mapGenerator = FindAnyObjectByType<MapGenerator>();
        oreGenerator = FindAnyObjectByType<OreGenerator>();
        EnhancedTouchSupport.Enable(); // Activa el soporte para toques en móviles
    }
    void Update()
    {
        
    }
    public void DamageTileData(Vector3Int position)
    {
        if (mapGenerator.tileHealthData.ContainsKey(position))
        {
            mapGenerator.tileHealthData[position].health -= damage;

            if (mapGenerator.tileHealthData[position].health <= 0)
            {
                DestroyTile(position);
            }
            float remainingHealth = (float)mapGenerator.tileHealthData[position].health / (float)mapGenerator.tileHealthData[position].maxHealth;
            Debug.Log(mapGenerator.tileHealthData[position].health+" " + mapGenerator.tileHealthData[position].maxHealth+" " +remainingHealth);
            tileHealthUI.ShowHealthBar(position, remainingHealth);
        }
    }

    void DestroyTile(Vector3Int position)
    {
        switch (mapGenerator.tileHealthData[position].type) {
            case TileData.tileType.wall : { mapGenerator.wallsTilemap.SetTile(position, null); } break; // Elimina el tile del mapa
            case TileData.tileType.ore : { oreGenerator.oreTilemap.SetTile(position, null); } break; // Elimina el tile del mapa
        }
        mapGenerator.tileHealthData.Remove(position); // Remueve el tile del diccionario
    }
    Vector3Int GetTilePositionOnClick(Vector2 screenPosition)
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        return mapGenerator.wallsTilemap.WorldToCell(worldPoint);
    }

    private void OnEnable()
    {
        controls.Player.Enable();

        // **PC: Detectar clic derecho**
        controls.Player.Attack.performed += ctx =>
        {
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            DamageTileData(GetTilePositionOnClick(screenPosition));
        };

        // **Móviles: Detectar toque en la pantalla**
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += finger =>
        {
            Vector2 touchPosition = finger.screenPosition;
            DamageTileData(GetTilePositionOnClick(touchPosition));
        };
    }


    private void OnDisable()
    {
        controls.Player.Disable();
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= finger =>
        {
            Vector2 touchPosition = finger.screenPosition;
            DamageTileData(GetTilePositionOnClick(touchPosition));
        };
    }
}
