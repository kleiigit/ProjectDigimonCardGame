using ProjectScript.Enums;
using SinuousProductions;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;

public class UIWindowManager : MonoBehaviour
{
    public static UIWindowManager Instance;

    [SerializeField] private ModalWindow modalWindowPrefab;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Modal simples (OK / Confirm)
    public void ShowModal(
        RectTransform rect,
        string title,
        string message,
        System.Action onConfirm)
    {
        if (rect == null)
        {
            Debug.LogError("Canvas não informado para a janela modal.");
            return;
        }

        ModalWindow window =
            Instantiate(modalWindowPrefab, rect.transform);

        window.transform.SetAsLastSibling();
        window.Setup(title, message, onConfirm);
    }

    // Modal Yes / No com retorno bool
    public ModalWindow ShowModalYesNo(
    RectTransform rect,
    string title,
    string message)
    {
        ModalWindow window = Instantiate(modalWindowPrefab, rect.transform);
        window.transform.SetAsLastSibling();
        window.SetupYesNo(title, message);
        return window;
    }

    // temporário, preciso colocar em um manager. talvez effect manager?
    public void MoveToCheckZone(Card card, PlayerSetup setup, FieldPlace from)
    {
        StartCoroutine(MoveToCheckZoneCoroutine(card, setup, from));
    }
    private IEnumerator MoveToCheckZoneCoroutine(Card card, PlayerSetup setup, FieldPlace from)
    {
        GameObject newCard = Instantiate(GameManager.cardPrefab, setup.checkzone.transform.position,
            Quaternion.identity, setup.checkzone.transform);
        newCard.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        newCard.layer = 9;
        CardDisplay newCardDisplay = newCard.GetComponent<CardDisplay>();
        newCardDisplay.cardData = card;
        newCardDisplay.UpdateCardDisplay();
        GridCell cell = setup.checkzone.GetComponent<GridCell>();
        cell.ObjectInCell = newCard;
        cell.cellFull = true;
        HoverCardManager.Instance.ShowCard(newCardDisplay.cardData);
        // SECURITY PILE
        if (from == FieldPlace.SecurityPile &&
            card.effects.Any(p => p.trigger == CardEffects.Trigger.Security))
        {
            ModalWindow modal =
                UIWindowManager.Instance.ShowModalYesNo(
                    setup.GetComponent<RectTransform>(),
                    card.cardName + " has security Effect!",
                    "efeito da carta a ser implementado");
            // AGUARDA O JOGADOR
            yield return new WaitUntil(() => modal.HasResult);
            if (modal.Result) TriggerCardManager.TriggerSecurityEffect(card, setup);

            setup.dataPile.AddCard(card);
            Destroy(newCard);
            if (modal != null) Destroy(modal.gameObject);
            yield break;
        }

        // HAND
        if (from == FieldPlace.Hand && card.cardType == CardType.Program)
        {
            Instance.ShowModal(setup.GetComponent<RectTransform>(),
                card.cardName + " effect activate!", card.effects.First(p => p.trigger == CardEffects.Trigger.NoTrigger).DescriptionEffect,
                () =>
                {
                    TriggerCardManager.TriggerActiveProgram(card, setup);
                    setup.discard.AddCard(card);
                    Destroy(newCard);
                });
            yield break;
        }
        setup.dataPile.AddCard(card);
        Destroy(newCard);

    }
}
