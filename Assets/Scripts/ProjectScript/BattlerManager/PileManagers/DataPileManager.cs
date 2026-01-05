using SinuousProductions;
using ProjectScript.Enums;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ProjectScript.Interfaces;
using UnityEditor.PackageManager;

public class DataPileManager : MonoBehaviour, IPile
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    private PlayerSetup setup;
    public Transform dataPileTransform;
    public Dictionary<CardColor, int> colorCounts = new Dictionary<CardColor, int>();
    [Header("Configurações de posicionamento 2D")]
    public Direction stackDirection = Direction.Down;
    public float cardRotationZ = 0f;
    public float cardSpacing = 30f;
    public float cardScale = 50f;

    void Awake()
    {
        setup = GetComponent<PlayerSetup>();
        ColorDictionary();
    }

    public void AddCard(Card cardData)
    {
        GameObject newCard = Instantiate(GameManager.cardPrefab, dataPileTransform.position, Quaternion.Euler(0f, 0f, cardRotationZ), dataPileTransform);

        newCard.name = cardData.cardName.ToUpper() + " - " + cardData.cardID;
        newCard.GetComponent<CardDisplay>().cardData = cardData;
        newCard.GetComponent<CardDisplay>().UpdateCardDisplay();
        newCard.transform.localScale = new Vector3(cardScale, cardScale, 1);
        newCard.layer = LayerMask.NameToLayer("Data");

        setup.listDataObj.Add(newCard);
        UpdateColorCount();
        UpdateVisuals();

        Debug.Log($"[DataPileManager] Carta adicionada à DataPile do {setup.setPlayer}: {cardData.cardName}");
    }
    public void RemoveCard(GameObject cardObject)
    {
        setup.listDataObj.Remove(cardObject);
        Destroy(cardObject);
    }
    private void UpdateColorCount()
    {
        // Zera os contadores
        foreach (CardColor color in System.Enum.GetValues(typeof(CardColor)))
        {
            colorCounts[color] = 0;
        }

        foreach (GameObject cardObject in setup.listDataObj)
        {
            Card card = cardObject.GetComponent<CardDisplay>().cardData;
            foreach (var color in card.cardColor)
            {
                if (colorCounts.ContainsKey(color))
                    colorCounts[color]++;
            }
        }
    }
    public void UpdateVisuals()
    {
        setup.listDataObj.RemoveAll(card => card == null);

        Vector3 directionVector = stackDirection switch
        {
            Direction.Up => Vector3.up,
            Direction.Down => Vector3.down,
            Direction.Left => Vector3.left,
            Direction.Right => Vector3.right,
            _ => Vector3.zero
        };

        int cardCount = setup.listDataObj.Count;
        float spacing = cardSpacing;

        if (cardCount > 5)
        {
           spacing = (spacing * 5) / cardCount ;
        }

        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = setup.listDataObj[i];
            card.transform.localRotation = Quaternion.Euler(0f, 0f, cardRotationZ);
            int reversedIndex = cardCount - 1 - i;
            card.transform.localPosition = directionVector * reversedIndex * spacing;

            SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = reversedIndex;
            }
        }
    }

    private void ColorDictionary()
    {
        foreach (CardColor color in System.Enum.GetValues(typeof(CardColor)))
        {
            colorCounts[color] = 0;
        }
    }
    public void MoveCardFromHandToDataPile(GameObject cardObject)
    {
        setup.hand.RemoveCard(cardObject);
        setup.dataPile.AddCard(cardObject.GetComponent<CardDisplay>().cardData);
    }
    public bool HasSufficientDataToPlayCard(Dictionary<CardColor,int> costColor)
    {
        foreach(var datacolor in costColor)
        {
            //Debug.Log($"Custo de Data: {datacolor.Key}: {datacolor.Value}");
        }
        foreach (var datacolor in colorCounts)
        {
            if (datacolor.Value == 0) continue;
            //Debug.Log($"Banco de Data: {datacolor.Key}: {datacolor.Value}");
        }

        Dictionary<CardColor, int> tempCounts = new Dictionary<CardColor, int>(colorCounts);

        // atribui as cores neutras ao total
        int availableColorless = tempCounts.GetValueOrDefault(CardColor.Colorless, 0);
        int totalResourcesAvailable = 0;
        foreach (var pair in tempCounts)
        {
            totalResourcesAvailable += pair.Value;
        }

        int totalCost = costColor.Sum(x => x.Value);

        if (totalResourcesAvailable < totalCost)
        {
            Debug.Log("total cost maior que recursos disponiveis");
            return false;
        }

        // Processa custo por cor como antes
        foreach(var pair in costColor)
        {
            CardColor requiredColor = pair.Key;
            int requiredAmount = pair.Value;

            int availableAmount = tempCounts.GetValueOrDefault(requiredColor, 0);

            if (availableAmount >= requiredAmount)
            {
                tempCounts[requiredColor] -= requiredAmount;
            }
            else
            {
                int deficit = requiredAmount - availableAmount;
                if (availableColorless >= deficit)
                {
                    tempCounts[requiredColor] = 0;
                    availableColorless -= deficit;
                }
                else
                {
                    Debug.Log("sem cor indisponiveis");
                    return false;
                }
            }
        }
        Debug.Log("Custos compridos");
        return true;
    }

    
}
