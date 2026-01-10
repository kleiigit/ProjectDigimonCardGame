using ProjectScript.Enums;
using ProjectScript.Interfaces;
using SinuousProductions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SecurityPileManager : MonoBehaviour, IPile
{
    public Transform securityPileTransform;
    [SerializeField] private Transform revealGrid;
    private GameObject decisionPanel;
    public float cardScale = 50f;
    [SerializeField]
    private float cardSpacing = 10f;

    private PlayerSetup setup;

    void Awake()
    {
        setup = GetComponent<PlayerSetup>();
        decisionPanel = GameObject.Find("DecisionPanel");

        if (decisionPanel != null)
            decisionPanel.SetActive(false);
    }
    public void AddCard(Card cardData)
    {
        GameObject newCard = Instantiate(GameManager.cardPrefab, securityPileTransform.position, Quaternion.identity, securityPileTransform);
        newCard.transform.SetAsLastSibling();

        newCard.name = cardData.cardName.ToUpper() + " - " + cardData.cardID;

        newCard.GetComponent<CardDisplay>().cardData = cardData;
        newCard.GetComponent<CardDisplay>().UpdateCardDisplay();
        newCard.transform.localScale = new Vector3(cardScale, cardScale, 1);
        newCard.layer = LayerMask.NameToLayer("Security");
        newCard.GetComponent<MenuCardManager>().handOwner = setup.setPlayer;

        setup.listSecurityObj.Add(newCard);

        foreach (Transform child in newCard.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "MainBack")
            {
                child.gameObject.SetActive(true);
                break;
            }
        }
        UpdateVisuals();
        //Debug.Log($"Adicionando carta ao topo da pilha de segurança {pileSide}: {newCard.name}");
    }

    public void RemoveCard(GameObject gameObject)
    {
        TriggerCardManager.TriggerSecurityDestroyed();
        setup.listSecurityObj.Remove(gameObject);
        Destroy(gameObject);
        UpdateVisuals();
    }

    public void DestroySecurity()
    {
        if (setup.listSecurityObj.Count == 0)
        {
            Debug.LogWarning("Não há cartas na pilha de segurança do lado especificado.");

                Debug.Log($">> GAME OVER para {setup.setPlayer}!");
            // Chamar o método de GameOver aqui
            #if UNITY_EDITOR
            EditorApplication.isPaused = true;
            #endif
            return;
        }

        GameObject topCard = setup.listSecurityObj[^1];
        var cardData = topCard.GetComponent<CardDisplay>().cardData;
        setup.securityPile.RemoveCard(setup.listSecurityObj[^1]);
        UIWindowManager.Instance.MoveToCheckZone(topCard.GetComponent<CardDisplay>(), setup, FieldPlace.SecurityPile);
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (setup == null)
        {
            Debug.LogError("Setup nao configuado");
        }
        if (setup.listSecurityObj.Count == 0) return;

        List<GameObject> cards = setup.listSecurityObj;

        for (int i = 0; i < setup.listSecurityObj.Count; i++)
        {
            if (cards[i] != null)
            {
                cards[i].transform.SetParent(securityPileTransform); // Garante o parent correto
                cards[i].transform.localPosition = new Vector3(i * cardSpacing, 0f, 0f);
            }
        }
    }
    public static void BattleSetupSecurity(int amountPerPlayer)
    {
        GameSetupStart.playerBlue.drawPile.DrawCard(amountPerPlayer, FieldPlace.SecurityPile);
        GameSetupStart.playerRed.drawPile.DrawCard(amountPerPlayer, FieldPlace.SecurityPile);
    }
}
