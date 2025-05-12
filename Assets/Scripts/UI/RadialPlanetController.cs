using UnityEngine;
using UnityEngine.UI;

public class RadialPlanetController : MonoBehaviour
{
    public int initialDegrees = 0;      // Grados iniciales para el primer bot�n (puedes ajustarlo)
    public float optionDistance = 50f;  // Distancia entre cada opci�n
    public float rotationVelocity = 0f;  // Distancia entre cada opci�n
    public GameObject[] planets;        // Array de botones (debe ser vac�o inicialmente)
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

    // M�todo para organizar el men� radial
    void ArrangeMenu()
    {
        // Filtrar solo las opciones activas
        GameObject[] activeOptions = System.Array.FindAll(planets, option => option.activeInHierarchy);

        // Calcular el n�mero de opciones activas
        int numActiveOptions = activeOptions.Length;

        if (numActiveOptions == 0) return; // Si no hay opciones activas, no hacemos nada

        // Distribuir las opciones activas radialmente
        for (int i = 0; i < numActiveOptions; i++)
        {
            // Obtenemos el RectTransform del objeto (bot�n)
            RectTransform optionRect = activeOptions[i].GetComponent<RectTransform>();

            // Calculamos el �ngulo de la opci�n actual (se distribuyen uniformemente en el c�rculo)
            float angle = (i * (360f / numActiveOptions)) + initialDegrees;

            // Convertir el �ngulo a radianes
            float angleInRadians = Mathf.Deg2Rad * angle;

            // Calcular la nueva posici�n de cada opci�n (bot�n) en coordenadas polares
            Vector2 position = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)) * optionDistance;

            // Aplicamos la nueva posici�n al RectTransform
            optionRect.localPosition = position;
        }
    }

}


