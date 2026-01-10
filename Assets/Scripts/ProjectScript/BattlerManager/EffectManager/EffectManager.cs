using ProjectScript.EffectManager;
using ProjectScript.Enums;
using ProjectScript.Selection;
using SinuousProductions;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class EffectManager : MonoBehaviour
{
    #region Setup
    static DisplayListCards displayListCards;
    static GameManager manager;
    private void Start()
    {
        displayListCards = FindFirstObjectByType<DisplayListCards>();
        manager = FindFirstObjectByType<GameManager>();
    }

    // Configura os prompts de efeito com base no efeito selecionado
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
    #endregion
    public static void ExecuteCardEffect(CardEffects effectSelected, CardDisplay card, PlayerSetup owner)
    {
        List<EffectPrompt> effectPrompts = PromptSetup(effectSelected, card.cardData);

        Debug.Log($"[ExecuteCardEffect] Executing effects for card: {card.cardData.cardName}");

        // efeitos iniciais (sem critério)
        foreach (EffectPrompt prompt in effectPrompts)
        {
            if (prompt.EffectType == Keyword.Condition)
                continue;

            switch (prompt.EffectType)
            {
                case Keyword.Draw:
                    DrawEffect(prompt.Quantity, TargetSideEffect(owner, prompt.OpponentSide));
                    break;

                case Keyword.Cache:
                    CacheEffect(prompt.Quantity, TargetSideEffect(owner, prompt.OpponentSide));
                    break;

                case Keyword.Down:
                    DownEffect(prompt.Quantity, TargetSideEffect(owner, prompt.OpponentSide), card);
                    break;
            }
        }

        // 2 coleta de critérios
        List<EffectPrompt> criteria = effectPrompts
            .Where(p => p.EffectType == Keyword.Condition)
            .ToList();

        if (criteria.Count == 0)
        {
            ExecuteEffect(effectPrompts, card, owner);
            return;
        }

        int pending = criteria.Count;
        bool allSuccess = true;

        foreach (EffectPrompt prompt in criteria)
        {
            SelectCardEffect(prompt, TargetSideEffect(owner, prompt.OpponentSide), success =>
            {
                allSuccess &= success;
                pending--;

                if (pending == 0 && allSuccess)
                    ExecuteEffect(effectPrompts, card, owner);
            });
        }
    }
    private static void ExecuteEffect(List<EffectPrompt> prompts,CardDisplay card, PlayerSetup owner)
    {
        foreach (EffectPrompt prompt in prompts)
        {
            if (prompt.EffectType != Keyword.Target)
                continue;
            // Card
            switch (prompt.Effect)
            {
                case Keyword.Draw:
                    DrawEffect(prompt.EffectQuantity, TargetSideEffect(owner, prompt.OpponentSide));
                    break;
            }
            // Field Effect
            if(prompt.PlaceTarget == FieldPlace.BattleZone)
            {
                SelectCardEffect(prompt, owner, success =>
                {
                    Debug.Log("Effect resolved");
                });
            }
        }
    }

    // Effects Implementation
    private static void DrawEffect(int quantity, PlayerSetup target)
    {
        target.drawPile.DrawCard(quantity == 0 ? 1 : quantity, FieldPlace.Hand);
    }
    private static bool DownEffect(int quantity, PlayerSetup target, CardDisplay card)
    {
        if (quantity <= 0)
        {
            card.gameObject.GetComponent<FieldCard>().SetDownPosition();
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

    // Criteria Selection
    private static void SelectCardEffect(EffectPrompt criteriaEffect, PlayerSetup owner, Action<bool> onCompleted)
    {
        Debug.Log(criteriaEffect.ToString());
        SelectionManager.Instance.StartSelection(new SelectionRequest(criteriaEffect.Quantity,
                new SelectionCriteria
                {
                    sideRequirements = TargetSideEffect(owner, criteriaEffect.OpponentSide).setPlayer,
                    placeRequirements = criteriaEffect.PlaceTarget,
                    typeRequirements = criteriaEffect.TypeTarget != CardType.Card ? criteriaEffect.TypeTarget : null,
                    fieldRequirements = criteriaEffect.DigimonField != DigimonField.NoField ? criteriaEffect.DigimonField : null
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
                            SelectedCardPlace(criteriaEffect.PlaceTarget, owner, cardIDselected);

                        // Condition Effect
                        if (criteriaEffect.EffectType == Keyword.Condition)
                        {
                            if (criteriaEffect.Effect == Keyword.Discard)
                            {
                                DiscardEffect(cardSelected, owner);
                                executed = true;
                            }
                        }

                        // Target Effect
                        if(criteriaEffect.EffectType == Keyword.Target)
                        {
                            if(criteriaEffect.PlaceTarget == FieldPlace.BattleZone)
                            {
                                TargetFieldCard(criteriaEffect.Effect, go.GetComponent<FieldCard>());
                            }
                        }
                    }

                    displayListCards.Hide();
                    onCompleted?.Invoke(executed);
                }
            ),
            owner.GetComponent<RectTransform>()
            , "Select cards", $"Select ({criteriaEffect.Quantity}) card(s) from " +
                $"{(criteriaEffect.OpponentSide ? "opponent" : "player")} " +
                $"{criteriaEffect.PlaceTarget}."
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
    private static PlayerSetup TargetSideEffect(PlayerSetup owner, bool isOpponent)
    {
        PlayerSetup opponentSide = owner.setPlayer == PlayerSide.PlayerBlue
                    ? GameSetupStart.playerRed : GameSetupStart.playerBlue;
        return isOpponent == true ? opponentSide : owner;
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

    // Verifica se os critérios para ativar efeitos são atendidos
    public static bool ConditionEffect(CardEffects effectSelected, CardDisplay card, PlayerSetup owner)
    {
        List<EffectPrompt> effectPrompts = PromptSetup(effectSelected, card.cardData);
        Debug.Log($"[ConditionEffect] Checking conditions for card: {card.cardData.cardName}");
        //Condition
        foreach (var criteriaCondition in effectPrompts.Where(p => p.EffectType == Keyword.Condition))
        {
            var target = TargetSideEffect(owner, criteriaCondition.OpponentSide);
            if (target == null)
               { Debug.Log("targetSideEffect erro");  return false; }

            if (!CheckCardsInGame(criteriaCondition, card.cardData, target))
                return false;
        }

        //Down
        foreach (var downCondition in effectPrompts.Where(p => p.EffectType == Keyword.Down))
        {
            if (card.GetComponent<FieldCard>().downPosition)
            {
                Debug.Log($"[ConditionEffect] {card.cardData.cardName} is already in down position.");
                return false;
            }
        }

        //Target
        foreach (var targetCondition in effectPrompts.Where(p => p.EffectType == Keyword.Target))
        {
            var target = TargetSideEffect(owner, targetCondition.OpponentSide);
            if (target == null)
            { Debug.Log("targetSideEffect erro"); return false; }

            if (!CheckCardsInGame(targetCondition, card.cardData, target))
                return false;
        }


        Debug.Log($"[ConditionEffect] {card.cardData.cardName} All criteria met for effect activation.");
        return true; // Todos os critérios foram satisfeitos
    }
    private static bool CheckCardsInGame(EffectPrompt criteriaEffect, Card card, PlayerSetup target)
    {
        if (criteriaEffect.PlaceTarget == FieldPlace.Hand)
        {
            // Quantity in hand
            if (target.listHandObj.Count < criteriaEffect.Quantity)
            {
                Debug.Log($"[CheckCardsInGame] Not enough cards in Hand. Found: {target.listHandObj.Count}, Required: {criteriaEffect.Quantity}");
                return false;
            }

            List<Card> handCards = target.listHandObj.Select(p => p.GetComponent<CardDisplay>().cardData).ToList();
            // Quantity type cards
            if (criteriaEffect.TypeTarget != CardType.Card 
                && handCards.Count(p => p.cardType == criteriaEffect.TypeTarget) < criteriaEffect.Quantity)
            {
                Debug.Log("[CheckCardsInGame] “There are no cards of type: " + criteriaEffect.TypeTarget);
                return false;
            }

            List<DigimonCard> digimonHandCards = handCards.Where(p => p.cardType == CardType.Digimon)
                .Select(p => p as DigimonCard).ToList();
            // Quantity Digimon cards
            if(criteriaEffect.DigimonField != DigimonField.NoField 
                && digimonHandCards.Count(p => p.Field == criteriaEffect.DigimonField) < criteriaEffect.Quantity)
            {
                Debug.Log("[CheckCardsInGame] “There are no digimon cards of field: " + criteriaEffect.DigimonField);
                return false;
            }

            Debug.Log($"[CheckCardsInGame] Quantity condition met in Hand.");
            return true;
        }

        if (criteriaEffect.PlaceTarget == FieldPlace.BattleZone)
        {
            List<GameObject> gameObjectList = target.setPlayer == PlayerSide.PlayerBlue ? manager.cardsInFieldBlue : manager.cardsInFieldRed;
                List<DigimonDisplay> digimonList = gameObjectList
                .Where(p => p.GetComponent<MenuCardManager>().handOwner == target.setPlayer)
                .Select(p => p.GetComponent<DigimonDisplay>()).ToList();

                // check quantity
                if (digimonList.Count < criteriaEffect.Quantity)
                {
                    Debug.Log($"[CheckCardsInGame] Not enough Digimons in BattleZone. Found: {digimonList.Count}, Required: {criteriaEffect.Quantity}");
                    return false;
                }

                // check power
                if (criteriaEffect.PowerTarget > 0)
                {
                    int matchingPower = digimonList.Count(p => criteriaEffect.IsLesser ?
                    p.power <= criteriaEffect.PowerTarget : p.power >= criteriaEffect.PowerTarget);
                    if (matchingPower >= criteriaEffect.Quantity)
                    {
                        Debug.Log($"[CheckCardsInGame] Power condition met: {matchingPower} cards found.");
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                // check digimon field
                if (criteriaEffect.DigimonField != DigimonField.NoField)
                {
                    if (digimonList.Count(p => p.field == criteriaEffect.DigimonField)
                        >= criteriaEffect.Quantity)
                    { return true; }
                    else
                    {
                        Debug.Log($"[CheckCardsInGame] DigimonField condition not met.");
                        return false;
                    }
                }
                Debug.Log($"[CheckCardsInGame] Quantity condition met in BattleZone.");
            return true;

        }

        if(criteriaEffect.PlaceTarget == FieldPlace.MainDeck)
        {
            if (criteriaEffect.Effect == Keyword.Draw) return true;
        }

        Debug.Log("erro CheckCardsInGame");
        return false;
    }
}
