using ProjectScript.Enums;
using SinuousProductions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static SinuousProductions.Card;
using static SinuousProductions.CardEffects;


public class CardDisplay : MonoBehaviour
{
    //Dados gerais
    public Card cardData; // objeto de dados da carta a ser exibido

    public Image cardImage; // imagem profile
    public Image colorImage; // imagens de cores da carta, cada uma representando uma cor diferente
    public Image colorImageHalf;
    public Image[] rarityImage; // raridade da carta. revelando quanto dificil é de se conseguir a carta
    public Image[] cardTypeImage; // icone do tipo da carta. Digimon / Programa / Skill e Líder
    public Image[] crestImage; // ícones dos crests, que são usados para definir o limite de cartas no deck
    public Image securityImage;
    public Image costBox;
    public Image[] colorCostImages;
    public Image powerBackground;
    public Image fieldImage; // Campo do Digimon, reflete no tipo de mecanica do digimon.
    public Image fieldBackground; // Fundo do campo do Digimon.
    public Image protectionBox;
    public Image programIcon;
    public Image[] timeImages;
    public Image cardBoxMemory;
    public Image whiteBorder;
    public Image programBorder;
    public Image AttributeImage;

    public TMP_Text cardName; // texto do nome da carta
    public TMP_Text cardDescriptionText; // texto da descrição da carta, ainda a ser trabalhado
    public TMP_Text cardSecurityText; // texto da descrição de segurança da carta
    public TMP_Text cardIDText; //texto do numeral da carta
    public TMP_Text crestText; // textos dos crests, que são usados para definir o limite de cartas no deck
    public TMP_Text[] cardCostText; // textos do custo da carta, cada um representando uma cor diferente do custo
    public TMP_Text cardLevelText; // texto do nível da carta
    public TMP_Text cardPowerText; // texto do poder do Digimon
    public TMP_Text cardAttributeText; // atributo do Digimon, irá afetar a mecanica do jogo
    public TMP_Text cardStageText; // estágio do Digimon, revela que nível de poder ele está
    public TMP_Text cardTypeText; // tipo de digimon, pode afetar alguns efeitos de card futuramente
    public TMP_Text cardLeaderMemoryText; // memória do líder, que é usada para definir quantos Digimon pode entrar em campo
    public TMP_Text activationTime; // texto de tempo de ativação da Skill.

    public Material halfFadeMaterial;
    public RectTransform crestBox;
    public GameObject timeMainAttack;
    public GameObject timeAllTime;
    public GameObject highLight;
    [SerializeField] private GameObject selectionIndicator;
    [SerializeField] private TMPro.TextMeshProUGUI selectionNumberText;

    public Image PowerImage;




    public void UpdateCardDisplay()
    {

        cardName.text = cardData.cardName.ToUpper();
        cardData.cardID = cardData.name;
        cardIDText.text = cardData.cardID = cardData.name.ToString().ToUpper();
        UpdateCardColor();
        UpdateCardImage(rarityImage, (int)cardData.cardRarity); // Atualiza a imagem de raridade
        UpdateCardImage(cardTypeImage, (int)cardData.cardType);
        UpdateCardDescription();

        cardLeaderMemoryText.gameObject.SetActive(false);
        cardBoxMemory.gameObject.SetActive(false);

        if (cardData.sprite == null) { }
        else
        {
            cardImage.sprite = cardData.sprite;
        }

        if (cardData is DigimonCard digimonCard)
        {
            UpdateDigimonCardDisplay(digimonCard);
            cardImage.rectTransform.anchoredPosition = digimonCard.digimonSpritePosition;
            cardImage.rectTransform.localScale = new Vector3(digimonCard.digimonSpriteScale, digimonCard.digimonSpriteScale, 1);

        }
        else if (cardData is ProgramandSkillCard programAndSkillCard)
        {
            UpdateProgramAndSkillCardDisplay(programAndSkillCard);
            cardImage.rectTransform.anchoredPosition = programAndSkillCard.programSpritePosition;
            cardImage.rectTransform.localScale = new Vector3(programAndSkillCard.programSpriteScale, programAndSkillCard.programSpriteScale, 1);
        }

        if ((int)cardData.cardType > 0) // cost icon
        {
            for (int j = 0; j < cardData.cost.Count; j++)
            {
                cardCostText[j].gameObject.SetActive(true);
                cardCostText[j].text = cardData.cost[j].ToString();

            }
            UpdateCostColor();
        }
        //crests
        UpdateCardCrests();
        if ((int)cardData.cardType == 2) // Líder
        {
            crestBox.anchoredPosition = new Vector2(-0.964f, -0.264f);
            crestText.gameObject.SetActive(false); // Desativa o texto dos crests para Líder
        }
        if ((int)cardData.cardType > 1)
        {
            whiteBorder.gameObject.SetActive(true);
        }

    }

