using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlanetInfoPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelObject;
    public TextMeshProUGUI planetNameText;
    public TextMeshProUGUI biomeText;
    public TextMeshProUGUI resourcesText;
    public TextMeshProUGUI fuelCostText;
    public Button travelButton;
    
    private Transform currentPlanet;
    private Transform canvasTransform;
    private int currentPlanetIndex = -1;
    
    void Start()
    {
        // If panelObject is not assigned, use this GameObject
        if (panelObject == null)
        {
            panelObject = gameObject;
        }
        
        // Find UI elements if not assigned
        if (planetNameText == null)
            planetNameText = panelObject.GetComponentInChildren<TextMeshProUGUI>();
            
        if (biomeText == null)
            biomeText = panelObject.transform.Find("BiomeText")?.GetComponent<TextMeshProUGUI>();
            
        if (resourcesText == null)
            resourcesText = panelObject.transform.Find("ResourcesText")?.GetComponent<TextMeshProUGUI>();
            
        if (fuelCostText == null)
            fuelCostText = panelObject.transform.Find("FuelCostText")?.GetComponent<TextMeshProUGUI>();
            
        if (travelButton == null)
            travelButton = panelObject.GetComponentInChildren<Button>();
        
        // Connect travel button
        if (travelButton != null)
        {
            travelButton.onClick.RemoveAllListeners();
            travelButton.onClick.AddListener(OnTravelButtonClicked);
            Debug.Log("Travel button connected to OnTravelButtonClicked");
        }
        else
        {
            Debug.LogWarning("Travel button not found!");
        }
        
        // Hide panel initially
        HidePanel();
        
        Debug.Log($"PlanetInfoPanel initialized. Panel object: {panelObject?.name}");
    }
    
    public void ShowPlanetInfo(int planetIndex, Transform planet, Transform canvas)
    {
        Debug.Log($"ShowPlanetInfo called for planet {planetIndex}");
        
        currentPlanetIndex = planetIndex;
        currentPlanet = planet;
        canvasTransform = canvas;
        
        // Obtener datos del planeta desde SolarSystemManager
        SolarSystemManager solarSystemManager = FindFirstObjectByType<SolarSystemManager>();
        if (solarSystemManager != null)
        {
            solarSystemManager.UpdatePlanetInfo(planetIndex);
        }
        else
        {
            Debug.LogWarning("SolarSystemManager not found! Using fallback data.");
            // Fallback data if SolarSystemManager is not available
            UpdatePanelInfoFallback(planetIndex);
        }
        
        PositionPanelNearPlanet();
        ShowPanel();
        
        Debug.Log($"Showing info for planet {planetIndex}");
    }
    
    void UpdatePanelInfoFallback(int planetIndex)
    {
        if (planetNameText != null)
            planetNameText.text = $"Planeta {planetIndex + 1}";
            
        if (biomeText != null)
            biomeText.text = "Bioma: Desierto / Ground";
            
        if (resourcesText != null)
            resourcesText.text = "Recursos: Fliotex (30%), Polarnyx (20%)";
            
        if (fuelCostText != null)
            fuelCostText.text = "Combustible: 50";
    }
    
    public void UpdatePlanetData(PlanetData planetData)
    {
        if (planetData == null) return;
        
        Debug.Log($"Updating panel info for planet {planetData.planetIndex}");
        
        if (planetNameText != null)
        {
            planetNameText.text = planetData.planetName;
            Debug.Log($"Updated planet name to: {planetNameText.text}");
        }
        else
        {
            Debug.LogWarning("planetNameText is null!");
        }
            
        if (biomeText != null)
        {
            biomeText.text = $"Bioma: {planetData.GetBiomeDescription()}";
            Debug.Log($"Updated biome text to: {biomeText.text}");
        }
        else
        {
            Debug.LogWarning("biomeText is null!");
        }
            
        if (resourcesText != null)
        {
            resourcesText.text = $"Recursos: {planetData.GetResourcesDescription()}";
            Debug.Log($"Updated resources text to: {resourcesText.text}");
        }
        else
        {
            Debug.LogWarning("resourcesText is null!");
        }
            
        if (fuelCostText != null)
        {
            fuelCostText.text = $"Combustible: {planetData.fuelCostToReach:F0}";
            Debug.Log($"Updated fuel cost text to: {fuelCostText.text}");
        }
        else
        {
            Debug.LogWarning("fuelCostText is null!");
        }
        
        // Actualizar estado del botón de viaje
        if (travelButton != null)
        {
            SolarSystemManager solarSystemManager = FindFirstObjectByType<SolarSystemManager>();
            bool canTravel = solarSystemManager != null && solarSystemManager.CanTravelToPlanet(planetData.planetIndex);
            travelButton.interactable = canTravel;
            
            // Cambiar color del botón según si se puede viajar
            var buttonColors = travelButton.colors;
            buttonColors.normalColor = canTravel ? Color.white : Color.gray;
            travelButton.colors = buttonColors;
        }
    }
    
    void UpdatePanelInfo()
    {
        // Este método se mantiene para compatibilidad, pero ahora usa UpdatePlanetData
        SolarSystemManager solarSystemManager = FindFirstObjectByType<SolarSystemManager>();
        if (solarSystemManager != null)
        {
            PlanetData planetData = solarSystemManager.GetPlanetData(currentPlanetIndex);
            if (planetData != null)
            {
                UpdatePlanetData(planetData);
            }
        }
    }
    
    void PositionPanelNearPlanet()
    {
        if (currentPlanet != null && panelObject != null && canvasTransform != null)
        {
            // Get the world position of the planet
            Vector3 planetWorldPos = currentPlanet.position;
            Debug.Log($"Planet world position: {planetWorldPos}");
            
            // Convert world position to screen position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(planetWorldPos);
            Debug.Log($"Screen position: {screenPos}");
            
            // Convert screen position to canvas position
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasTransform as RectTransform,
                screenPos,
                Camera.main,
                out canvasPos
            );
            
            // Position panel to the right of the planet, with some offset
            Vector2 offset = new Vector2(150, 0);
            panelObject.transform.localPosition = canvasPos + offset;
            
            Debug.Log($"Positioned panel for planet {currentPlanetIndex} at {panelObject.transform.localPosition}");
        }
        else
        {
            Debug.LogWarning($"Cannot position panel. currentPlanet: {currentPlanet}, panelObject: {panelObject}, canvasTransform: {canvasTransform}");
            
            // Fallback: position in center of screen
            if (panelObject != null)
            {
                panelObject.transform.localPosition = Vector3.zero;
                Debug.Log("Panel positioned at center as fallback");
            }
        }
    }
    
    public void ShowPanel()
    {
        Debug.Log($"ShowPanel called. Panel object: {panelObject?.name}");
        
        if (panelObject != null)
        {
            panelObject.SetActive(true);
            Debug.Log($"Panel for planet {currentPlanetIndex} is now visible. Active: {panelObject.activeSelf}");
        }
        else
        {
            Debug.LogError("panelObject is null in ShowPanel!");
        }
    }
    
    public void HidePanel()
    {
        Debug.Log("HidePanel called");
        
        if (panelObject != null)
        {
            panelObject.SetActive(false);
            currentPlanetIndex = -1;
            currentPlanet = null;
            Debug.Log("Panel hidden and state reset");
        }
        else
        {
            Debug.LogError("panelObject is null in HidePanel!");
        }
    }
    
    public bool IsVisible()
    {
        bool visible = panelObject != null && panelObject.activeSelf;
        Debug.Log($"IsVisible check: {visible}");
        return visible;
    }
    
    public void OnTravelButtonClicked()
    {
        if (currentPlanetIndex >= 0)
        {
            SolarSystemManager solarSystemManager = FindFirstObjectByType<SolarSystemManager>();
            if (solarSystemManager != null)
            {
                solarSystemManager.TravelToPlanet(currentPlanetIndex);
                HidePanel(); // Ocultar panel después del viaje
            }
        }
    }
} 