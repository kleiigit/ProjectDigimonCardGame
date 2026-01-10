using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ProjectScript.Enums;

namespace SinuousProductions
{
    public class DeckStatisticsDisplay : MonoBehaviour
    {
        [Header("Referências de Texto - Tipos")]
        [SerializeField] private TMP_Text digimonCountText;
        [SerializeField] private TMP_Text programCountText;
        [SerializeField] private TMP_Text partnerCountText;
        [SerializeField] private TMP_Text skillCountText;

        [Header("Referências de Texto - Cores")]
        [SerializeField] private TMP_Text colorCountText;

        [Header("Referências de Texto - Níveis")]
        [SerializeField] private TMP_Text level1Text;
        [SerializeField] private TMP_Text level2Text;
        [SerializeField] private TMP_Text level3Text;
        [SerializeField] private TMP_Text level4Text;
        [SerializeField] private TMP_Text level5Text;

        [Header("Barras Visuais - Tipos")]
        [SerializeField] private Image digimonBar;
        [SerializeField] private Image programBar;
        [SerializeField] private Image partnerBar;
        [SerializeField] private Image skillBar;

        [SerializeField] private Image level1Bar;
        [SerializeField] private Image level2Bar;
        [SerializeField] private Image level3Bar;
        [SerializeField] private Image level4Bar;
        [SerializeField] private Image level5Bar;

        

        public void ShowStatistics(List<DeckCardEntry> mainDeckEntries, List<DeckCardEntry> partnerDeckEntries, List<Card> cardDatabase)
        {
            List<Card> allCards = new();

            void AddCards(List<DeckCardEntry> entries)
            {
                foreach (var entry in entries)
                {
                    Card baseCard = cardDatabase.FirstOrDefault(c => c.cardID == entry.cardID);
                    if (baseCard == null) continue;

                    for (int i = 0; i < entry.quantity; i++)
                    {
                        allCards.Add(baseCard);
                    }
                }
            }

            AddCards(mainDeckEntries);
            AddCards(partnerDeckEntries);

            // Tipos
            int digimonCount = allCards.Count(c => c.cardType == CardType.Digimon);
            int programCount = allCards.Count(c => c.cardType == CardType.Program);
            int partnerCount = allCards.Count(c => c.cardType == CardType.Partner);
            int skillCount = allCards.Count(c => c.cardType == CardType.Skill);

            // Níveis (apenas Digimon)
            Dictionary<int, int> levelCounts = new();
            foreach (var card in allCards)
            {
                if (card.cardType != CardType.Digimon) continue;
                if (card is DigimonCard digimonCard)
                {
                    int level = digimonCard.Level;
                    if (!levelCounts.ContainsKey(level))
                        levelCounts[level] = 0;
                    levelCounts[level]++;
                }
            }

            // Cores
            Dictionary<CardColor, int> colorCounts = new();
            foreach (var card in allCards)
            {
                foreach (var color in card.cardColor)
                {
                    if (!colorCounts.ContainsKey(color))
                        colorCounts[color] = 0;
                    colorCounts[color]++;
                }
            }

            // Atualiza UI - Tipos
            if (digimonCountText != null)
                digimonCountText.text = digimonCount.ToString();
            if (programCountText != null)
                programCountText.text = programCount.ToString();
            if (partnerCountText != null)
                partnerCountText.text = partnerCount.ToString();
            if (skillCountText != null)
                skillCountText.text = skillCount.ToString();

            // Atualiza barras (fillAmount de 0 a 1)
            int totalMainCards = digimonCount + programCount;
            int totalPartnerCards = partnerCount + skillCount;

            if (digimonBar != null)
                digimonBar.fillAmount = totalMainCards > 0 ? (float)digimonCount / totalMainCards : 0f;
            if (programBar != null)
                programBar.fillAmount = totalMainCards > 0 ? (float)programCount / totalMainCards : 0f;
            if (partnerBar != null)
                partnerBar.fillAmount = totalPartnerCards > 0 ? (float)partnerCount / totalPartnerCards : 0f;
            if (skillBar != null)
                skillBar.fillAmount = totalPartnerCards > 0 ? (float)skillCount / totalPartnerCards : 0f;

            int totalDigimon = levelCounts.Values.Sum();

            if (level1Bar != null)
                level1Bar.fillAmount = totalDigimon > 0 ? (float)(levelCounts.ContainsKey(1) ? levelCounts[1] : 0) / totalDigimon : 0f;
            if (level2Bar != null)
                level2Bar.fillAmount = totalDigimon > 0 ? (float)(levelCounts.ContainsKey(2) ? levelCounts[2] : 0) / totalDigimon : 0f;
            if (level3Bar != null)
                level3Bar.fillAmount = totalDigimon > 0 ? (float)(levelCounts.ContainsKey(3) ? levelCounts[3] : 0) / totalDigimon : 0f;
            if (level4Bar != null)
                level4Bar.fillAmount = totalDigimon > 0 ? (float)(levelCounts.ContainsKey(4) ? levelCounts[4] : 0) / totalDigimon : 0f;
            if (level5Bar != null)
                level5Bar.fillAmount = totalDigimon > 0 ? (float)(levelCounts.ContainsKey(5) ? levelCounts[5] : 0) / totalDigimon : 0f;
            

            // Atualiza UI - Cores
            if (colorCountText != null)
            {
                colorCountText.text = string.Join("\n", colorCounts
                    .OrderBy(kv => kv.Key.ToString())
                    .Select(kv => $"{kv.Key}: {kv.Value}"));
            }

            // Atualiza UI - Níveis
           
            SetLevelText(level1Text, 1, levelCounts);
            SetLevelText(level2Text, 2, levelCounts);
            SetLevelText(level3Text, 3, levelCounts);
            SetLevelText(level4Text, 4, levelCounts);
            SetLevelText(level5Text, 5, levelCounts);
        }


        private void SetLevelText(TMP_Text textField, int level, Dictionary<int, int> levelCounts)
        {
            if (textField != null)
            {
                if (levelCounts.TryGetValue(level, out int count))
                    textField.text = count.ToString();
                else
                    textField.text = "0";
            }
        }
    }
}
