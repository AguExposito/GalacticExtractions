using UnityEngine;
using UnityEngine.UI;

public class OrbitLineController : MonoBehaviour
{
    [Header("Orbit Settings")]
    public float rotationSpeed = 10f;
    public float radius = 100f;
    
    private Image orbitImage;
    private RectTransform rectTransform;
    
    void Awake()
    {
        // Initialize components in Awake to ensure they're available
        orbitImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform == null)
        {
            Debug.LogError("OrbitLineController: RectTransform component not found!");
        }
    }
    
    void Start()
    {
        // Set initial size based on radius
        if (rectTransform != null)
        {
            float thickness = 2f;
            rectTransform.sizeDelta = new Vector2(radius * 2 + thickness, radius * 2 + thickness);
        }
    }
    
    void Update()
    {
        // Rotate the orbit line
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
    
    public void SetRadius(float newRadius)
    {
        radius = newRadius;
        if (rectTransform != null)
        {
            float thickness = 2f;
            rectTransform.sizeDelta = new Vector2(radius * 2 + thickness, radius * 2 + thickness);
        }
        else
        {
            Debug.LogError("OrbitLineController: RectTransform is null in SetRadius!");
        }
    }
    
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }
} 