    private void UpdateProgramAndSkillCardDisplay(ProgramandSkillCard programAndSkillCard)
    {
        cardPowerText.gameObject.SetActive(false);
        cardAttributeText.gameObject.SetActive(false);
        cardStageText.gameObject.SetActive(false);
        cardTypeText.gameObject.SetActive(false);
        PowerImage.gameObject.SetActive(false);
        cardLeaderMemoryText.gameObject.SetActive(false);

        cardLevelText.gameObject.SetActive(false);


        if (cardData.cardType == CardType.Program)
        {
            programIcon.gameObject.SetActive(true);
            programBorder.gameObject.SetActive(true);
        }
        else
        {
            activationTime.gameObject.SetActive(true);
            switch (programAndSkillCard.skillTimeActivation) // Verifica o tempo de ativação da Skill
            {
                case SkillActivation.MainPhase:
                    timeImages[0].gameObject.SetActive(true);
                    activationTime.text += "Main phase".ToUpper();
                    break;
                case SkillActivation.AttackPhase:
                    timeImages[1].gameObject.SetActive(true);
                    activationTime.text += "Attack phase".ToUpper();
                    break;
                case SkillActivation.AntiProgram:
                    timeImages[2].gameObject.SetActive(true);
                    activationTime.text += "Anti-Program".ToUpper();

                    break;
                case SkillActivation.MainPhaseAndBattlePhase:
                    timeMainAttack.SetActive(true);
                    activationTime.text += "Main phase and Attack phase".ToUpper();
                    break;
                case SkillActivation.AllTime:
                    timeAllTime.SetActive(true);
                    activationTime.text += "Main phase, Attack phase and Anti-Program".ToUpper();
                    break;
            }
        }


    }

