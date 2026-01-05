using ProjectScript.Enums;
using ProjectScript.Selection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionDataManager : MonoBehaviour
{
    public static void CostCard(Dictionary<CardColor, int> colorCost)
    {
        Debug.Log("Selection activated!");
        if(colorCost.Values.Sum() == 0) return;
        SelectionManager.Instance.StartSelection(
            new SelectionRequest(colorCost.Values.Sum(), // quantidade de objetos a selecionar
        selected => {
            // ESTE bloco só executa após a seleção ser confirmada
            foreach (var item in selected)
            {
                var go = ((MonoBehaviour)item).gameObject;
                Debug.Log("Selecionado: " + go.name);
            }
        }
         )
        );
    }
}
