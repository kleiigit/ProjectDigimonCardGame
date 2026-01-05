using SinuousProductions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectScript.Enums;
using System;
using Processor.EffectManager;
public class EffectManager : MonoBehaviour
{
    public static void ExecuteCardEffect(CardEffects effectSelected, Card card, PlayerSetup owner)
    { 
        string[] effectRecipe = CardEffects.EffectTypePrompt(effectSelected.promptEffect);
        if (effectRecipe == null || effectRecipe.Length == 0)
        {
            Debug.LogWarning($"[ExecuteCardEffect] Empty or invalid effectSelected on: {card.cardName} " +
                $"- {effectSelected.promptEffect}");
            return;
        }
        Debug.Log("Number effects in prompt: " + effectRecipe.Length);

        List<EffectPrompt> effectPrompts = new();

        for (int i = 0; i < effectRecipe.Length; i++)
        {
            EffectPrompt effecTarget = new(card,effectRecipe[i]);
            effectPrompts.Add(effecTarget);
            Debug.Log(effecTarget);
        }







        foreach (EffectPrompt drawEffect in effectPrompts.Where(p => p.EffectType == Keyword.Draw))
        {
            Debug.Log("Draw effect");
            DrawEffect(drawEffect.Quantity, TargetSide(owner, drawEffect.OpponentSide)); 
        }
        foreach (EffectPrompt cacheEffect in effectPrompts.Where(p => p.EffectType == Keyword.Cache))
        {
            Debug.Log("Cache effect");
            CacheEffect(cacheEffect.Quantity, TargetSide(owner, cacheEffect.OpponentSide));
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
    private static void CacheEffect(int quantity, PlayerSetup target)
    {
        TriggerCardManager.TriggerCache();
        target.drawPile.DrawCardToDataPile(quantity == 0 ? 1 : quantity);
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
}
