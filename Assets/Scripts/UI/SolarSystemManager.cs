using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlanetData
{
    public int planetIndex;
    public string planetName;
    public BiomeType primaryBiome;
    public BiomeType secondaryBiome;
    public Vector3 position;
    public bool isExplored;
    public bool isPlayerPlanet;
    public float fuelCostToReach;
    
    // Recursos disponibles en el planeta
    public Dictionary<OreNames, float> resourceProbabilities = new Dictionary<OreNames, float>();
    
    public PlanetData(int index, Vector3 pos)
    {
        planetIndex = index;
        planetName = $"Planeta {index + 1}";
        position = pos;
        isExplored = false;
        isPlayerPlanet = false;
        
        // Generar biomas aleatorios
        GenerateRandomBiomes();
        
        // Calcular probabilidades de recursos basadas en biomas
        CalculateResourceProbabilities();
    }
    
    void GenerateRandomBiomes()
    {
        BiomeType[] allBiomes = { BiomeType.ground, BiomeType.dirt, BiomeType.desert, BiomeType.snow };
        
        // Seleccionar dos biomas diferentes
        int primaryIndex = Random.Range(0, allBiomes.Length);
        int secondaryIndex;
        do
        {
            secondaryIndex = Random.Range(0, allBiomes.Length);
        } while (secondaryIndex == primaryIndex);
        
        primaryBiome = allBiomes[primaryIndex];
        secondaryBiome = allBiomes[secondaryIndex];
    }
    
    void CalculateResourceProbabilities()
    {
        // Resetear probabilidades
        resourceProbabilities.Clear();
        
        // Probabilidades base por bioma
        Dictionary<BiomeType, Dictionary<OreNames, float>> biomeResources = new Dictionary<BiomeType, Dictionary<OreNames, float>>
        {
            { BiomeType.ground, new Dictionary<OreNames, float> { { OreNames.Fliotex, 0.3f }, { OreNames.Polarnyx, 0.2f }, { OreNames.Trevonita, 0.1f } } },
            { BiomeType.dirt, new Dictionary<OreNames, float> { { OreNames.Fliotex, 0.4f }, { OreNames.Polarnyx, 0.1f }, { OreNames.Trevonita, 0.05f } } },
            { BiomeType.desert, new Dictionary<OreNames, float> { { OreNames.Fliotex, 0.1f }, { OreNames.Polarnyx, 0.4f }, { OreNames.Trevonita, 0.2f } } },
            { BiomeType.snow, new Dictionary<OreNames, float> { { OreNames.Fliotex, 0.05f }, { OreNames.Polarnyx, 0.2f }, { OreNames.Trevonita, 0.5f } } }
        };
        
        // Combinar probabilidades de ambos biomas
        foreach (var resource in biomeResources[primaryBiome])
        {
            float primaryProb = resource.Value;
            float secondaryProb = biomeResources[secondaryBiome].ContainsKey(resource.Key) ? 
                biomeResources[secondaryBiome][resource.Key] : 0f;
            
            // Promedio ponderado (bioma primario tiene más peso)
            resourceProbabilities[resource.Key] = (primaryProb * 0.7f) + (secondaryProb * 0.3f);
        }
        
        // Añadir recursos del bioma secundario que no estén en el primario
        foreach (var resource in biomeResources[secondaryBiome])
        {
            if (!resourceProbabilities.ContainsKey(resource.Key))
            {
                resourceProbabilities[resource.Key] = resource.Value * 0.3f;
            }
        }
    }
    
    public float CalculateFuelCost(Vector3 fromPosition)
    {
        float distance = Vector3.Distance(fromPosition, position);
        // Costo base: 10 combustible por 100 unidades de distancia
        fuelCostToReach = Mathf.Ceil(distance / 100f) * 10f;
        return fuelCostToReach;
    }
    
    public string GetBiomeDescription()
    {
        return $"{primaryBiome.ToString().ToUpper()} / {secondaryBiome.ToString().ToUpper()}";
    }
    
    public string GetResourcesDescription()
    {
        List<string> resources = new List<string>();
        foreach (var resource in resourceProbabilities)
        {
            if (resource.Value > 0.1f) // Solo mostrar recursos con probabilidad > 10%
            {
                resources.Add($"{resource.Key} ({(resource.Value * 100):F0}%)");
            }
        }
        return string.Join(", ", resources);
    }
}

public class SolarSystemManager : MonoBehaviour
{
    [Header("System Settings")]
    public float[] orbitRadii = { 200f, 400f, 600f };
    public int[] planetsPerOrbit = { 3, 4, 5 };
    public int playerPlanetIndex = 0;
    
    [Header("References")]
    public RadialPlanetController radialPlanetController;
    public PlanetInfoPanel planetInfoPanel;
    public FuelManager fuelManager;
    
    [Header("Planet Data")]
    public List<PlanetData> planets = new List<PlanetData>();
    
    private Vector3 playerCurrentPosition;
    
    void Start()
    {
        InitializePlanets();
        SetupPlayerPlanet();
        ConnectWithUI();
    }
    
