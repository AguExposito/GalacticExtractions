using UnityEngine;
using TMPro;

public class FuelManager : MonoBehaviour
{
    [Header("Fuel Settings")]
    public float currentFuel = 100f;
    public float maxFuel = 100f;
    public float fuelConsumptionRate = 0.5f;
    
    [Header("UI References")]
    public TextMeshProUGUI fuelDisplayText;
    
    private void Start()
    {
        UpdateFuelDisplay();
    }
    
    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Min(currentFuel + amount, maxFuel);
        UpdateFuelDisplay();
    }
    
    public void ConsumeFuel(float amount)
    {
        currentFuel = Mathf.Max(currentFuel - amount, 0f);
        UpdateFuelDisplay();
    }
    
    public bool HasEnoughFuel(float requiredAmount)
    {
        return currentFuel >= requiredAmount;
    }
    
    public float GetFuelPercentage()
    {
        return (currentFuel / maxFuel) * 100f;
    }
    
    public float GetCurrentFuel()
    {
        return currentFuel;
    }
    
    private void UpdateFuelDisplay()
    {
        if (fuelDisplayText != null)
        {
            fuelDisplayText.text = $"Fuel: {currentFuel:F1}/{maxFuel:F1}";
        }
    }
    
    // Method to create fuel from resources (crafting system)
    public bool CraftFuel(int fliotexCost, int polarnyxCost, int trevonitaCost)
    {
        ResourceManager resourceManager = FindFirstObjectByType<ResourceManager>();
        if (resourceManager == null) return false;
        
        // Check if we have enough resources
        if (resourceManager.fliotex >= fliotexCost && 
            resourceManager.polarnyx >= polarnyxCost && 
            resourceManager.trevonita >= trevonitaCost)
        {
            // Consume resources
            resourceManager.fliotex -= fliotexCost;
            resourceManager.polarnyx -= polarnyxCost;
            resourceManager.trevonita -= trevonitaCost;
            
            AddFuel(25f); // Add 25 fuel units
            return true;
        }
        
        return false;
    }
} 