using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using ProjectScript.Enums;

namespace SinuousProductions
{
    public class CardFilterManager : MonoBehaviour
    {
        [Header("UI Filtros")]
        public TMP_Dropdown fieldDropdown;
        public TMP_Dropdown attributeDropdown;
        public TMP_Dropdown typeDropdown;
        public TMP_Dropdown stageDropdown;
        public TMP_Dropdown colorDropdown;
        public TMP_Dropdown statusDropdown;
        public TMP_Dropdown secondColorDropdown;

        public TMP_InputField minLevelInput;
        public TMP_InputField maxLevelInput;
        public TMP_InputField minPowerInput;
        public TMP_InputField maxPowerInput;

        public Button applyButton;
        public Button resetButton;

        [Header("UI Toggle")]
        public GameObject filterPanel;
        public Button filterButton;

        [Header("Coleção de Cartas")]
        public Transform cardContainer;
        public DeckEditorUI deckEditorUI;

        private void Start()
        {
            PopulateDropdownWithAllOption<DigimonField>(fieldDropdown, "All", f => f.ToString(), excludeNoField: true);
            PopulateDropdownWithAllOption<DigimonAttribute>(attributeDropdown, "All", a => a.ToString(), excludeNoAttribute: true);
            PopulateDropdownWithAllOption<CardType>(typeDropdown, "All", t => t.ToString(), excludeCardTypeCard: true);
            PopulateDropdownWithAllOption<DigimonStage>(stageDropdown, "All", s => s.ToString());
            PopulateDropdownWithAllOption<CardColor>(colorDropdown, "All", c => c.ToString(), excludeNoColor: true);
            PopulateDropdownWithAllOption<CardColor>(secondColorDropdown, "All", c => c.ToString(), excludeNoColor: true); // NOVO

            applyButton.onClick.AddListener(ApplyFilters);

            if (filterButton != null && filterPanel != null)
            {
                filterButton.onClick.AddListener(() =>
                {
                    filterPanel.SetActive(!filterPanel.activeSelf);
                });
            }
            if (resetButton != null)
                resetButton.onClick.AddListener(ResetFilters);
        }
        private void Update()
        {
            if (filterPanel != null && filterPanel.activeSelf && Input.GetMouseButtonDown(0))
            {
                if (!IsPointerOverUIObject(filterPanel))
                {
                    filterPanel.SetActive(false);
                }
            }
        }
        private void ApplyFilters()
        {
            if (deckEditorUI == null || deckEditorUI.cardDatabase == null)
                return;

            bool typeAll = typeDropdown.value == 0;
            bool colorAll = colorDropdown.value == 0;
            bool secondColorAll = secondColorDropdown.value == 0;
            bool fieldAll = fieldDropdown.value == 0;
            bool attributeAll = attributeDropdown.value == 0;
            bool stageAll = stageDropdown.value == 0;

            DigimonField selectedField = fieldAll ? default : (DigimonField)(fieldDropdown.value - 1);
            DigimonAttribute selectedAttribute = attributeAll ? default : (DigimonAttribute)(attributeDropdown.value - 1);
            CardType selectedType = typeAll ? default : (CardType)(typeDropdown.value - 1);
            DigimonStage selectedStage = stageAll ? default : (DigimonStage)(stageDropdown.value - 1);
            CardColor selectedColor = colorAll ? default : (CardColor)(colorDropdown.value - 1);
            CardColor selectedSecondColor = secondColorAll ? default : (CardColor)(secondColorDropdown.value - 1);

            int? minLevel = ParseNullableIntWithZero(minLevelInput.text);
            int? maxLevel = ParseNullableIntWithZero(maxLevelInput.text);
            int? minPower = ParseNullableIntWithZero(minPowerInput.text);
            int? maxPower = ParseNullableIntWithZero(maxPowerInput.text);

            bool filterLevel = minLevel.HasValue || maxLevel.HasValue;
            bool filterPower = minPower.HasValue || maxPower.HasValue;

            foreach (Transform cardGO in cardContainer)
            {
                string cardId = cardGO.name;
                Card card = deckEditorUI.cardDatabase.FirstOrDefault(c => c.cardID == cardId);
                CardStatus status = CardsCollectionManager.Instance.GetCardStatus(cardId);
                bool matchStatus = false;

                switch (statusDropdown.value)
                {
                    case 0: // Obtidas
                        matchStatus = status == CardStatus.Owned;
                        break;
                    case 1: // Apenas vistas
                        matchStatus = status == CardStatus.Seen;
                        break;
                    case 2: // Obtidas + vistas
                        matchStatus = status == CardStatus.Owned || status == CardStatus.Seen;
                        break;
                }

                if (card == null)
                {
                    cardGO.gameObject.SetActive(false);
                    continue;
                }

                // Field (apenas para Digimon)
                bool matchField = fieldAll || (card is DigimonCard digimonField && digimonField.Field == selectedField);

                // Atributo e estágio (apenas para Digimon)
                bool matchAttribute = true;
                bool matchStage = true;

                if (!attributeAll)
                {
                    if (card is DigimonCard digimonAttr)
                        matchAttribute = digimonAttr.Attribute == selectedAttribute;
                    else
                        matchAttribute = false;
                }

                if (!stageAll)
                {
                    if (card is DigimonCard digimonStage)
                        matchStage = digimonStage.Stage == selectedStage;
                    else
                        matchStage = false;
                }

                // Tipo (aplicável a todas)
                bool matchType = typeAll || card.cardType == selectedType;

                // Cor
                bool matchColor = false;
                if (colorAll && secondColorAll)
                {
                    matchColor = true;
                }
                else if (!colorAll && secondColorAll)
                {
                    matchColor = card.cardColor != null && card.cardColor.Contains(selectedColor);
                }
                else if (colorAll && !secondColorAll)
                {
                    matchColor = card.cardColor != null && card.cardColor.Contains(selectedSecondColor);
                }
                else
                {
                    matchColor = card.cardColor != null &&
                                 card.cardColor.Contains(selectedColor) &&
                                 card.cardColor.Contains(selectedSecondColor);
                }

                // Level e Power
                bool matchLevel = true;
                bool matchPower = true;

                if (card is DigimonCard digimon)
                {
                    if (filterLevel)
                    {
                        if (minLevel.HasValue && digimon.Level < minLevel.Value)
                            matchLevel = false;
                        if (maxLevel.HasValue && digimon.Level > maxLevel.Value)
                            matchLevel = false;
                    }

                    if (filterPower)
                    {
                        if (minPower.HasValue && digimon.Power < minPower.Value)
                            matchPower = false;
                        if (maxPower.HasValue && digimon.Power > maxPower.Value)
                            matchPower = false;
                    }
                }
                else
                {
                    if (filterLevel)
                        matchLevel = false;
                }

                bool show = matchField && matchAttribute && matchType && matchStage &&
                            matchColor && matchLevel && matchPower && matchStatus;

                cardGO.gameObject.SetActive(show);
            }

            filterPanel.SetActive(false);
        }


