using ProjectScript.Enums;
using ProjectScript.Interfaces;
using SinuousProductions;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEditor.Progress;

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
    private DisplayListCards displayListCards;

    public Transform dataPileTransform;
    public Dictionary<CardColor, int> colorCounts = new Dictionary<CardColor, int>();
    [Header("Configurações de posicionamento 2D")]
    public Direction stackDirection = Direction.Down;
    public float cardRotationZ = 0f;
    public float cardSpacing = 30f;
    public float cardScale = 50f;

    void Awake()
    {
        displayListCards = FindFirstObjectByType<DisplayListCards>();
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
        newCard.layer = LayerMask.NameToLayer("DataPile");
        newCard.GetComponent<MenuCardManager>().handOwner = setup.setPlayer;

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
    public void DiscardData(GameObject cardObject)
    {
        setup.discard.AddCard(cardObject.GetComponent<CardDisplay>().cardData);
        RemoveCard(cardObject);
        UpdateVisuals();
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
    public bool HasSufficientDataToPlayCard(Dictionary<CardColor, int> costColor)
    {
        Dictionary<CardColor, int> tempCounts = new Dictionary<CardColor, int>(colorCounts);

        int totalAvailable = tempCounts.Values.Sum();
        int totalCost = costColor.Values.Sum();

        if (totalAvailable < totalCost)
            return false;

        foreach (var pair in costColor)
        {
            if (pair.Key == CardColor.Colorless)
                continue;

            int available = tempCounts.GetValueOrDefault(pair.Key, 0);

            if (available < pair.Value)
                return false;

            tempCounts[pair.Key] -= pair.Value;
        }

        int colorlessCost = costColor.GetValueOrDefault(CardColor.Colorless, 0);
        int remainingResources = tempCounts.Values.Sum();

        if (remainingResources < colorlessCost)
            return false;

        return true;
    }



    public void ListDataCardsButton()
    {
        if (setup.listDataObj.Count == 0)
        {
            Debug.LogWarning("Nenhuma carta para exibir.");
            return;
        }

        string listDescription = $"Cartas dados do {setup.evoPile.GetActivePartner().cardName}";
        Debug.Log("Botão de lista acionado");

        displayListCards.side = setup.setPlayer;

        displayListCards.isOwner = false;
        LayerMask layerMask = 12;

        displayListCards.ShowCardList
            (setup.listDataObj.Select(p => p.GetComponent<CardDisplay>()
            .cardData).ToList(), layerMask, listDescription, false);
    }
}
