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
        
        if (oreGenerator.oreTileData.ContainsKey(position)) {
            oreGenerator.oreTileData[position].health -= damage;

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