        private void ResetFilters()
        {
            fieldDropdown.value = 0;
            attributeDropdown.value = 0;
            typeDropdown.value = 0;
            stageDropdown.value = 0;
            colorDropdown.value = 0;
            secondColorDropdown.value = 0;
            statusDropdown.value = 0;

            minLevelInput.text = "";
            maxLevelInput.text = "";
            minPowerInput.text = "";
            maxPowerInput.text = "";

            foreach (Transform cardGO in cardContainer)
            {
                cardGO.gameObject.SetActive(true);
            }
            filterPanel.SetActive(false);
        }

        private int? ParseNullableIntWithZero(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (int.TryParse(input, out int value))
                return value;

            return null;
        }

        private void PopulateDropdownWithAllOption<TEnum>(
    TMP_Dropdown dropdown,
    string allLabel,
    Func<TEnum, string> displayNameFunc = null,
    bool excludeNoColor = false,
    bool excludeCardTypeCard = false,
    bool excludeNoField = false,
    bool excludeNoAttribute = false
) where TEnum : Enum
        {
            dropdown.ClearOptions();

            List<string> options = new List<string> { allLabel };

            IEnumerable<TEnum> values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

            if (typeof(TEnum) == typeof(CardColor) && excludeNoColor)
            {
                values = values.Where(v => !v.Equals((TEnum)(object)CardColor.NoColor));
            }

            if (typeof(TEnum) == typeof(CardType) && excludeCardTypeCard)
            {
                values = values.Where(v => !v.Equals((TEnum)(object)CardType.Card));
            }

            if (typeof(TEnum) == typeof(DigimonField) && excludeNoField)
            {
                values = values.Where(v => !v.Equals((TEnum)(object)DigimonField.NoField));
            }

            if (typeof(TEnum) == typeof(DigimonAttribute) && excludeNoAttribute)
            {
                values = values.Where(v => !v.Equals((TEnum)(object)DigimonAttribute.NoAttribute));
            }

            options.AddRange(values.Select(v => displayNameFunc != null ? displayNameFunc(v) : v.ToString()));

            dropdown.AddOptions(options);
            dropdown.value = 0;
        }
        private bool IsPointerOverUIObject(GameObject uiObject)
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            foreach (var result in results)
            {
                if (result.gameObject == uiObject || result.gameObject.transform.IsChildOf(uiObject.transform))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
