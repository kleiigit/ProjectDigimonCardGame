using UnityEngine;
using SinuousProductions;
using ProjectScript.Enums;

[CreateAssetMenu(fileName = "New Digimon Card", menuName = "Card/Digimon")]
public class DigimonCard : Card
{
    [Header("Digimon Specificd Information:")]
    public int level;
    public int power = 100;
    public int leaderMemory = 0;

    public DigimonAttribute attribute = DigimonAttribute.NoAttribute;
    public DigimonStage stage;
    public DigimonType type;
    public DigimonField fieldDigimon = DigimonField.NoField;
    public bool isProtection = false;

    public Vector2 digimonSpritePosition = new Vector2(0, 0.55f);
    public float digimonSpriteScale = 1.6f;
}