    void InitializePlanets()
    {
        planets.Clear();
        
        int planetIndex = 0;
        for (int orbitIndex = 0; orbitIndex < orbitRadii.Length; orbitIndex++)
        {
            float radius = orbitRadii[orbitIndex];
            int planetCount = planetsPerOrbit[orbitIndex];
            
            for (int planetInOrbit = 0; planetInOrbit < planetCount; planetInOrbit++)
            {
                float angle = (360f / planetCount) * planetInOrbit;
                Vector3 position = CalculatePlanetPosition(radius, angle);
                
                PlanetData planetData = new PlanetData(planetIndex, position);
                planets.Add(planetData);
                
                planetIndex++;
            }
        }
        
        Debug.Log($"Initialized {planets.Count} planets");
    }
    
    void SetupPlayerPlanet()
    {
        if (playerPlanetIndex >= 0 && playerPlanetIndex < planets.Count)
        {
            planets[playerPlanetIndex].isPlayerPlanet = true;
            planets[playerPlanetIndex].isExplored = true;
            playerCurrentPosition = planets[playerPlanetIndex].position;
            
            Debug.Log($"Player planet set to: {planets[playerPlanetIndex].planetName}");
        }
    }
    
    void ConnectWithUI()
    {
        if (radialPlanetController == null)
            radialPlanetController = FindFirstObjectByType<RadialPlanetController>();
            
        if (planetInfoPanel == null)
            planetInfoPanel = FindFirstObjectByType<PlanetInfoPanel>();
            
        if (fuelManager == null)
            fuelManager = FindFirstObjectByType<FuelManager>();
    }
    
    Vector3 CalculatePlanetPosition(float radius, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector3(
            Mathf.Cos(radians) * radius,
            Mathf.Sin(radians) * radius,
            0
        );
    }
    
    public PlanetData GetPlanetData(int planetIndex)
    {
        if (planetIndex >= 0 && planetIndex < planets.Count)
        {
            return planets[planetIndex];
        }
        return null;
    }
    
    public void UpdatePlanetInfo(int planetIndex)
    {
        PlanetData planetData = GetPlanetData(planetIndex);
        if (planetData != null)
        {
            // Calcular costo de combustible desde la posición actual del jugador
            planetData.CalculateFuelCost(playerCurrentPosition);
            
            // Actualizar el panel de información
            if (planetInfoPanel != null)
            {
                planetInfoPanel.UpdatePlanetData(planetData);
            }
        }
    }
    
    public bool CanTravelToPlanet(int planetIndex)
    {
        PlanetData planetData = GetPlanetData(planetIndex);
        if (planetData == null) return false;
        
        // Verificar si hay suficiente combustible
        if (fuelManager != null)
        {
            return fuelManager.HasEnoughFuel(planetData.fuelCostToReach);
        }
        
        return true; // Si no hay FuelManager, permitir viaje
    }
    
    public void TravelToPlanet(int planetIndex)
    {
        PlanetData planetData = GetPlanetData(planetIndex);
        if (planetData == null) return;
        
        if (CanTravelToPlanet(planetIndex))
        {
            // Consumir combustible
            if (fuelManager != null)
            {
                fuelManager.ConsumeFuel(planetData.fuelCostToReach);
            }
            
            // Actualizar estado del jugador
            playerCurrentPosition = planetData.position;
            planetData.isExplored = true;
            
            // Cambiar planeta del jugador
            if (playerPlanetIndex >= 0 && playerPlanetIndex < planets.Count)
            {
                planets[playerPlanetIndex].isPlayerPlanet = false;
            }
            planetData.isPlayerPlanet = true;
            playerPlanetIndex = planetIndex;
            
            Debug.Log($"Traveled to {planetData.planetName}. Fuel cost: {planetData.fuelCostToReach}");
            
            // Actualizar outlines de los planetas
            UpdatePlanetOutlines();
            
            // Aquí podrías cambiar de escena o vista
            GameManager.Instance.SwitchToPlanetView(planetIndex);
        }
        else
        {
            Debug.LogWarning($"Not enough fuel to travel to {planetData.planetName}. Required: {planetData.fuelCostToReach}");
        }
    }
    
    public void UpdatePlanetOutlines()
    {
        if (radialPlanetController != null)
        {
            for (int i = 0; i < planets.Count; i++)
            {
                PlanetData planetData = GetPlanetData(i);
                if (planetData != null)
                {
                    radialPlanetController.UpdatePlanetOutline(i, planetData);
                }
            }
        }
    }
    
    public List<PlanetData> GetPlanetsInOrbit(int orbitIndex)
    {
        List<PlanetData> orbitPlanets = new List<PlanetData>();
        
        int startIndex = 0;
        for (int i = 0; i < orbitIndex; i++)
        {
            startIndex += planetsPerOrbit[i];
        }
        
        int endIndex = startIndex + planetsPerOrbit[orbitIndex];
        for (int i = startIndex; i < endIndex && i < planets.Count; i++)
        {
            orbitPlanets.Add(planets[i]);
        }
        
        return orbitPlanets;
    }
} 