using System;
using System.Collections.Generic;
using UnityEngine;
using SinuousProductions;
using ProjectScript.Enums;

public class GridManager : MonoBehaviour
{
    public List<GridCell> playerRedCells;
    public List<GridCell> playerBlueCells;
    private FieldCard fieldCardScript;
    public bool AddObjectToGrid(CardDisplay card, int gridPosition, PlayerSide playerSide)
    {
        Card cardData = card.cardData;
        GridCell targetCell = null;
        bool isRedOwnerCell = false;

        //Debug.Log("Iniciando AddObjectToGrid com card: " + cardData.cardName);

        targetCell = playerRedCells.Find(cell => cell.gridIndex == gridPosition);

        if (targetCell != null)
        {
            isRedOwnerCell = true;
        }
        else
        {
            targetCell = playerBlueCells.Find(cell => cell.gridIndex == gridPosition);
        }

        if (targetCell == null)
        {
            Debug.LogWarning($"Nenhuma célula encontrada para a posição {gridPosition}.");
            return false;
        }

        if (targetCell.cellFull)
        {
            Debug.Log($"Célula {gridPosition} já está ocupada.");
            return false;
        }

        GameObject newObj = Instantiate(
            GameManager.cardPrefab,
            targetCell.transform.position,
            targetCell.transform.rotation,
            targetCell.transform
        );

        newObj.name = cardData.cardName.ToUpper() + " - " + cardData.cardID;
        newObj.GetComponent<RectTransform>().localScale = new Vector3(0.55f, 0.55f, 1f);
        newObj.GetComponent<MenuCardManager>().handOwner = playerSide;

        CardDisplay cardDisplay = newObj.GetComponent<CardDisplay>();
        cardDisplay.cardData = cardData;
        cardDisplay.UpdateCardDisplay();

        fieldCardScript = newObj.GetComponent<FieldCard>();
        fieldCardScript.enabled = true;
        fieldCardScript.parentCell = targetCell;

        // Ajustes locais
        
        newObj.layer = 7;

        targetCell.cellFull = true;
        GameManager.Instance.AddCardToField(newObj);
        return true;
    }


}