    //Digimon e Partner
    private void UpdateDigimonCardDisplay(DigimonCard digimonCard)
    {
        cardLevelText.text = digimonCard.level.ToString();
        cardAttributeText.text = digimonCard.attribute.ToString().ToUpper();
        cardStageText.text = digimonCard.stage.ToString().ToUpper();
        cardTypeText.text = digimonCard.type.ToString().ToUpper();

        UpdateCardBackground();
        programIcon.gameObject.SetActive(false);

        if (cardData.cardType == CardType.Digimon) // Digimon Configuration
        {
            costBox.gameObject.SetActive(false);
            cardPowerText.text = digimonCard.power.ToString();
            cardPowerText.gameObject.SetActive(true);
            powerBackground.gameObject.SetActive(true);
            fieldImage.gameObject.SetActive(true);
            protectionBox.gameObject.SetActive(false);
            Sprite[] subs = Resources.LoadAll<Sprite>("Sprites/SpritesCard/IconResources");
            switch (digimonCard.attribute)
            {
                
                case DigimonAttribute.Data:
                    AttributeImage.sprite = Array.Find(subs, s => s.name == "IconResources_171");
                    break;
                case DigimonAttribute.Vaccine:
                    AttributeImage.sprite = Array.Find(subs, s => s.name == "IconResources_170");
                    break;
                case DigimonAttribute.Virus:
                    AttributeImage.sprite = Array.Find(subs, s => s.name == "IconResources_165");
                    break;
                case DigimonAttribute.Variable:
                    AttributeImage.sprite = Array.Find(subs, s => s.name == "IconResources_166");
                    break;
                case DigimonAttribute.Unknown:
                    AttributeImage.sprite = Array.Find(subs, s => s.name == "IconResources_173");
                    break;
                default:
                    AttributeImage.sprite = Array.Find(subs, s => s.name == "IconResources_172");
                    break;
            }
                

            if (digimonCard.isProtection)
            {
                
                protectionBox.gameObject.SetActive(true);
                if (gameObject.layer != LayerMask.NameToLayer("Digimon"))
                {
                   // protectionBox.rectTransform.anchoredPosition = new Vector2(-0.811f, 0.372f);
                    //protectionBox.rectTransform.localScale = new Vector3(1.8f, 1.8f, 1);
                }
            }
            if ((int)cardData.cardType == 2) //Partner configuration
            {
                cardLeaderMemoryText.text = digimonCard.leaderMemory.ToString();
                cardLeaderMemoryText.gameObject.SetActive(true);
                cardBoxMemory.gameObject.SetActive(true);
                


            }
        }
        else
        {
            cardBoxMemory.gameObject.SetActive(true);
            cardLeaderMemoryText.text = digimonCard.leaderMemory.ToString();
            cardLeaderMemoryText.gameObject.SetActive(true);    
        }
    }
    #region(Metodos de controle de imagens)
    private void UpdateCardImage(Image[] image, int amount)
    {

        for (int i = 0; i < image.Length; i++)
        {
            image[i].gameObject.SetActive(false);

            if (i == amount)
            {
                image[i].gameObject.SetActive(true);
            }
        }
    }
    private void UpdateCardBackground()
    {
        Sprite[] backgroundSprite = new Sprite[9];
        Sprite[] fieldSprite = new Sprite[9];
        DigimonCard digimonField = cardData as DigimonCard;
        string sourcePath = "Sprites/FieldBackground/";
        string sourceFieldPath = "Sprites/Field/";
        backgroundSprite[0] = Resources.Load<Sprite>(sourcePath + "DR");
        backgroundSprite[1] = Resources.Load<Sprite>(sourcePath + "DS");
        backgroundSprite[2] = Resources.Load<Sprite>(sourcePath + "JT");
        backgroundSprite[3] = Resources.Load<Sprite>(sourcePath + "ME");
        backgroundSprite[4] = Resources.Load<Sprite>(sourcePath + "NSO");
        backgroundSprite[5] = Resources.Load<Sprite>(sourcePath + "NSP");
        backgroundSprite[6] = Resources.Load<Sprite>(sourcePath + "VB");
        backgroundSprite[7] = Resources.Load<Sprite>(sourcePath + "WG");
        backgroundSprite[8] = Resources.Load<Sprite>(sourcePath + "UK");
        fieldSprite[0] = Resources.Load<Sprite>(sourceFieldPath + "DR_Emblem");
        fieldSprite[1] = Resources.Load<Sprite>(sourceFieldPath + "DS_Emblem");
        fieldSprite[2] = Resources.Load<Sprite>(sourceFieldPath + "JT_Emblem");
        fieldSprite[3] = Resources.Load<Sprite>(sourceFieldPath + "ME_Emblem");
        fieldSprite[4] = Resources.Load<Sprite>(sourceFieldPath + "NSo_Emblem");
        fieldSprite[5] = Resources.Load<Sprite>(sourceFieldPath + "NSp_Emblem");
        fieldSprite[6] = Resources.Load<Sprite>(sourceFieldPath + "VB_Emblem");
        fieldSprite[7] = Resources.Load<Sprite>(sourceFieldPath + "WG_Emblem");
        fieldSprite[8] = Resources.Load<Sprite>(sourceFieldPath + "UK_Emblem");

        switch (digimonField.fieldDigimon)
        {
            case DigimonField.DragonsRoar:
                fieldBackground.sprite = backgroundSprite[0];
                fieldImage.sprite = fieldSprite[0];
                break;
            case DigimonField.DeepSavers:
                fieldBackground.sprite = backgroundSprite[1];
                fieldImage.sprite = fieldSprite[1];
                break;
            case DigimonField.JungleTroopers:
                fieldBackground.sprite = backgroundSprite[2];
                fieldImage.sprite = fieldSprite[2];
                break;
            case DigimonField.MetalEmpire:
                fieldBackground.sprite = backgroundSprite[3];
                fieldImage.sprite = fieldSprite[3];
                break;
            case DigimonField.NightmareSoldiers:
                fieldBackground.sprite = backgroundSprite[4];
                fieldImage.sprite = fieldSprite[4];
                break;
            case DigimonField.NatureSpirits:
                fieldBackground.sprite = backgroundSprite[5];
                fieldImage.sprite = fieldSprite[5];
                break;
            case DigimonField.VirusBusters:
                fieldBackground.sprite = backgroundSprite[6];
                fieldImage.sprite = fieldSprite[6];
                break;
            case DigimonField.WindGuardians:
                fieldBackground.sprite = backgroundSprite[7];
                fieldImage.sprite = fieldSprite[7];
                break;
            default:
                fieldBackground.sprite = backgroundSprite[8];
                fieldImage.sprite = fieldSprite[8];
                break;
        }
    }
    private void UpdateCardColor()
    {
        Sprite[] colorSprite = new Sprite[7];
        List<CardColor> colorSetup = cardData.cardColor;
        string sourcePath = "Sprites/ColorTemplates/";
        Sprite tempSprite;
        colorSprite[0] = Resources.Load<Sprite>(sourcePath + "RedCardBack");
        colorSprite[1] = Resources.Load<Sprite>(sourcePath + "GreenCardBack");
        colorSprite[2] = Resources.Load<Sprite>(sourcePath + "BlueCardBack");
        colorSprite[3] = Resources.Load<Sprite>(sourcePath + "YellowCardBack");
        colorSprite[4] = Resources.Load<Sprite>(sourcePath + "PurpleCardBack");
        colorSprite[5] = Resources.Load<Sprite>(sourcePath + "BlackCardBack");
        colorSprite[6] = Resources.Load<Sprite>(sourcePath + "ColoressCardBack"); // Colorless Card Back

        for (int i = 0; i < colorSetup.Count; i++)
        {
            switch (colorSetup[i])
            {
                case CardColor.Red:
                    tempSprite = colorSprite[0];
                    break;
                case CardColor.Green:
                    tempSprite = colorSprite[1];
                    break;
                case CardColor.Blue:
                    tempSprite = colorSprite[2];
                    break;
                case CardColor.Yellow:
                    tempSprite = colorSprite[3];
                    break;
                case CardColor.Purple:
                    tempSprite = colorSprite[4];
                    break;
                case CardColor.Black:
                    tempSprite = colorSprite[5];
                    break;
                default:
                    tempSprite = colorSprite[6];
                    break;
            }
            if(i > 0 )
            {
                colorImageHalf.sprite = tempSprite;
                colorImageHalf.gameObject.SetActive(true);
                colorImageHalf.material = halfFadeMaterial; // Aplica o material de fade
            }
            else
            {
                colorImage.sprite = tempSprite;
            }

        }
    }
    private void UpdateCostColor()
    {
        for (int i = 0; i < colorCostImages.Length; i++)
        {
            colorCostImages[i].gameObject.SetActive(false);
        }
        float positionYcost = 0.32f;
        // Ativa imagens para cada cor presente na cardData
        foreach (CardColor color in cardData.costColor)
        {
            int index = (int)color; // Assumindo que enum está na mesma ordem que ColorImages
            positionYcost -= 0.32f;

            if (index >= 0 && index < colorCostImages.Length)
            {
                colorCostImages[index].gameObject.SetActive(true);
                colorCostImages[index].rectTransform.anchoredPosition = new Vector2(colorCostImages[index].rectTransform.anchoredPosition.x, positionYcost);
            }

        }
    }
    private void UpdateCardCrests()
    {
        for (int i = 0; i < crestImage.Length; i++)
        {
            crestImage[i].gameObject.SetActive(false);
        }


        foreach (DigimonCrest crest in cardData.limitDeck)
        {
            int index = (int)crest; // Assumindo que enum está na mesma ordem que ColorImages
            if (index >= 0 && index < crestImage.Length)
            {
                crestImage[index].gameObject.SetActive(true);
                crestText.gameObject.SetActive(true);
                switch (index)
                {
                    case 0: // Crest of Courage
                        crestText.text += "a Coragenm";
                        break;
                    case 1: // Crest of Friendship
                        crestText.text += "a Amizade";
                        break;
                    case 2: // Crest of Love
                        crestText.text += "o Amor";
                        break;
                    case 4: // Crest of Reliability
                        crestText.text += "a Confiança";
                        break;
                    case 5: // Crest of Sincerity
                        crestText.text += "a Sinceridade";
                        break;
                    case 3: // Crest of Knowledge
                        crestText.text += "a Sabedoria";
                        break;
                    case 6: // Crest of Hope
                        crestText.text += "a Esperança";
                        break;
                    case 7: // Crest of Light
                        crestText.text += "a Luz";
                        break;
                    case 8: // Crest of Kindness
                        crestText.text += "a Bondade";
                        break;
                    case 9: // Crest of Miracles
                        crestText.text += "o Milagre";
                        break;
                    default:
                        crestText.text += "Unknown Crest: ";
                        break;
                }


            }
        }
    }
    #endregion

