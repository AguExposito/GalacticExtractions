using UnityEngine;

public class OutlineShaderSetup : MonoBehaviour
{
    [Header("Shader Settings")]
    public Shader outlineShader;
    public Material outlineMaterial;
    
    [Header("Outline Properties")]
    public float outlineWidth = 2f;
    public Color defaultOutlineColor = Color.white;
    
    void Start()
    {
        SetupOutlineMaterial();
    }
    
    void SetupOutlineMaterial()
    {
        // Try to find the shader
        if (outlineShader == null)
        {
            outlineShader = Shader.Find("Custom/SpriteOutline");
            if (outlineShader == null)
            {
                outlineShader = Shader.Find("SpriteOutline");
            }
            if (outlineShader == null)
            {
                Debug.LogWarning("SpriteOutline shader not found! Please ensure the shader is properly imported.");
                return;
            }
        }
        
        // Create a template material for reference (optional)
        if (outlineMaterial == null)
        {
            outlineMaterial = new Material(outlineShader);
            outlineMaterial.name = "OutlineShaderTemplate";
        }
        
        // Set default properties
        outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
        outlineMaterial.SetColor("_OutlineColor", defaultOutlineColor);
        
        Debug.Log("Outline shader setup completed");
    }
    
    // Method to get the outline shader
    public Shader GetOutlineShader()
    {
        return outlineShader;
    }
    
    // Method to get the outline material (template)
    public Material GetOutlineMaterial()
    {
        return outlineMaterial;
    }
    
    // Method to update outline properties
    public void UpdateOutlineProperties(float width, Color color)
    {
        if (outlineMaterial != null)
        {
            outlineMaterial.SetFloat("_OutlineWidth", width);
            outlineMaterial.SetColor("_OutlineColor", color);
        }
    }
} 