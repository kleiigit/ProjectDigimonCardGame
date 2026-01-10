using UnityEngine;
using System.Collections.Generic;
using ProjectScript.Enums;

public class DigimonDisplay : MonoBehaviour
{
    public static List<DigimonDisplay> AllDigimons { get; } = new List<DigimonDisplay>();

    public DigimonCard digimonCardStartData;
    public CardType cardType;
    public string cardName;
    public int level;
    public int power;
    public int memory;

    public DigimonAttribute attribute;
    public DigimonStage stage;
    public DigimonType type;
    public DigimonField field;

    private bool statsStet = false;


    void Update()
    {
        if (!statsStet && digimonCardStartData != null)
        {
            SetStartStats();
        }
    }

    private void SetStartStats()
    {
        cardName = digimonCardStartData.cardName.ToUpper();
        level = digimonCardStartData.Level;
        power = digimonCardStartData.Power;
        memory = digimonCardStartData.Memory;
        attribute = digimonCardStartData.Attribute;
        stage = digimonCardStartData.Stage;
        type = digimonCardStartData.Type;
        field = digimonCardStartData.Field;
        cardType = digimonCardStartData.cardType;
        statsStet = true;
    }

}
