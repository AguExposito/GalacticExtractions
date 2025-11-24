using UnityEngine;

public class CircleSpriteCreator : MonoBehaviour
{
    [Header("Circle Settings")]
    public int resolution = 64;
    public float thickness = 2f;
    public Color circleColor = Color.white;
    
    public Sprite CreateCircleSprite()
    {
        int size = resolution;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = (size / 2f) - thickness;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 pixel = new Vector2(x, y);
                float distance = Vector2.Distance(pixel, center);
                
                Color color = Color.clear;
                
                // Create ring shape
                if (distance >= radius - thickness && distance <= radius)
                {
                    color = circleColor;
                }
                
                texture.SetPixel(x, y, color);
            }
        }
        
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = "CircleSprite";
        
        return sprite;
    }
} 