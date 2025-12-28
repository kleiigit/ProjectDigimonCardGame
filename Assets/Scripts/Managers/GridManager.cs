using System;
using System.Collections.Generic;
using UnityEngine;
using SinuousProductions;

public class GridManager : MonoBehaviour
{
    public List<GridCell> playerRedCells;
    public List<GridCell> playerBlueCells;
    public GameObject cardPrefab;
    private FieldCard fieldCardScript;

    public bool AddObjectToGrid(Card cardData, int gridPosition)
    {
        GridCell targetCell = null;
        bool isRedOwnerCell = false;

        Debug.Log("Iniciando AddObjectToGrid com card: " + cardData.cardName);

        targetCell = playerRedCells.Find(cell => cell.gridIndex == gridPosition);
        if (targetCell != null)
        {
            isRedOwnerCell = true;
        }
        else
        {
            targetCell = playerBlueCells.Find(cell => cell.gridIndex == gridPosition);
        }

        if (targetCell != null)
        {
            if (targetCell.cellFull)
            {
                Debug.Log($"Célula {gridPosition} já está ocupada.");
                return false;
            }

            GameObject newObj = Instantiate(cardPrefab);
            newObj.name = cardData.cardName.ToUpper() + " - " + cardData.cardID;
            newObj.GetComponent<CardDisplay>().cardData = cardData;
            newObj.GetComponent<CardDisplay>().UpdateCardDisplay();

            fieldCardScript = newObj.GetComponent<FieldCard>();
            fieldCardScript.enabled = true;
            fieldCardScript.parentCell = targetCell;

            // Define o cardOwner com base no gridOwner da célula
            fieldCardScript.SetCardOwnerFromGrid();

            newObj.transform.SetParent(targetCell.transform, false);
            newObj.transform.localPosition = Vector3.zero;

            RectTransform rectTransform = newObj.GetComponent<RectTransform>();
            if (isRedOwnerCell)
            {
                rectTransform.localScale = new Vector3(-0.56f, -0.56f, 0.56f); // Invertido
            }
            else
            {
                rectTransform.localScale = new Vector3(0.56f, 0.56f, 0.56f); // Normal
            }

            newObj.layer = 7;
            targetCell.cellFull = true;
            GameManager.Instance.AddCardToField(newObj);
            Debug.Log($"Objeto criado na célula {gridPosition}.");
            return true;
        }
        else
        {
            Debug.LogWarning($"Nenhuma célula encontrada para a posição {gridPosition}.");
            return false;
        }
    }


}


