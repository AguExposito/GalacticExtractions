using UnityEngine;
using UnityEngine.UI;

public class PlanetMaterialDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showMaterialInfo = true;
    public bool testColorChanges = false;
    
    private PlanetClickHandler[] planetHandlers;
    
    void Start()
    {
        Invoke("DebugPlanetMaterials", 1f); // Wait for initialization
    }
    
    void DebugPlanetMaterials()
    {
        planetHandlers = FindObjectsByType<PlanetClickHandler>(FindObjectsSortMode.None);
        Debug.Log($"=== PLANET MATERIAL DEBUGGER ===");
        Debug.Log($"Found {planetHandlers.Length} planet handlers");
        
        if (showMaterialInfo)
        {
            ShowMaterialInfo();
        }
        
        if (testColorChanges)
        {
            TestColorChanges();
        }
    }
    
    void ShowMaterialInfo()
    {
        Debug.Log("=== MATERIAL INFORMATION ===");
        
        for (int i = 0; i < planetHandlers.Length; i++)
        {
            var handler = planetHandlers[i];
            if (handler != null)
            {
                Image planetImage = handler.GetComponent<Image>();
                if (planetImage != null)
                {
                    Debug.Log($"Planet {i}: {handler.gameObject.name}");
                    Debug.Log($"  - Has Image: {planetImage != null}");
                    Debug.Log($"  - Has Material: {planetImage.material != null}");
                    
                    if (planetImage.material != null)
                    {
                        Debug.Log($"  - Material Name: {planetImage.material.name}");
                        Debug.Log($"  - Material Instance ID: {planetImage.material.GetInstanceID()}");
                        Debug.Log($"  - Shader: {planetImage.material.shader.name}");
                    }
                }
            }
        }
    }
    
    void TestColorChanges()
    {
        Debug.Log("=== TESTING COLOR CHANGES ===");
        
        // Test hover on first planet
        if (planetHandlers.Length > 0)
        {
            var firstHandler = planetHandlers[0];
            Debug.Log($"Testing hover on planet: {firstHandler.gameObject.name}");
            
            // Simulate hover
            firstHandler.OnPointerEnter(null);
            
            // Check if other planets are affected
            for (int i = 1; i < planetHandlers.Length; i++)
            {
                var otherHandler = planetHandlers[i];
                Image otherImage = otherHandler.GetComponent<Image>();
                if (otherImage != null && otherImage.material != null)
                {
                    Color currentColor = Color.white;
                    if (otherImage.material.HasProperty("_OutlineColor"))
                    {
                        currentColor = otherImage.material.GetColor("_OutlineColor");
                    }
                    else if (otherImage.material.HasProperty("_Color"))
                    {
                        currentColor = otherImage.material.GetColor("_Color");
                    }
                    
                    Debug.Log($"  - Planet {i} color after hover on planet 0: {currentColor}");
                }
            }
            
            // Remove hover
            firstHandler.OnPointerExit(null);
        }
    }
    
    // Method to manually trigger debug
    [ContextMenu("Debug Materials")]
    public void ManualDebug()
    {
        DebugPlanetMaterials();
    }
    
    // Method to test individual planet hover
    public void TestPlanetHover(int planetIndex)
    {
        if (planetIndex >= 0 && planetIndex < planetHandlers.Length)
        {
            var handler = planetHandlers[planetIndex];
            Debug.Log($"Testing hover on planet {planetIndex}: {handler.gameObject.name}");
            
            handler.OnPointerEnter(null);
            
            // Store the handler for removal
            StartCoroutine(RemoveHoverAfterDelay(handler, planetIndex, 2f));
        }
    }
    
    System.Collections.IEnumerator RemoveHoverAfterDelay(PlanetClickHandler handler, int planetIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        handler.OnPointerExit(null);
        Debug.Log($"Removed hover from planet {planetIndex}");
    }
} 