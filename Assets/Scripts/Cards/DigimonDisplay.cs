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
        level = digimonCardStartData.level;
        power = digimonCardStartData.power;
        memory = digimonCardStartData.leaderMemory;
        attribute = digimonCardStartData.attribute;
        stage = digimonCardStartData.stage;
        type = digimonCardStartData.type;
        field = digimonCardStartData.fieldDigimon;
        cardType = digimonCardStartData.cardType;
        statsStet = true;
    }

}
