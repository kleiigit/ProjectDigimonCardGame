using ProjectScript.Enums;
using ProjectScript.Selection;
using SinuousProductions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionDataManager : MonoBehaviour
{
    static DisplayListCards displayCards;
    private void Start()
    {
        displayCards = FindFirstObjectByType<DisplayListCards>();
    }
    public static void CostCard(PlayerSetup setup, Dictionary<CardColor, int> colorCost, System.Action onCostPaid)
    {
        if (colorCost.Values.Sum() == 0)
        {
            Debug.Log("Custo 0, automaticamente concluido!");
            onCostPaid?.Invoke();
            return;
        }
        setup.dataPile.ListDataCardsButton();
        SelectionManager.Instance.StartSelection(new SelectionRequest(colorCost.Values.Sum(), 
            new SelectionCriteria
            { 
                placeRequirements = FieldPlace.DataPile, 
                colorRequirements = colorCost
            },
            
            selected =>
                {
                    foreach (var item in selected)
                    {
                        GameObject go = ((MonoBehaviour)item).gameObject;
                        Debug.Log("Selecionado: " + go.name);
                        string dataID = go.GetComponent<CardDisplay>().cardData.cardID;
                        GameObject cardDataObj = setup.listDataObj.First(p => p.GetComponent<CardDisplay>().cardData.cardID == dataID);
                        setup.dataPile.DiscardData(cardDataObj);
                    }
                    displayCards.Hide();
                    onCostPaid?.Invoke();
                }
            ),
            setup.GetComponent<RectTransform>()
        );
    }

}
