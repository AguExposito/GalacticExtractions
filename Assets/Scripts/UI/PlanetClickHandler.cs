using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlanetClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Button planetButton;
    private PlanetInfoPanel infoPanel;
    private int planetIndex = -1;
    private bool isInitialized = false;
    private Image planetImage;
    private Material originalMaterial;
    private Material outlineMaterial;
    private bool isHovering = false;
    private bool hasOwnMaterial = false;
    
    void Awake()
    {
        // Ensure we have all necessary components
        planetButton = GetComponent<Button>();
        if (planetButton == null)
        {
            planetButton = gameObject.AddComponent<Button>();
        }
        
        // Get the planet image
        planetImage = GetComponent<Image>();
        if (planetImage != null)
        {
            planetImage.raycastTarget = true;
            originalMaterial = planetImage.material;
        }
        
        // Load outline shader material
        LoadOutlineMaterial();
        
        // Add click listener
        planetButton.onClick.AddListener(OnPlanetClick);
        
        Debug.Log($"PlanetClickHandler Awake completed for planet {planetIndex}");
    }
    
    void Start()
    {
        Debug.Log($"PlanetClickHandler Start for planet {planetIndex}, initialized: {isInitialized}");
    }
    
    public void Initialize(int index, PlanetInfoPanel panel)
    {
        planetIndex = index;
        infoPanel = panel;
        isInitialized = true;
        
        Debug.Log($"PlanetClickHandler {planetIndex} connected to panel: {panel?.name}");
        
        // Ensure button is interactable
        if (planetButton != null)
        {
            planetButton.interactable = true;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Planet {planetIndex} clicked via IPointerClickHandler");
        OnPlanetClick();
    }
    
    void OnPlanetClick()
    {
        Debug.Log($"Planet {planetIndex} clicked! Initialized: {isInitialized}");
        
        if (!isInitialized)
        {
            Debug.LogWarning($"Planet {planetIndex} not initialized yet!");
            return;
        }
        
        if (infoPanel != null)
        {
            Debug.Log($"Info panel found: {infoPanel.name}");
            
            // Always show panel when clicking a planet
            infoPanel.ShowPlanetInfo(planetIndex, transform, infoPanel.transform.parent);
        }
        else
        {
            Debug.LogError($"Info panel is null for planet {planetIndex}!");
        }
    }
    
    // Método para verificar si el planeta es clickeable
    public bool IsClickable()
    {
        return isInitialized && planetButton != null && planetButton.interactable;
    }
    
    void LoadOutlineMaterial()
    {
        // Get the base shader
        Shader outlineShader = null;
        
        // Try to get shader from OutlineShaderSetup
        OutlineShaderSetup outlineSetup = FindFirstObjectByType<OutlineShaderSetup>();
        if (outlineSetup != null && outlineSetup.outlineShader != null)
        {
            outlineShader = outlineSetup.outlineShader;
        }
        else
        {
            // Try to find shader by name
            outlineShader = Shader.Find("Custom/SpriteOutline");
            if (outlineShader == null)
            {
                outlineShader = Shader.Find("SpriteOutline");
            }
        }
        
        if (outlineShader == null)
        {
            Debug.LogWarning("Outline shader not found! Please ensure the 2D Sprite Outline shader is properly set up.");
            return;
        }
        
        // Create a unique material instance for this planet
        outlineMaterial = new Material(outlineShader);
        outlineMaterial.name = $"PlanetOutline_{planetIndex}";
        hasOwnMaterial = true;
        
        Debug.Log($"Created unique outline material for planet {planetIndex}");
    }
    
    // Hover methods
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInitialized) return;
        
        isHovering = true;
        SetOutlineColor(GetHoverColor());
        
        Debug.Log($"Planet {planetIndex} hover started");
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInitialized) return;
        
        isHovering = false;
        UpdateOutlineColor();
        
        Debug.Log($"Planet {planetIndex} hover ended");
    }
    
    // Method to update outline color based on planet state
    public void UpdateOutlineColor()
    {
        if (isHovering) return; // Don't change color if hovering
        
        Color outlineColor = GetOutlineColorForState();
        SetOutlineColor(outlineColor);
    }
    
    // Method to initialize outline with planet data
    public void InitializeOutline(PlanetData planetData)
    {
        UpdateOutlineColor();
    }
    
    void SetOutlineColor(Color color)
    {
        if (planetImage != null && outlineMaterial != null)
        {
            // Apply outline material
            planetImage.material = outlineMaterial;
            
            // Set outline color in shader (try different property names)
            if (outlineMaterial.HasProperty("_OutlineColor"))
            {
                outlineMaterial.SetColor("_OutlineColor", color);
            }
            else if (outlineMaterial.HasProperty("_Color"))
            {
                outlineMaterial.SetColor("_Color", color);
            }
            else if (outlineMaterial.HasProperty("_MainTex"))
            {
                // Some shaders use _MainTex for color
                outlineMaterial.SetColor("_MainTex", color);
            }
            else
            {
                Debug.LogWarning($"Could not find outline color property in shader for planet {planetIndex}");
            }
        }
    }
    
    Color GetOutlineColorForState()
    {
        // Get colors from RadialPlanetController
        RadialPlanetController radialController = FindFirstObjectByType<RadialPlanetController>();
        if (radialController != null)
        {
            // Get planet data to determine state
            SolarSystemManager solarManager = FindFirstObjectByType<SolarSystemManager>();
            if (solarManager != null)
            {
                PlanetData planetData = solarManager.GetPlanetData(planetIndex);
                if (planetData != null)
                {
                    if (planetData.isPlayerPlanet)
                        return radialController.playerPlanetOutlineColor;
                    else if (planetData.isExplored)
                        return radialController.exploredPlanetOutlineColor;
                    else
                        return radialController.unexploredPlanetOutlineColor;
                }
            }
        }
        
        // Fallback colors
        return Color.white;
    }
    
    Color GetHoverColor()
    {
        RadialPlanetController radialController = FindFirstObjectByType<RadialPlanetController>();
        if (radialController != null)
        {
            return radialController.hoverPlanetOutlineColor;
        }
        
        return new Color(1f, 0.5f, 0f, 1f); // Fallback orange
    }
    
    // Method to restore original material
    public void RestoreOriginalMaterial()
    {
        if (planetImage != null && originalMaterial != null)
        {
            planetImage.material = originalMaterial;
        }
    }
    
    // Clean up material when destroyed
    void OnDestroy()
    {
        if (hasOwnMaterial && outlineMaterial != null)
        {
            DestroyImmediate(outlineMaterial);
        }
    }
} 