using UnityEngine;

public class RadialMenuController : MonoBehaviour
{
    public int initialDegrees = 0;      // Grados iniciales para el primer botón (puedes ajustarlo)
    public float optionDistance = 50f;  // Distancia entre cada opción
    public GameObject[] options;        // Array de botones (debe ser vacío inicialmente)
    public int multiplier = 1;          // multiplicador de distancia

    private void OnEnable()
    {
        optionDistance *= multiplier;
        ArrangeMenu();
    }

    // Método para organizar el menú radial
    void ArrangeMenu()
    {
        // Filtrar solo las opciones activas
        GameObject[] activeOptions = System.Array.FindAll(options, option => option.activeInHierarchy);

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

    // Método para agregar un botón extra dinámicamente
    public void AddOption(GameObject newOption)
    {
        newOption.transform.SetParent(transform); // Añadimos el botón como hijo del menú
        ArrangeMenu(); // Reorganizamos el menú radial
    }

    public void ReparentAndResize(Transform tr) { 
        transform.parent.SetParent(tr);
        transform.parent.localPosition=Vector3.zero;
        int largestAxis = (int)Mathf.Max(tr.localScale.x, tr.localScale.y);
        multiplier = largestAxis;

    }
}
