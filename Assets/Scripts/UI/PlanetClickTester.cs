using UnityEngine;

public class PlanetClickTester : MonoBehaviour
{
    [Header("Test Settings")]
    public bool enableClickTesting = true;
    public KeyCode testKey = KeyCode.T;
    
    private RadialPlanetController radialController;
    private SolarSystemManager solarManager;
    
    void Start()
    {
        radialController = FindFirstObjectByType<RadialPlanetController>();
        solarManager = FindFirstObjectByType<SolarSystemManager>();
        
        if (radialController == null)
            Debug.LogError("RadialPlanetController not found!");
        else
            Debug.Log("RadialPlanetController found and ready for testing");
            
        if (solarManager == null)
            Debug.LogError("SolarSystemManager not found!");
        else
            Debug.Log("SolarSystemManager found and ready for testing");
    }
    
    void Update()
    {
        if (!enableClickTesting) return;
        
        // Press T to test planet status
        if (Input.GetKeyDown(testKey))
        {
            TestPlanetSystem();
        }
        
        // Press R to refresh planet outlines
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (radialController != null)
            {
                radialController.DebugPlanetStatus();
            }
        }
        
        // Press C to test outline colors
        if (Input.GetKeyDown(KeyCode.C))
        {
            TestOutlineColors();
        }
        
        // Press O to test shader outline system
        if (Input.GetKeyDown(KeyCode.O))
        {
            TestShaderOutlineSystem();
        }
    }
    
    void TestPlanetSystem()
    {
        Debug.Log("=== TESTING PLANET SYSTEM ===");
        
        if (radialController != null)
        {
            radialController.DebugPlanetStatus();
        }
        
        if (solarManager != null)
        {
            Debug.Log($"SolarSystemManager has {solarManager.planets.Count} planets");
            
            for (int i = 0; i < solarManager.planets.Count; i++)
            {
                PlanetData planet = solarManager.GetPlanetData(i);
                if (planet != null)
                {
                    Debug.Log($"Planet {i}: {planet.planetName}, " +
                             $"Explored: {planet.isExplored}, " +
                             $"Player: {planet.isPlayerPlanet}, " +
                             $"Biomes: {planet.GetBiomeDescription()}");
                }
            }
        }
    }
    
    // Test outline colors
    public void TestOutlineColors()
    {
        Debug.Log("=== TESTING OUTLINE COLORS ===");
        
        if (radialController != null)
        {
            Debug.Log($"Player Planet Color: {radialController.playerPlanetOutlineColor}");
            Debug.Log($"Explored Planet Color: {radialController.exploredPlanetOutlineColor}");
            Debug.Log($"Unexplored Planet Color: {radialController.unexploredPlanetOutlineColor}");
            Debug.Log($"Hover Planet Color: {radialController.hoverPlanetOutlineColor}");
        }
    }
    
    // Test shader outline system
    public void TestShaderOutlineSystem()
    {
        Debug.Log("=== TESTING SHADER OUTLINE SYSTEM ===");
        
        // Check if OutlineShaderSetup exists
        OutlineShaderSetup outlineSetup = FindFirstObjectByType<OutlineShaderSetup>();
        if (outlineSetup != null)
        {
            Debug.Log("OutlineShaderSetup found and active");
            Shader outlineShader = outlineSetup.GetOutlineShader();
            if (outlineShader != null)
            {
                Debug.Log($"Outline shader: {outlineShader.name}");
            }
            else
            {
                Debug.LogWarning("Outline shader is null");
            }
        }
        else
        {
            Debug.LogWarning("OutlineShaderSetup not found");
        }
        
        // Check planet click handlers and their materials
        PlanetClickHandler[] clickHandlers = FindObjectsByType<PlanetClickHandler>(FindObjectsSortMode.None);
        Debug.Log($"Found {clickHandlers.Length} planet click handlers");
        
        foreach (var handler in clickHandlers)
        {
            if (handler != null)
            {
                Debug.Log($"Planet click handler found on: {handler.gameObject.name}");
                
                                 // Check if they have unique materials
                 UnityEngine.UI.Image planetImage = handler.GetComponent<UnityEngine.UI.Image>();
                 if (planetImage != null && planetImage.material != null)
                 {
                     Debug.Log($"  - Material: {planetImage.material.name}");
                 }
            }
        }
    }
} 