using TMPro;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    private float deltaTime = 0.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Application.targetFrameRate = 60; // Fijar a 60 FPS
        QualitySettings.vSyncCount = 0;  // Desactivar V-Sync para evitar limitaciones
    }


    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = Mathf.Ceil(fps).ToString() + " FPS";
    }
}
