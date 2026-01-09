using ProjectScript.Enums;
using SinuousProductions;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;

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
    public void ShowModal(RectTransform rect, string title, string message, Action onConfirm, List<GameObject> showCard)
    {
        if (rect == null)
        {
            Debug.LogError("Canvas não informado para a janela modal.");
            return;
        }

        ModalWindow window =
            Instantiate(modalWindowPrefab, rect.transform);

        window.transform.SetAsLastSibling();
        window.Setup(title, message, onConfirm, showCard);
    }

    // Modal Yes / No com retorno bool
    public ModalWindow ShowModalYesNo(RectTransform rect, string title, string message, List<GameObject> showCard)
    {
        ModalWindow window = Instantiate(modalWindowPrefab, rect.transform);
        window.transform.SetAsLastSibling();
        window.SetupYesNo(title, message, showCard);
        return window;
    }
    public ModalWindow ShowSelectionModal(RectTransform rect, string title, string message, int requiredAmount, Func<List<GameObject>> getSelectedObjects,
    Action onConfirm)
    {
        ModalWindow window = Instantiate(modalWindowPrefab, rect.transform);
        window.transform.SetAsLastSibling();

        window.SetupSelectionModal(
            title,
            message,
            requiredAmount,
            getSelectedObjects,
            onConfirm
        );

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
            ModalWindow modal = Instance.ShowModalYesNo(setup.GetComponent<RectTransform>(), card.cardName + " has security Effect!", 
                card.effects.First(p => p.trigger == CardEffects.Trigger.Security).DescriptionEffect, new() { newCard});
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
                }, new() { newCard});
            yield break;
        }
        // SKILL
        if(from == FieldPlace.PartnerPile && card.cardType == CardType.Skill)
        {
            Instance.ShowModal(setup.GetComponent<RectTransform>(),
                card.cardName + " effect activate!", card.effects.First(p => p.trigger == CardEffects.Trigger.NoTrigger).DescriptionEffect,
                () =>
                {
                    TriggerCardManager.TriggerActiveSkill(card, setup);
                    setup.discard.AddCard(card);
                    Destroy(newCard);
                }, new() { newCard });
            yield break;
        }


        setup.dataPile.AddCard(card);
        Destroy(newCard);

    }
}
