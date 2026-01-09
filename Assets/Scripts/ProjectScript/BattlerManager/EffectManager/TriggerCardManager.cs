using ProjectScript.Enums;
using SinuousProductions;
using System;
using System.Linq;
using UnityEngine;

public class TriggerCardManager : MonoBehaviour
{
    public static void TriggerWhenPlayed(Card card, PlayerSetup side)
    {
        Debug.Log("[TRIGGER] Play Digimon Card: ");
        if (card.effects.Count == 0) return;

        for (int i = 0; i < card.effects.Count; i++)
        {
            if (card.effects[i].trigger == CardEffects.Trigger.OnPlay)
            {
                //Execute the effect of the card when played
                EffectManager.ExecuteCardEffect(card.effects[i], card, side);
            }
        }
    }
    public static void TriggerDigimonDestroyed()
    {
        Debug.Log("[TRIGGER] Destroyed Digimon: ");
    }
    public static void TriggerSecurityDestroyed()
    {
        Debug.Log("[TRIGGER] Destroyed Security: ");
    }
    public static void TriggerSecurityEffect(Card card, PlayerSetup side)
    {
        if (card.effects.Count == 0) return;
        Debug.Log("[TRIGGER] Security activate: " + card.cardName);
        for (int i = 0; i < card.effects.Count; i++)
        {
            if (card.effects[i].trigger == CardEffects.Trigger.Security)
            {
                // Execute the effect of the card when played
                EffectManager.ExecuteCardEffect(card.effects[i], card, side);
            }
        }
    }
    public static void TriggerAttacking()
    {
        Debug.Log("[TRIGGER] Attacking card: ");
    }
    public static void TriggerDowned()
    {
        Debug.Log("[TRIGGER] Down card: ");
    }
    public static void TriggerDiscarted()
    {
        Debug.Log("[TRIGGER] Discard card: ");
    }
    public static void TriggerDrawing()
    {
        Debug.Log("[TRIGGER] Draw card: ");
    }
    public static void TriggerActiveProgram(Card card, PlayerSetup side)
    {
        Debug.Log("[TRIGGER] Active Program: " + card.cardName);
        if (card.effects.Count == 0 || card.cardType != CardType.Program) return;
        foreach (var effect in card.effects.Where(p => p.trigger == CardEffects.Trigger.NoTrigger))
        {
            EffectManager.ExecuteCardEffect(effect, card, side);
        }
    }
    public static void TriggerActivateAction(Card card, PlayerSetup side)
    {
        Debug.Log("[TRIGGER] Active Action Effect: " + card.cardName);
        if (card.effects.Count == 0 || 
            (card.cardType != CardType.Digimon || card.cardType != CardType.Partner)) return;

        for (int i = 0; i < card.effects.Count; i++)
        {
            if (card.effects[i].trigger == CardEffects.Trigger.Action)
            {
                //Execute the effect of the card when played
                EffectManager.ExecuteCardEffect(card.effects[i], card, side);
            }
        }
    }
    internal static void TriggerFreeze()
    {
        Debug.Log("[TRIGGER] Freeze effect: ");
    }

    internal static void TriggerCache()
    {
        Debug.Log("[TRIGGER] Cache effect: ");
    }

    internal static void TriggerActiveSkill(Card card, PlayerSetup side)
    {
        Debug.Log("[TRIGGER] Active Skill: " + card.cardName);
        if (card.effects.Count == 0 || card.cardType != CardType.Skill) return;
        foreach (var effect in card.effects.Where(p => p.trigger == CardEffects.Trigger.NoTrigger))
        {
            EffectManager.ExecuteCardEffect(effect, card, side);
        }
    }
}
