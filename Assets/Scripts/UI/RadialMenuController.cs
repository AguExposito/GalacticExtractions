using UnityEngine;

public class RadialMenuController : MonoBehaviour
{
    public int initialDegrees = 0;      // Grados iniciales para el primer bot�n (puedes ajustarlo)
    public float optionDistance = 50f;  // Distancia entre cada opci�n
    public GameObject[] options;        // Array de botones (debe ser vac�o inicialmente)
    public int multiplier = 1;          // multiplicador de distancia

    private void OnEnable()
    {
        optionDistance *= multiplier;
        ArrangeMenu();
    }

    // M�todo para organizar el men� radial
    void ArrangeMenu()
    {
        // Filtrar solo las opciones activas
        GameObject[] activeOptions = System.Array.FindAll(options, option => option.activeInHierarchy);

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

    // M�todo para agregar un bot�n extra din�micamente
    public void AddOption(GameObject newOption)
    {
        newOption.transform.SetParent(transform); // A�adimos el bot�n como hijo del men�
        ArrangeMenu(); // Reorganizamos el men� radial
    }

    public void ReparentAndResize(Transform tr) { 
        transform.parent.SetParent(tr);
        transform.parent.localPosition=Vector3.zero;
        int largestAxis = (int)Mathf.Max(tr.localScale.x, tr.localScale.y);
        multiplier = largestAxis;

    }
}
