using UnityEngine;
using SinuousProductions;

[CreateAssetMenu(fileName = "New Program and Skill Card", menuName = "Card/Program and Skill")]
public class ProgramandSkillCard : Card
{
    [Header("Program and Skill Specific Information:")]
    public SkillActivation skillTimeActivation;


    public Vector2 programSpritePosition = new Vector2(0f, 0.69f);
    public float programSpriteScale = 2.222f;
}
