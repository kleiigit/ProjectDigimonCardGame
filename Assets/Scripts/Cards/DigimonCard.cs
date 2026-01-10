using UnityEngine;
using SinuousProductions;
using ProjectScript.Enums;

[CreateAssetMenu(fileName = "New Digimon Card", menuName = "Card/Digimon")]
public class DigimonCard : Card
{
    [Header("Digimon Specificd Information:")]
    public int Level;
    public int Power = 100;
    public int Memory = 0;

    public DigimonAttribute Attribute = DigimonAttribute.NoAttribute;
    public DigimonStage Stage;
    public DigimonType Type;
    public DigimonField Field = DigimonField.NoField;
    public bool isProtection = false;

    public Vector2 digimonSpritePosition = new Vector2(0, 0.55f);
    [Range(0f, 5f)]
    public float digimonSpriteScale = 1.6f;

    public bool CanDigimonPlayed(PlayerSetup setup)
    {
        if(setup.evoPile.GetActivePartner() == null) 
            return false;

        DigimonCard activePartner = setup.evoPile.GetActivePartner();
        if (activePartner.Level >= Level)
        {
            if (setup.currentMemory + Level <= activePartner.Memory) return true;
        }
        return false;
    }
}

