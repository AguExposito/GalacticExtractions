using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialPlanetController : MonoBehaviour
{
    [Header("Orbit Settings")]
    public float[] orbitRadii = { 200f, 400f, 600f };
    public float[] orbitRotationSpeeds = { 0.5f, 0.75f, 1f };
    public int[] planetsPerOrbit = { 3, 4, 5 };
    public bool[] isClosestOrbit = { true, false, false };
    
    [Header("Planet Settings")]
    public Sprite[] biomeSprites;
    public float planetSelfRotationSpeed = 30f;
    public float planetSelfRotationSpeedVariation = 10f;
    
    [Header("UI References")]
    public GameObject planetPrefab;
    public GameObject orbitLinePrefab;
    public Transform canvasTransform;
    public PlanetInfoPanel planetInfoPanel; // Single panel reference
    
    [Header("Player Settings")]
    public Color playerPlanetOutlineColor = Color.blue; // Planeta actual del jugador
    public Color exploredPlanetOutlineColor = Color.green; // Planetas visitados
    public Color unexploredPlanetOutlineColor = Color.white; // Planetas por visitar
    public Color hoverPlanetOutlineColor = new Color(1f, 0.5f, 0f, 1f); // Naranja al hacer hover
    public int playerPlanetIndex = 0;
    
    private List<Transform> planets = new List<Transform>();
    private List<OrbitLineController> orbitLines = new List<OrbitLineController>();
    private List<PlanetClickHandler> planetClickHandlers = new List<PlanetClickHandler>();
    private SolarSystemManager solarSystemManager;
    
    void Start()
    {
        Debug.Log("RadialPlanetController Start called");
        
        // Find SolarSystemManager
        solarSystemManager = FindFirstObjectByType<SolarSystemManager>();
        if (solarSystemManager == null)
        {
            Debug.LogError("SolarSystemManager not found! Please add it to the scene.");
            return;
        }
        
        // Validate references
        if (canvasTransform == null)
        {
            Debug.LogError("Canvas transform is null! Please assign it in the inspector.");
            return;
        }
        
        if (planetPrefab == null)
        {
            Debug.LogError("Planet prefab is null! Please assign it in the inspector.");
            return;
        }
        
        if (orbitLinePrefab == null)
        {
            Debug.LogError("Orbit line prefab is null! Please assign it in the inspector.");
            return;
        }
        
        if (planetInfoPanel == null)
        {
            Debug.LogError("PlanetInfoPanel is null! Please assign it in the inspector.");
            return;
        }
        
        CreateOrbitLines();
        CreatePlanets();
        ConnectPlanetsWithPanel();
        UpdatePlanetOutlines();
        Debug.Log($"Created {orbitLines.Count} orbit lines and {planets.Count} planets");
        
        // Debug info after creation
        Invoke("DebugPlanetStatus", 1f); // Delay to ensure everything is initialized
    }
    
    void Update()
    {
        RotatePlanets();
    }
    
    void CreateOrbitLines()
    {
        Debug.Log("Creating orbit lines...");
        for (int i = 0; i < orbitRadii.Length; i++)
        {
            GameObject orbitLine = Instantiate(orbitLinePrefab, canvasTransform);
            OrbitLineController orbitController = orbitLine.GetComponent<OrbitLineController>();
            
            if (orbitController == null)
            {
                orbitController = orbitLine.AddComponent<OrbitLineController>();
            }
            
            // Configure the orbit line
            orbitController.SetRadius(orbitRadii[i]);
            orbitController.SetRotationSpeed(orbitRotationSpeeds[i]);
            
            // Center the orbit line
            orbitLine.transform.localPosition = Vector3.zero;
            
            orbitLines.Add(orbitController);
            Debug.Log($"Created orbit line {i} with radius {orbitRadii[i]} and speed {orbitRotationSpeeds[i]}");
        }
    }
    
    void CreatePlanets()
    {
        Debug.Log("Creating planets...");
        for (int orbitIndex = 0; orbitIndex < orbitRadii.Length; orbitIndex++)
        {
            float radius = orbitRadii[orbitIndex];
            int planetCount = planetsPerOrbit[orbitIndex];
            
            // Get the orbit line transform as parent
            Transform orbitParent = orbitLines[orbitIndex].transform;
            
            for (int planetIndex = 0; planetIndex < planetCount; planetIndex++)
            {
                float angle = (360f / planetCount) * planetIndex;
                Vector3 position = CalculatePlanetPosition(radius, angle);
                
                // Create planet as child of the orbit
                GameObject planet = Instantiate(planetPrefab, orbitParent);
                planet.transform.localPosition = position;
                
                // Add PlanetPrefabController if it doesn't exist
                PlanetPrefabController planetController = planet.GetComponent<PlanetPrefabController>();
                if (planetController == null)
                {
                    planetController = planet.AddComponent<PlanetPrefabController>();
                }
                
                // Set planet index
                planetController.SetPlanetIndex(planets.Count);
                
                // Set planet sprite based on biome
                Image planetImage = planet.GetComponent<Image>();
                if (planetImage != null && biomeSprites.Length > 0)
                {
                    int spriteIndex = Random.Range(0, biomeSprites.Length);
                    planetImage.sprite = biomeSprites[spriteIndex];
                    planetController.SetSprite(planetImage.sprite);
                }
                
                // Outline will be handled by shader in PlanetClickHandler
                
                // Ensure all planets are clickable
                planetController.SetInteractable(true);
                
                // Add click handler
                PlanetClickHandler clickHandler = planet.GetComponent<PlanetClickHandler>();
                if (clickHandler == null)
                    clickHandler = planet.AddComponent<PlanetClickHandler>();
                
                planetClickHandlers.Add(clickHandler);
                planets.Add(planet.transform);
                
                Debug.Log($"Created planet {planets.Count - 1} in orbit {orbitIndex}");
            }
        }
    }
    
    void ConnectPlanetsWithPanel()
    {
        Debug.Log("Connecting planets with single panel...");
        for (int i = 0; i < planetClickHandlers.Count; i++)
        {
            planetClickHandlers[i].Initialize(i, planetInfoPanel);
            
            // Initialize outline with planet data
            if (solarSystemManager != null)
            {
                PlanetData planetData = solarSystemManager.GetPlanetData(i);
                if (planetData != null)
                {
                    planetClickHandlers[i].InitializeOutline(planetData);
                }
            }
            
            Debug.Log($"Connected planet {i} to panel");
        }
    }
    
    Vector3 CalculatePlanetPosition(float radius, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector3(
            Mathf.Cos(radians) * radius,
            Mathf.Sin(radians) * radius,
            0
        );
    }
    
    void RotatePlanets()
    {
        for (int i = 0; i < planets.Count; i++)
        {
            if (planets[i] != null)
            {
                float speed = planetSelfRotationSpeed + Random.Range(-planetSelfRotationSpeedVariation, planetSelfRotationSpeedVariation);
                planets[i].Rotate(0, 0, speed * Time.deltaTime);
            }
        }
    }
    
    void UpdatePlanetOutlines()
    {
        if (solarSystemManager == null) return;
        
        for (int i = 0; i < planets.Count; i++)
        {
            if (planets[i] != null)
            {
                PlanetData planetData = solarSystemManager.GetPlanetData(i);
                if (planetData != null)
                {
                    UpdatePlanetOutline(i, planetData);
                }
            }
        }
    }
    
    public void UpdatePlanetOutline(int planetIndex, PlanetData planetData)
    {
        if (planets[planetIndex] != null)
        {
            // Update outline using PlanetClickHandler with shader
            PlanetClickHandler clickHandler = planets[planetIndex].GetComponent<PlanetClickHandler>();
            if (clickHandler != null)
            {
                clickHandler.InitializeOutline(planetData);
            }
        }
    }
    
    // Método de debug para verificar el estado de los planetas
    public void DebugPlanetStatus()
    {
        Debug.Log($"=== PLANET DEBUG INFO ===");
        Debug.Log($"Total planets created: {planets.Count}");
        Debug.Log($"Total click handlers: {planetClickHandlers.Count}");
        
        for (int i = 0; i < planets.Count; i++)
        {
            if (planets[i] != null)
            {
                PlanetClickHandler clickHandler = planets[i].GetComponent<PlanetClickHandler>();
                Button button = planets[i].GetComponent<Button>();
                Image image = planets[i].GetComponent<Image>();
                
                Debug.Log($"Planet {i}: Active={planets[i].gameObject.activeSelf}, " +
                         $"ClickHandler={clickHandler != null}, " +
                         $"Button={button != null}, " +
                         $"ButtonInteractable={button?.interactable}, " +
                         $"Image={image != null}, " +
                         $"RaycastTarget={image?.raycastTarget}");
            }
        }
        
        if (planetInfoPanel != null)
        {
            Debug.Log($"PlanetInfoPanel: Active={planetInfoPanel.gameObject.activeSelf}, " +
                     $"PanelObject={planetInfoPanel.panelObject != null}");
        }
        else
        {
            Debug.LogError("PlanetInfoPanel is null!");
        }
    }
}


