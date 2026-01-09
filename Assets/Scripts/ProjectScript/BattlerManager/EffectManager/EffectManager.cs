using Processor.EffectManager;
using ProjectScript.Enums;
using ProjectScript.Selection;
using SinuousProductions;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Audio.GeneratorInstance;
using static UnityEngine.GraphicsBuffer;
public class EffectManager : MonoBehaviour
{
    static DisplayListCards displayListCards;
    private void Start()
    {
        displayListCards = FindFirstObjectByType<DisplayListCards>();
    }
    public static void ExecuteCardEffect(CardEffects effectSelected, Card card, PlayerSetup owner)
    {
        List<EffectPrompt> effectPrompts = PromptSetup(effectSelected, card);
        bool criteriaResult;
        foreach (EffectPrompt effectPrompt in effectPrompts)
        {
            switch (effectPrompt.EffectType)
            {
                case Keyword.Draw:
                    Debug.Log("Draw effect");
                    DrawEffect(effectPrompt.Quantity, TargetSide(owner, effectPrompt.OpponentSide));
                    break;

                case Keyword.Cache:
                    Debug.Log("Cache effect");
                    CacheEffect(effectPrompt.Quantity, TargetSide(owner, effectPrompt.OpponentSide));
                    break;

                case Keyword.Down:
                    Debug.Log("Down effect");
                    criteriaResult = DownEffect(effectPrompt.Quantity, TargetSide(owner, effectPrompt.OpponentSide), card);
                    break;
                case Keyword.Condition:
                    Debug.Log("Criteria effect");
                    SelectCriteria(effectPrompt, TargetSide(owner, effectPrompt.OpponentSide), success =>
                    {
                        criteriaResult = success;
                        if (success)
                            Debug.Log("Efeito executado com sucesso");
                        else
                            Debug.Log("Seleção concluída sem execução");
                    });
                    break;
            }
        }
    }

    private static PlayerSetup TargetSide(PlayerSetup owner, bool isOpponent)
    {
        PlayerSetup opponentSide = owner.setPlayer == PlayerSide.PlayerBlue
                    ? GameSetupStart.playerRed : GameSetupStart.playerBlue;
        return isOpponent == true ? opponentSide : owner;
    }
    private static void DrawEffect(int quantity, PlayerSetup target)
    {
        target.drawPile.DrawCard(quantity, FieldPlace.Hand);
    }
    private static bool DownEffect(int quantity, PlayerSetup target, Card card)
    {
        if (quantity <= 0)
        {
            card.GetComponent<FieldCard>().SetDownPosition();
            return true;
        }
        else return false;
    }
    private static void CacheEffect(int quantity, PlayerSetup target)
    {
        TriggerCardManager.TriggerCache();
        target.drawPile.DrawCardToDataPile(quantity == 0 ? 1 : quantity);
    }
    private static void DiscardEffect(GameObject card, PlayerSetup target)
    {
        target.hand.RemoveCard(card);
    }
    private static void SelectCriteria(EffectPrompt criteriaEffect, PlayerSetup target, Action<bool> onCompleted)
    {
        SelectionManager.Instance.StartSelection(new SelectionRequest(criteriaEffect.Quantity,
                new SelectionCriteria
                {
                    placeRequirements = criteriaEffect.PlaceTarget,
                },
                selected =>
                {
                    bool executed = false;

                    foreach (var item in selected)
                    {
                        GameObject go = ((MonoBehaviour)item).gameObject;
                        Debug.Log("Selecionado: " + go.name);

                        string cardIDselected = go.GetComponent<CardDisplay>().cardData.cardID;

                        GameObject cardSelected =
                            SelectedCardPlace(criteriaEffect.PlaceTarget, target, cardIDselected);

                        if (criteriaEffect.Effect == Keyword.Discard)
                        {
                            DiscardEffect(cardSelected, target);
                            executed = true;
                        }
                    }

                    displayListCards.Hide();
                    onCompleted?.Invoke(executed);
                }
            ),
            target.GetComponent<RectTransform>()
        );
    }
    private static GameObject SelectedCardPlace(FieldPlace place, PlayerSetup target, string cardID)
    {
        switch (place)
        {
            case FieldPlace.Hand:
                return target.listHandObj.First(p => p.GetComponent<CardDisplay>().cardData.cardID == cardID);
            case FieldPlace.DataPile:
                return target.listDataObj.First(p => p.GetComponent<CardDisplay>().cardData.cardID == cardID);
            default:
                return null;
        }
    }
    private static void TargetFieldCard(Keyword effect, FieldCard target)
    {
        switch (effect)
        {
            case Keyword.Destroy:
                target.DestroyFieldCard();
                break;

            case Keyword.Discard:
                target.DiscardFieldCard();
                break;

            case Keyword.Down:
                target.SetDownPosition();
                break;

            case Keyword.Freeze:
                target.SetFreeze();
                break;

            default:
                Debug.LogWarning($"[TargetFieldCard] Efeito desconhecido: {effect}");
                break;
        }
    }

    //
    public static bool CriteriaEffect(CardEffects effectSelected, Card card, PlayerSetup owner)
    {
        // Retorna true se houver os critérios no jogo para ativar efeitos
        List<EffectPrompt> effectPrompts = PromptSetup(effectSelected, card);

        // Avalia efeitos Condition
        foreach (var criteriaEffect in effectPrompts.Where(p => p.EffectType == Keyword.Condition))
        {
            var target = TargetSide(owner, criteriaEffect.OpponentSide);
            if (target != null && CriteriaEffectBoolean(criteriaEffect, card, target))
                return true; // Pelo menos um critério satisfeito
        }

        // Avalia efeitos Down
        foreach (var downEffect in effectPrompts.Where(p => p.EffectType == Keyword.Down))
        {
            if (downEffect.Quantity <= 0 && !card.GetComponent<FieldCard>().downPosition)
            {
                return true;
            }
        }

        return false; // Nenhum critério satisfeito
    }

    private static bool CriteriaEffectBoolean(EffectPrompt criteriaEffect, Card card, PlayerSetup target)
    {
        if (criteriaEffect.PlaceTarget == FieldPlace.Hand)
            return target.listHandObj.Count >= criteriaEffect.Quantity;

        // Aqui você pode adicionar outros FieldPlace se necessário
        return false;
    }


    private static List<EffectPrompt> PromptSetup(CardEffects effectSelected, Card card)
    {
        List<EffectPrompt> effectPrompts = new();
        string[] effectRecipe = CardEffects.EffectTypePrompt(effectSelected.promptEffect);
        if (effectRecipe == null || effectRecipe.Length == 0)
        {
            Debug.LogWarning($"[ExecuteCardEffect] Empty or invalid effectSelected on: {card.cardName} " +
                $"- {effectSelected.promptEffect}");
            return effectPrompts;
        }
        for (int i = 0; i < effectRecipe.Length; i++)
        {
            EffectPrompt effecTarget = new(card, effectRecipe[i]);
            effectPrompts.Add(effecTarget);
            Debug.Log(effecTarget);
        }
        return effectPrompts;
    }
}
