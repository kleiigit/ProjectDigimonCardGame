using SinuousProductions;
using UnityEngine;

public class TriggerCardAction : MonoBehaviour
{
    private EffectManager effectManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        effectManager = FindFirstObjectByType<EffectManager>();
    }
    public void TriggerOnplay(Card card)
    {
        if (card.effects.Count == 0)
        {
            return;
        }
        for (int i = 0; i < card.effects.Count; i++)
        {
            if (card.effects[i].trigger == CardEffects.Trigger.OnPlay)
            {
                // Execute the effect of the card when played
                effectManager.ExecuteCardEffect(card.effects[i], card);
            }
        }
    }

    public void TriggerDestroyed()
    {

    }
    public void TriggerAttacking()
    {

    }
    public void TriggerDowned()
    {

    }
    public void TriggerDiscarted()
    {

    }
    public void TriggerDrawing()
    {

    }
    public void TriggerActiveProgram()
    {

    }

}