    #region(Effects Descriptions)
    void UpdateCardDescription()
    {
        cardDescriptionText.text = string.Empty;
        cardSecurityText.text = null;
        cardSecurityText.gameObject.SetActive(false); // Esconde o texto de segurança inicialmente
        securityImage.gameObject.SetActive(false); // Esconde a imagem de segurança inicialmente
        var effectsIconSprite = new Dictionary<string, string>
            {
                { "<DR>", "<sprite name=DR>" },
                { "<Nso>", "<sprite name=NSO>" },
                { "<NSp>", "<sprite name=NSP>" },
                { "<VB>", "<sprite name=VB>" },
                { "<WG>", "<sprite name=WG>" },
                { "<DS>", "<sprite name=DS>" },
                { "<ME>", "<sprite name=ME>" },
                { "<JR>", "<sprite name=JR>" },
                { "<UK>", "<sprite name=UK>" },
                { "<red>", "<sprite name=red>" },
                { "<blue>", "<sprite name=blue>" },
                { "<yellow>", "<sprite name=yellow>" },
                { "<green>", "<sprite name=green>" },
                { "<black>", "<sprite name=black>" },
                { "<purple>", "<sprite name=purple>" },
                { "<colorless>", "<sprite name=colorless>" },
                { "<down>", "<sprite=15>" },
                { "<cache>", "<sprite=11>"},

            };
        for (int i = 0; i < cardData.effects.Count; i++)
        {
            CardEffects effectCard = cardData.effects[i]; // Obtém a carta de efeito, se houver
            string effectDescription = cardData.effects[i].DescriptionEffect;
            //Trigger Effects
            if (effectCard.trigger == Trigger.Security)
            {
                securityImage.gameObject.SetActive(true); // Mostra a imagem de segurança
                cardSecurityText.gameObject.SetActive(true);
                cardSecurityText.text = IconTextChange(effectDescription, effectsIconSprite);
                continue;
            }
            if (i > 0)
            {
                cardDescriptionText.text += "\n"; // Adiciona uma quebra de linha entre os efeitos
            }
            if (effectCard.trigger == Trigger.OnPlay) // Ativo
            {
                cardDescriptionText.text += "<sprite=5> ";
            }
            if (effectCard.trigger == Trigger.Action) // Ação
            {
                cardDescriptionText.text += "<sprite=6> ";
            }
            if (effectCard.trigger == Trigger.Constant) // Constante
            {
                cardDescriptionText.text += "<sprite=7>: ";
            }
            if (effectCard.trigger == Trigger.Auto) // Automático
            {
                cardDescriptionText.text += "<sprite=13>: ";
            }
            if (effectCard.trigger == Trigger.Protection) // Proteção
            {
                cardDescriptionText.text += "<sprite=3>: ";
            }
            if (effectCard.trigger == Trigger.Colorfull) // multicolor
            {
                cardDescriptionText.text += "<sprite=16> ";
            }

            cardDescriptionText.text += IconTextChange(effectDescription, effectsIconSprite);
        }
    }
    #endregion
    public static string IconTextChange(string text, Dictionary<string, string> substituicoes)
    {
        if (string.IsNullOrEmpty(text) || substituicoes == null)
            return text;

        string returnText = text;

        foreach (var par in substituicoes)
        {
            if (!string.IsNullOrEmpty(par.Key))
            {
                // Cria expressão regular com RegexOptions.IgnoreCase
                string pattern = Regex.Escape(par.Key);
                returnText = Regex.Replace(returnText, pattern, par.Value, RegexOptions.IgnoreCase);
            }
        }

        return returnText;
    }
    // Cost Color
    public void UpdateCostColorAvailability(bool canBePlayed)
    {
        Color colorToUse = canBePlayed ? Color.white : Color.red;

        foreach (TMP_Text costText in cardCostText)
        {
            if (costText != null)
            {
                costText.color = colorToUse;
            }
        }
    }


}


