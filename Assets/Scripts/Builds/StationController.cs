using UnityEngine;

public class StationController : MonoBehaviour
{
    public int squareRangeX = 9;
    public int squareRangeY = 9;
    public GameObject effectRange;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LineRenderer lr= effectRange.GetComponent<LineRenderer>();
        SetLRCorners();
    }

    Vector3[] GetLRCorners(LineRenderer lr) {
        Vector3[] corners = new Vector3[lr.positionCount];
        for (int i = 0; i < lr.positionCount; i++)
        {
            corners[i] = lr.GetPosition(i);
        }
        return corners;
    }

    void SetLRCorners() {
        float rangeX = ((float)squareRangeX / 2);
        float rangeY = ((float)squareRangeY / 2);
        Vector3[] corners = new Vector3[]
        {
            new Vector3(-rangeX,-rangeY,0),//BL
            new Vector3(rangeX,-rangeY,0),//BR
            new Vector3(rangeX,rangeY,0),//TR
            new Vector3(-rangeX,rangeY,0),//TL
        };
        effectRange.GetComponent<LineRenderer>().SetPositions(corners);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
