using UnityEngine;
using UnityEngine.UI;

public class RadialPlanetController : MonoBehaviour
{
    public int initialDegrees = 0;      // Grados iniciales para el primer botón (puedes ajustarlo)
    public float optionDistance = 50f;  // Distancia entre cada opción
    public float rotationVelocity = 0f;  // Distancia entre cada opción
    public GameObject[] planets;        // Array de botones (debe ser vacío inicialmente)
    public float multiplier = 1;          // multiplicador de velocidad
    public ConnectionsManager connectionManager;
    private void OnEnable()
    {
        connectionManager = FindFirstObjectByType<ConnectionsManager>();
        initialDegrees = Random.Range(0, 360);
        rotationVelocity = Random.Range(-3f, 3f);
        if (rotationVelocity > -1 && rotationVelocity < 1) {
            if (rotationVelocity > 0)
            {
                rotationVelocity = 1;
            }
            else { rotationVelocity = -1; }
        }
        ArrangeMenu();
    }
    private void Update()
    {
        gameObject.transform.Rotate(Vector3.forward * rotationVelocity * multiplier * Time.deltaTime, Space.Self);
    }

    // Método para organizar el menú radial
    void ArrangeMenu()
    {
        // Filtrar solo las opciones activas
        GameObject[] activeOptions = System.Array.FindAll(planets, option => option.activeInHierarchy);

        // Calcular el número de opciones activas
        int numActiveOptions = activeOptions.Length;

        if (numActiveOptions == 0) return; // Si no hay opciones activas, no hacemos nada

        // Distribuir las opciones activas radialmente
        for (int i = 0; i < numActiveOptions; i++)
        {
            // Obtenemos el RectTransform del objeto (botón)
            RectTransform optionRect = activeOptions[i].GetComponent<RectTransform>();

            // Calculamos el ángulo de la opción actual (se distribuyen uniformemente en el círculo)
            float angle = (i * (360f / numActiveOptions)) + initialDegrees;

            // Convertir el ángulo a radianes
            float angleInRadians = Mathf.Deg2Rad * angle;

            // Calcular la nueva posición de cada opción (botón) en coordenadas polares
            Vector2 position = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)) * optionDistance;

            // Aplicamos la nueva posición al RectTransform
            optionRect.localPosition = position;
        }
    }

}


