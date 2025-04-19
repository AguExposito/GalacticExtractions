using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject UIBuildingMenu;
    public void UIBuildingMenuChangeState() 
    { 
        UIBuildingMenu.SetActive(!UIBuildingMenu.activeInHierarchy);
    }
}
