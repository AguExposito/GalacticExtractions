using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public int fliotex;
    public int polarnyx;
    public int trevonita;

    public void SetResources(int amount, TextMeshProUGUI biomeResourcesText) 
    {
        int currentAmount = GetResourceAmount(biomeResourcesText);
        biomeResourcesText.text = biomeResourcesText.name + " = " + (currentAmount + amount);

        switch (biomeResourcesText.name)
        {
            case "Fliotex": { fliotex += amount; } break;
            case "Polarnyx": { polarnyx += amount; } break;
            case "Trevonita": { trevonita += amount; }break;
        }
    }

    int GetResourceAmount(TextMeshProUGUI biomeResourcesText) {
        switch (biomeResourcesText.name)
        {
            case "Fliotex": { return fliotex; } ;
            case "Polarnyx": { return polarnyx; } ;
            case "Trevonita": { return trevonita; } ;
            default: { return 0; };
        }
    }

}
