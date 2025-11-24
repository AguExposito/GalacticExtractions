using UnityEngine;

/// <summary>
/// Script para manejar warnings de materiales que no son compatibles con 2D SRP Batcher
/// </summary>
public class MaterialWarningFixer : MonoBehaviour
{
    [Header("Material Warning Info")]
    [TextArea(3, 5)]
    public string warningInfo = @"Material 'TubeMoveEffect' has _TexelSize / _ST texture properties which are not supported by 2D SRP Batcher. SRP batching will be disabled for 2D Renderers using this Material.

This is just a warning and won't affect gameplay. The material will still work, but won't be batched for performance optimization.

To fix this warning (optional):
1. Find the TubeMoveEffect material
2. Remove _TexelSize or _ST properties from the shader
3. Or use a different material that's compatible with 2D SRP Batcher";

    void Start()
    {
        // Log the warning info for reference
        Debug.Log("=== MATERIAL WARNING INFO ===");
        Debug.Log(warningInfo);
        Debug.Log("This warning is not critical and won't affect the outline system.");
    }
    
    [ContextMenu("Show Warning Info")]
    public void ShowWarningInfo()
    {
        Debug.Log("=== MATERIAL WARNING INFO ===");
        Debug.Log(warningInfo);
    }
} 