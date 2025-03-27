using UnityEngine;
using UnityEngine.Tilemaps;

public class GridTest : MonoBehaviour
{
    public GameObject objPrefab, cube;
    public Grid grid;
    public GridTestInput gridInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 selectedPosition= gridInput.GetSelectedMapPosition();
        Vector3Int cellPosition = grid.WorldToCell(selectedPosition);
        cube.transform.position = grid.GetCellCenterWorld(cellPosition);

        if (gridInput.GetPlacementInput())
        {
            Instantiate(objPrefab, cube.transform.position, Quaternion.identity);
        }
    }
}
