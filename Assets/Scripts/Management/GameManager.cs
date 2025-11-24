using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager principal que integra todos los sistemas del juego
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("System Managers")]
    public ResourceManager resourceManager;
    public FuelManager fuelManager;
    public ConnectionsManager connectionsManager;
    
    [Header("UI Managers")]
    public RadialPlanetController radialPlanetController;
    
    [Header("Game State")]
    public bool isInSolarSystemView = false;
    public bool isInPlanetView = false;
    public int currentPlanetIndex = 0;
    
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        InitializeSystems();
    }
    
    void InitializeSystems()
    {
        // Buscar managers si no existen
        if (resourceManager == null)
            resourceManager = FindFirstObjectByType<ResourceManager>();
            
        if (fuelManager == null)
            fuelManager = FindFirstObjectByType<FuelManager>();
            
        if (connectionsManager == null)
            connectionsManager = FindFirstObjectByType<ConnectionsManager>();
            
        if (radialPlanetController == null)
            radialPlanetController = FindFirstObjectByType<RadialPlanetController>();
        
        // Verificar que todos los sistemas estén inicializados
        if (fuelManager == null)
        {
            Debug.LogError("FuelManager no encontrado! Creando uno...");
            CreateFuelManager();
        }
        
        Debug.Log("GameManager inicializado correctamente");
    }
    
    void CreateFuelManager()
    {
        GameObject go = new GameObject("FuelManager");
        fuelManager = go.AddComponent<FuelManager>();
    }
    
    /// <summary>
    /// Cambia a la vista del sistema solar
    /// </summary>
    public void SwitchToSolarSystemView()
    {
        isInSolarSystemView = true;
        isInPlanetView = false;
        
        Debug.Log("Cambiando a vista del sistema solar");
    }
    
    /// <summary>
    /// Cambia a la vista de un planeta específico
    /// </summary>
    public void SwitchToPlanetView(int planetIndex)
    {
        currentPlanetIndex = planetIndex;
        isInPlanetView = true;
        isInSolarSystemView = false;
        
        Debug.Log($"Cambiando a vista del planeta {planetIndex}");
    }
    
    /// <summary>
    /// Añade combustible al jugador
    /// </summary>
    public void AddFuel(float amount)
    {
        if (fuelManager != null)
        {
            fuelManager.AddFuel(amount);
        }
    }
    
    /// <summary>
    /// Guarda el estado del juego
    /// </summary>
    public void SaveGameState()
    {
        PlayerPrefs.SetInt("CurrentPlanetIndex", currentPlanetIndex);
        PlayerPrefs.SetFloat("CurrentFuel", fuelManager != null ? fuelManager.currentFuel : 100f);
        PlayerPrefs.SetInt("Fliotex", resourceManager != null ? resourceManager.fliotex : 0);
        PlayerPrefs.SetInt("Polarnyx", resourceManager != null ? resourceManager.polarnyx : 0);
        PlayerPrefs.SetInt("Trevonita", resourceManager != null ? resourceManager.trevonita : 0);
        PlayerPrefs.Save();
        
        Debug.Log("Estado del juego guardado");
    }
    
    /// <summary>
    /// Carga el estado del juego
    /// </summary>
    public void LoadGameState()
    {
        currentPlanetIndex = PlayerPrefs.GetInt("CurrentPlanetIndex", 0);
        
        if (fuelManager != null)
        {
            fuelManager.currentFuel = PlayerPrefs.GetFloat("CurrentFuel", 100f);
        }
        
        if (resourceManager != null)
        {
            resourceManager.fliotex = PlayerPrefs.GetInt("Fliotex", 0);
            resourceManager.polarnyx = PlayerPrefs.GetInt("Polarnyx", 0);
            resourceManager.trevonita = PlayerPrefs.GetInt("Trevonita", 0);
        }
        
        Debug.Log("Estado del juego cargado");
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGameState();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveGameState();
        }
    }
} 