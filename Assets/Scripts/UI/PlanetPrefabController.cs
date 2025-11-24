using UnityEngine;
using UnityEngine.UI;

public class PlanetPrefabController : MonoBehaviour
{
    [Header("Planet Settings")]
    public int planetIndex = 0;
    public bool isSun = false;
    
    private Image planetImage;
    private Button planetButton;
    private PlanetClickHandler clickHandler;
    
    void Awake()
    {
        // Ensure we have all necessary components
        planetImage = GetComponent<Image>();
        if (planetImage == null)
        {
            planetImage = gameObject.AddComponent<Image>();
        }
        
        // Ensure we have a Button component
        planetButton = GetComponent<Button>();
        if (planetButton == null)
        {
            planetButton = gameObject.AddComponent<Button>();
            Debug.Log("Added Button component to planet");
        }
        
        // Ensure raycast target is enabled
        if (planetImage != null)
        {
            planetImage.raycastTarget = true;
        }
        
        // Add click handler if it doesn't exist
        clickHandler = GetComponent<PlanetClickHandler>();
        if (clickHandler == null)
        {
            clickHandler = gameObject.AddComponent<PlanetClickHandler>();
        }
    }
    
    void Start()
    {
        Debug.Log($"PlanetPrefabController initialized for planet {planetIndex}");
    }
    
    public void SetPlanetIndex(int index)
    {
        planetIndex = index;
    }
    
    public void SetSprite(Sprite sprite)
    {
        if (planetImage != null)
        {
            planetImage.sprite = sprite;
        }
    }
    
    public void SetInteractable(bool interactable)
    {
        if (planetButton != null)
        {
            planetButton.interactable = interactable;
        }
    }
} 