using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class TileHealthUI : MonoBehaviour
{
    public GameObject healthBarPrefab; // Prefab de la barra de vida
    public int maxHealthBars = 3; // M�ximo de barras visibles
    public Queue<GameObject> healthBars = new Queue<GameObject>(); // Cola para reciclar las barras
    MapGenerator mapGenerator;
    private void Start()
    {
        mapGenerator=FindAnyObjectByType<MapGenerator>();
        // Crear y desactivar las barras de vida iniciales
        for (int i = 0; i < maxHealthBars; i++)
        {
            GameObject bar = Instantiate(healthBarPrefab, transform);
            bar.SetActive(false);
            healthBars.Enqueue(bar);
        }
    }

    public void ShowHealthBar(Vector3Int tilePosition, float healthPercentage)
    {
        GameObject healthBar = GetAvailableHealthBar();
        Vector3 worldPosition = TileToWorldPosition(tilePosition);

        healthBar.transform.position = worldPosition;
        healthBar.SetActive(true);

        // Ajustar el tama�o o color seg�n la vida restante
        healthBar.transform.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Image>().fillAmount= healthPercentage;
    }

    private GameObject GetAvailableHealthBar()
    {
        foreach (GameObject bar in healthBars)
        {
            if (!bar.activeSelf) return bar;
        }

        // Si no hay barras disponibles, reciclar la m�s antigua
        GameObject oldestBar = healthBars.Dequeue();
        oldestBar.SetActive(false); // Asegurar que se resetee antes de reutilizar
        healthBars.Enqueue(oldestBar); // Moverla al final de la cola
        return oldestBar;
    }

    private Vector3 TileToWorldPosition(Vector3Int tilePosition)
    {
        Vector3 worldPosition = mapGenerator.wallsTilemap.GetCellCenterWorld(tilePosition);
        worldPosition.y += 0.5f; // Ajustar la posici�n para que la barra est� sobre el tile
        return worldPosition;
    }
}
