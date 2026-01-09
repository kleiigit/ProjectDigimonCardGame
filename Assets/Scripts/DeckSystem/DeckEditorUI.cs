using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectScript.Enums;

namespace SinuousProductions
{
    public class DeckEditorUI : MonoBehaviour
    {
        public Vector2 displayDeck;
        #region("Referencias")
        [Header("Referências de UI")]
        [SerializeField] private Button createDeckButton;
        [SerializeField] private Button collectionButton; // Botão para abrir a coleção de cartas
        [SerializeField] private Transform deckListContent;
        [SerializeField] private GameObject deckItemPrefab;
        [SerializeField] private DeckStatisticsDisplay statisticsDisplay;
        [SerializeField] private DeckManager deckManager; // Referência ao DeckManager para acessar os decks
        [SerializeField] private CardFilterManager cardFilterManager;
        [SerializeField] private TMP_Text sortStatusText;
        public GameObject collectionPanel;
        public GameObject deckListPanel;
        [SerializeField] private TMP_Text deckNameText;
        [SerializeField] private RectTransform mainDeckPanel;
        [SerializeField] private RectTransform partnerDeckPanel;
        [SerializeField] private RectTransform mainDeckContent;
        [SerializeField] private RectTransform partnerDeckContent;
        [SerializeField] private RectTransform collectionContent;
        [SerializeField] private RectTransform showEditDeck; // Referência para o painel de edição do deck, deve ser arrastada no Inspector

        [Header("UI Salvar Deck")]
        [SerializeField] private GameObject saveDeckPanel;  // Painel modal para salvar deck
        [SerializeField] private TMP_InputField deckNameInputField; // Campo para digitar nome do deck
        [SerializeField] private Button saveDeckConfirmButton;
        [SerializeField] private Button saveDeckCancelButton;

        [Header("DisplayListCards Instâncias")]
        [SerializeField] private DisplayListCards mainDeckDisplay;
        [SerializeField] private DisplayListCards partnerDeckDisplay;
        [SerializeField] private DisplayListCards collectionDisplay; // nova referência para a coleção

        [Header("Botões de Ordenação")]
        [SerializeField] private GameObject sortOptionsPanel;
        [SerializeField] private Button sortToggleButton;
        [SerializeField] private Button sortByLevelButton;
        [SerializeField] private Button sortByNameButton;
        [SerializeField] private Button sortByPowerButton;
        [SerializeField] private Button sortByCostButton;
        [SerializeField] private Button sortByTypeButton;
        [SerializeField] private Button sortByStageButton;
        [SerializeField] private Button sortByColorButton;
        [SerializeField] private Button sortByNumberButton;
        [SerializeField] private Button sortByAttributeButton;
        [SerializeField] private Button sortByTypeDigimonButton;

        [SerializeField] private Button sortByFieldButton;


        [SerializeField] private Button sortByQuantityButton; // Botão para ordenar por quantidade, se necessário

        string newDeckName = "Novo Baralho";
        public float animationMove = 0.5f; // Velocidade da animação de movimento das cartas
        public TMP_Text deckCount;
        public TMP_Text securityCount;
        private Dictionary<string, int> cardQuantities = new Dictionary<string, int>();
        private const int MaxDecks = 10;
        private LayerMask layerMask = 14;

        [Header("Banco de Cartas")]
        [SerializeField] public List<Card> cardDatabase = new List<Card>(); // Banco de dados de cartas, preenchido pelo CardsCollectionManager

        private List<DeckData> allDecks = new();
        private int editingDeckIndex = -1;
        #endregion
        private void Start()
        {
           
            // Popula cardDatabase com as cartas da coleção
            if (CardsCollectionManager.Instance != null)
            {
                cardDatabase = CardsCollectionManager.Instance.GetAllCardsInGame();
            }
            else
            {
                Debug.LogWarning("[DeckEditorUI] CardsCollectionManager não encontrado.");
                cardDatabase = new List<Card>();
            }
            cardQuantities.Clear();
            foreach (var card in cardDatabase)
            {
                // Aqui você deve preencher cardQuantities com base nos dados reais da sua coleção.
                // Exemplo genérico:
                cardQuantities[card.cardID] = 1; // ajuste conforme sua lógica
            }
            var sortButtons = new Dictionary<SortMode, Button>()
            {
                { SortMode.LevelAsc, sortByLevelButton },
                { SortMode.NameAZ, sortByNameButton },
                { SortMode.CostAsc, sortByCostButton },
                { SortMode.PowerAsc, sortByPowerButton },
                { SortMode.TypeMode1, sortByTypeButton },
                { SortMode.StageAsc, sortByStageButton },
                { SortMode.Color, sortByColorButton },
                { SortMode.QuantityAsc, sortByQuantityButton },
                { SortMode.Field, sortByFieldButton },
                { SortMode.Attribute, sortByAttributeButton },
                { SortMode.TypeDigimon, sortByTypeDigimonButton },
                { SortMode.CardIDAsc, sortByNumberButton } 
            };

            DeckCardSorter.InitializeSorterUI(
                sortToggleButton,
                sortOptionsPanel,
                sortButtons,
                cardDatabase,
                cardQuantities,      // <- Passar o dicionário aqui
                OnSortApplied);

            sortOptionsPanel.gameObject.SetActive(false);
            // O restante do Start original
            createDeckButton.onClick.AddListener(() => StartNewDeck());
            deckManager = DeckManager.FindFirstObjectByType<DeckManager>();

            if (SaveManager.Instance != null && SaveManager.Instance.CurrentData == null)
            {
                SaveManager.Instance.LoadOrCreateSlot(0, "Jogador");
                Debug.Log("[DeckEditorUI] Slot carregado automaticamente.");
            }

            if (SaveManager.Instance != null && SaveManager.Instance.CurrentData != null)
            {
                allDecks = SaveManager.Instance.CurrentData.savedDecks;
            }
            
            ShowDeckListPanel();

            if (saveDeckConfirmButton != null)
                saveDeckConfirmButton.onClick.AddListener(OnSaveDeckConfirmed);

            if (saveDeckCancelButton != null)
                saveDeckCancelButton.onClick.AddListener(CloseSaveDeckPanel);

            if (saveDeckPanel != null)
                saveDeckPanel.SetActive(false);
        }
        private void Update()
        {
            deckCount.text = "Deck " + mainDeckContent.childCount.ToString()
                        + " / " + partnerDeckContent.childCount.ToString();

            if (editingDeckIndex >= 0 && editingDeckIndex < allDecks.Count)
            {
                DeckData currentDeck = allDecks[editingDeckIndex];
                int security = CountSecurityEffectCards(currentDeck.mainDeck);
                securityCount.text = $"Security {security} / 20";
            }
        }
        private void ShowDeckListPanel()
        {
            RefreshDeckList();
        }
        private void StartNewDeck()
        {
            if (allDecks.Count >= MaxDecks)
            {
                Debug.LogWarning("[DeckEditorUI] Limite máximo de decks atingido.");
                return;
            }

            DeckData newDeck = new DeckData
            {
                deckName = newDeckName,
                mainDeck = new List<DeckCardEntry>(),
                partnerDeck = new List<DeckCardEntry>()
            };

            allDecks.Add(newDeck);
            int newDeckIndex = allDecks.Count - 1;

            EditDeck(newDeckIndex);
            UpdateDeckCreationButtons(); // Atualiza visibilidade dos botões após criação
        }
        private void EditDeck(int index)
        {
            if (index < 0 || index >= allDecks.Count)
            {
                Debug.LogWarning($"[DeckEditorUI] Índice inválido para edição de deck: {index}");
                return;
            }

            editingDeckIndex = index;
            
            DeckData deck = allDecks[index];
            deckNameText.text = deck.deckName;

            ShowEditorPanel();
            
            UpdateDeckDisplays(deck);
        }
        public void ClearDeckVisuals() // Limpa os decks
        {
            // Limpa a parte visual
            foreach (Transform child in mainDeckContent)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in partnerDeckContent)
            {
                Destroy(child.gameObject);
            }

            // Limpa os dados do deck em edição
            if (editingDeckIndex >= 0 && editingDeckIndex < allDecks.Count)
            {
                allDecks[editingDeckIndex].mainDeck.Clear();
                allDecks[editingDeckIndex].partnerDeck.Clear();
            }

            // Atualiza a exibição da coleção
            if (collectionDisplay != null && CardsCollectionManager.Instance != null)
            {
                CardsCollectionManager.Instance.ShowAllCardsInGame(collectionDisplay);
            }
        }
        private void UpdateDeckDisplays(DeckData deck)
        {
            // Converte DeckCardEntry para lista de cartas instanciadas, repetindo conforme quantidade
            List<Card> mainDeckCards = new List<Card>();
            foreach (var entry in deck.mainDeck)
            {
                Card card = cardDatabase.FirstOrDefault(c => c.cardID == entry.cardID);
                if (card != null)
                {
                    for (int i = 0; i < entry.quantity; i++)
                        mainDeckCards.Add(card);
                }
            }

            List<Card> partnerDeckCards = new List<Card>();
            foreach (var entry in deck.partnerDeck)
            {
                Card card = cardDatabase.FirstOrDefault(c => c.cardID == entry.cardID);
                if (card != null)
                {
                    for (int i = 0; i < entry.quantity; i++)
                        partnerDeckCards.Add(card);
                }
            }

            // Ordena a lista completa de cartas (deck todo)
            DeckCardSorter.SortCardsDefault(mainDeckCards);
            DeckCardSorter.SortCardsDefault(partnerDeckCards);

            // Atualiza o Display usando as listas ordenadas
            mainDeckDisplay.SetCardGridContent(mainDeckContent);
            mainDeckDisplay.ShowCardList(mainDeckCards, layerMask, "Cartas do Baralho Principal", false);

            partnerDeckDisplay.SetCardGridContent(partnerDeckContent);
            partnerDeckDisplay.ShowCardList(partnerDeckCards, layerMask, "Cartas do Baralho Parceiro", false);

            // Atualiza coleção normalmente
            collectionDisplay.SetCardGridContent(collectionContent);
            collectionDisplay.collectionCards = true;
            CardsCollectionManager.Instance.ShowAllCardsInGame(collectionDisplay);
            UpdateCollectionVisualCounters();

            if (statisticsDisplay != null)
            {
                statisticsDisplay.ShowStatistics(deck.mainDeck, deck.partnerDeck, cardDatabase);
            }
        }
        private void ShowEditorPanel()
        {
            Debug.Log("Mostrando painel de edição");
            
            collectionDisplay.SetCardGridLayout(new Vector2(460f, 2000f), new Vector2(327f, 0f));
            collectionPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(601f, -81f); // posição da coleção
            RectTransform rt = collectionPanel.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(653f, 905.26f); // tamanho do coleção

            ObjShowEditor(true);
            deckListPanel.SetActive(false);
            createDeckButton.gameObject.SetActive(false);
            collectionButton.gameObject.SetActive(false);

        }
        public void ObjShowEditor(bool show)
        {
            showEditDeck.gameObject.SetActive(show);
            mainDeckPanel.gameObject.SetActive(show);
            partnerDeckPanel.gameObject.SetActive(show);
            collectionDisplay.gameObject.SetActive(show);
            collectionDisplay.gameObject.SetActive(show);
        }
        private void DeleteDeck(int index)
        {
            allDecks.RemoveAt(index);
            RefreshDeckList();
        }
        private void ViewDeckList(int index)
        {
            if (index < 0 || index >= allDecks.Count)
            {
                Debug.LogWarning($"[DeckEditorUI] Índice de deck inválido: {index}");
                return;
            }

            if (cardDatabase == null || cardDatabase.Count == 0)
            {
                Debug.LogError("[DeckEditorUI] cardDatabase está vazio. As cartas não poderão ser carregadas.");
                return;
            }

            DeckData deckData = allDecks[index];

            Debug.Log($"[DeckEditorUI] Visualizando deck '{deckData.deckName}' com {deckData.mainDeck.Count} cartas no main e {deckData.partnerDeck.Count} no partner.");

            foreach (var entry in deckData.mainDeck.Concat(deckData.partnerDeck))
            {
                if (!cardDatabase.Any(c => c.cardID == entry.cardID))
                {
                    Debug.LogWarning($"[DeckEditorUI] cardID '{entry.cardID}' não encontrado no cardDatabase.");
                }
            }

            DeckConverter.FromDeckData(deckData, out List<Card> mainDeck, out List<Card> partnerDeck, cardDatabase);

            if ((mainDeck == null || mainDeck.Count == 0) && (partnerDeck == null || partnerDeck.Count == 0))
            {
                Debug.LogWarning("[DeckEditorUI] Nenhuma carta foi carregada para visualização.");
                return;
            }

            List<Card> allCards = new();
            allCards.AddRange(mainDeck);
            allCards.AddRange(partnerDeck);

            // Define currentSortMode para ViewDeckOrder e zera previousSortMode para evitar ordem acumulada
            var currentSortModeField = typeof(DeckCardSorter).GetField("currentSortMode", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (currentSortModeField != null)
                currentSortModeField.SetValue(null, SortMode.ViewDeckOrder);

            var previousSortModeField = typeof(DeckCardSorter).GetField("previousSortMode", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (previousSortModeField != null)
                previousSortModeField.SetValue(null, null);

            // Aplica ordenação completa ignorando a ordem anterior
            DeckCardSorter.SortCards(allCards);

            string description = $"Visualizando deck: {deckData.deckName}";
            collectionDisplay.collectionCards = false;
            collectionDisplay.SetCardGridContent(collectionContent);

            Debug.Log($"allCards count. {allCards.Count} cartas carregadas para visualização.");

            collectionDisplay.ShowCardList(allCards, layerMask, description, false);

            collectionPanel.SetActive(true);
            collectionDisplay.gameObject.SetActive(true);

            showEditDeck.gameObject.SetActive(false);
            mainDeckPanel.gameObject.SetActive(false);
            partnerDeckPanel.gameObject.SetActive(false);

            collectionDisplay.SetCardGridLayout(new Vector2(633f, 2000f), new Vector2(364f, 0f));
            RectTransform rt = collectionPanel.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(581f, -82f);
            rt.sizeDelta = new Vector2(731f, 906f);

            Debug.Log("[DeckEditorUI] Deck visualizado com sucesso.");
        }
        private void RefreshDeckList()
        {
            foreach (Transform child in deckListContent)
                Destroy(child.gameObject);

            for (int i = 0; i < allDecks.Count; i++)
            {
                DeckData deck = allDecks[i];
                GameObject item = Instantiate(deckItemPrefab, deckListContent);
                deckListContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0); // Ajusta o tamanho do conteúdo
                //

                TextMeshProUGUI deckNameText = item.GetComponentInChildren<TextMeshProUGUI>();
                if (deckNameText != null)
                    deckNameText.text = deck.deckName;

                DeckSlot slot = item.GetComponent<DeckSlot>();
                if (slot != null)
                    slot.Setup(i, this);
            }
            deckManager.SetDeckListAndPopulate(allDecks);
            UpdateDeckCreationButtons();
        }
        public void OnEditDeck(int index)
        {
            EditDeck(index);
        }
        public void OnDeleteDeck(int index)
        {
            DeleteDeck(index);
        }
        public void OnViewDeck(int index)
        {
            ViewDeckList(index);
        }
        #region("Copiar Decks")
        public void OnCopyDeck(int index)
        {
            if (allDecks.Count >= MaxDecks)
            {
                Debug.LogWarning("[DeckEditorUI] Limite máximo de decks atingido. Cópia não permitida.");
                return;
            }

            if (index < 0 || index >= allDecks.Count)
            {
                Debug.LogWarning($"[DeckEditorUI] Índice de deck inválido para cópia: {index}");
                return;
            }

            DeckData originalDeck = allDecks[index];
            DeckData copiedDeck = new DeckData
            
            {
                deckName = originalDeck.deckName + ",1",
                mainDeck = new List<DeckCardEntry>(originalDeck.mainDeck),
                partnerDeck = new List<DeckCardEntry>(originalDeck.partnerDeck)
            };

            allDecks.Add(copiedDeck);
            RefreshDeckList();
            UpdateDeckCreationButtons(); // Atualiza visibilidade dos botões após cópia
        }
        private void UpdateDeckCreationButtons()
        {
            bool canCreate = allDecks.Count < MaxDecks;
            createDeckButton.interactable = canCreate;

            // Caso tenha botões visuais de cópia por deck, você pode controlar aqui também.
            DeckSlot[] slots = deckListContent.GetComponentsInChildren<DeckSlot>();
            foreach (var slot in slots)
            {
                slot.SetCopyButtonInteractable(canCreate);
            }
        }
        #endregion
        #region("Editar Decks")
        public bool AddCardToEditingDeck(Card card, ref int currentValue)
        {
            if (card == null || currentValue <= 0 || editingDeckIndex < 0 || editingDeckIndex >= allDecks.Count)
            {
                Debug.LogWarning("[DeckEditorUI] Parâmetros inválidos para adicionar carta ao deck.");
                return false;
            }

            DeckData deck = allDecks[editingDeckIndex];
            List<DeckCardEntry> targetDeck = null;
            DisplayListCards targetDisplay = null;
            RectTransform targetDeckContent = null;
            int maxCopies = 0;

            if (card.cardType == CardType.Digimon || card.cardType == CardType.Program)
            {
                int totalMainDeckCards = deck.mainDeck.Sum(e => e.quantity);
                if (totalMainDeckCards >= 40)
                {
                    Debug.LogWarning("[DeckEditorUI] O deck principal já possui 40 cartas. Não é possível adicionar mais.");
                    return false;
                }

                targetDeck = deck.mainDeck;
                targetDisplay = mainDeckDisplay;
                targetDeckContent = mainDeckContent;
                maxCopies = 4;

                bool isSecurityCard = card.effects != null && card.effects.Any(e => e.trigger == CardEffects.Trigger.Security);
                if (isSecurityCard)
                {
                    int currentSecurityCount = CountSecurityEffectCards(deck.mainDeck);
                    DeckCardEntry securityEntry = targetDeck.FirstOrDefault(e => e.cardID == card.cardID);
                    int nextQuantity = securityEntry != null ? securityEntry.quantity + 1 : 1;

                    if (currentSecurityCount + 1 > 20 && (securityEntry == null || securityEntry.quantity < nextQuantity))
                    {
                        Debug.LogWarning("[DeckEditorUI] Limite de 20 cartas com efeito Security atingido.");
                        return false;
                    }
                }
            }
            else if (card.cardType == CardType.Partner || card.cardType == CardType.Skill)
            {
                int totalPartnerDeckCards = deck.partnerDeck.Sum(e => e.quantity);
                if (totalPartnerDeckCards >= 10)
                {
                    Debug.LogWarning("[DeckEditorUI] O deck de parceiro já possui 10 cartas. Não é possível adicionar mais.");
                    return false;
                }

                targetDeck = deck.partnerDeck;
                targetDisplay = partnerDeckDisplay;
                targetDeckContent = partnerDeckContent;
                maxCopies = 1;
            }
            else
            {
                Debug.LogWarning($"[DeckEditorUI] Tipo de carta não suportado: {card.cardType}");
                return false;
            }

            DeckCardEntry existingEntry = targetDeck.FirstOrDefault(e => e.cardID == card.cardID);
            if (existingEntry != null)
            {
                if (existingEntry.quantity >= maxCopies)
                {
                    Debug.LogWarning("[DeckEditorUI] Limite máximo de cópias atingido.");
                    return false;
                }
                existingEntry.quantity++;
            }
            else
            {
                targetDeck.Add(new DeckCardEntry(card.cardID, 1));
            }

            // Encontra o GameObject da carta na coleção
            GameObject collectionCardObject = null;
            foreach (Transform child in collectionContent)
            {
                CardDisplay display = child.GetComponent<CardDisplay>();
                if (display != null && display.cardData != null && display.cardData.cardID == card.cardID)
                {
                    collectionCardObject = child.gameObject;
                    break;
                }
            }

            if (collectionCardObject != null && targetDeckContent != null)
            {
                Vector3 targetPos = targetDeckContent.childCount > 0
                    ? targetDeckContent.GetChild(targetDeckContent.childCount - 1).position
                    : targetDeckContent.position;

                CardMoveAnimator.AnimateCardMovement(
    collectionCardObject,
    targetPos,
    animationMove,
    false,
    () =>
    {
        // Atualiza visual
        targetDisplay.AddCardToDisplay(card);

        // Expande o deck para uma lista com repetições
        List<Card> expandedDeck = new List<Card>();
        foreach (var entry in targetDeck)
        {
            Card c = cardDatabase.FirstOrDefault(cd => cd.cardID == entry.cardID);
            if (c != null)
            {
                for (int i = 0; i < entry.quantity; i++)
                    expandedDeck.Add(c);
            }
        }

        // Ordena o deck expandido
        DeckCardSorter.SortCardsDefault(expandedDeck);

        // Mapeia os objetos visuais para os cards
        Dictionary<Transform, CardDisplay> visualCards = new Dictionary<Transform, CardDisplay>();
        foreach (Transform child in targetDeckContent)
        {
            var display = child.GetComponent<CardDisplay>();
            if (display != null)
                visualCards[child] = display;
        }

        // Reordena visualmente os filhos de targetDeckContent conforme expandedDeck
        int visualIndex = 0;
        for (int i = 0; i < expandedDeck.Count; i++)
        {
            Card cardAtIndex = expandedDeck[i];
            // Busca o primeiro filho não posicionado com essa carta
            for (; visualIndex < targetDeckContent.childCount; visualIndex++)
            {
                var child = targetDeckContent.GetChild(visualIndex);
                if (visualCards.TryGetValue(child, out CardDisplay cd) && cd.cardData.cardID == cardAtIndex.cardID)
                {
                    child.SetSiblingIndex(i);
                    visualIndex++;
                    break;
                }
            }
        }

        if (statisticsDisplay != null && editingDeckIndex >= 0 && editingDeckIndex < allDecks.Count)
        {
            statisticsDisplay.ShowStatistics(deck.mainDeck, deck.partnerDeck, cardDatabase);
        }
    }
);
            }

            currentValue = Mathf.Max(0, currentValue - 1);

            return true;
        }



        public GameObject RemoveCardDeck(Card card)
        {
            if (collectionContent == null || card == null || editingDeckIndex < 0 || editingDeckIndex >= allDecks.Count)
                return null;

            // Reduz uma cópia no deck de edição
            DeckData deck = allDecks[editingDeckIndex];
            List<DeckCardEntry> targetDeck = null;

            if (card.cardType == CardType.Digimon || card.cardType == CardType.Program)
            {
                targetDeck = deck.mainDeck;
            }
            else if (card.cardType == CardType.Partner || card.cardType == CardType.Skill)
            {
                targetDeck = deck.partnerDeck;
            }

            if (targetDeck != null)
            {
                DeckCardEntry existingEntry = targetDeck.FirstOrDefault(e => e.cardID == card.cardID);
                if (existingEntry != null)
                {
                    existingEntry.quantity--;

                    if (existingEntry.quantity <= 0)
                        targetDeck.Remove(existingEntry);
                }
            }

            if (statisticsDisplay != null && editingDeckIndex >= 0 && editingDeckIndex < allDecks.Count)
            {
                statisticsDisplay.ShowStatistics(allDecks[editingDeckIndex].mainDeck, allDecks[editingDeckIndex].partnerDeck, cardDatabase);
            }

            GameObject deckCardGO = null;

            // Encontrar GameObject da carta que está no deck (para iniciar animação dela)
            if (targetDeck != null)
            {
                RectTransform targetDeckContent = null;

                if (card.cardType == CardType.Digimon || card.cardType == CardType.Program)
                    targetDeckContent = mainDeckContent;
                else if (card.cardType == CardType.Partner || card.cardType == CardType.Skill)
                    targetDeckContent = partnerDeckContent;

                if (targetDeckContent != null)
                {
                    for (int i = targetDeckContent.childCount - 1; i >= 0; i--)
                    {
                        Transform child = targetDeckContent.GetChild(i);
                        CardDisplay display = child.GetComponent<CardDisplay>();
                        if (display != null && display.cardData.cardID == card.cardID)
                        {
                            deckCardGO = child.gameObject;
                            break;
                        }
                    }
                }
            }

            if (deckCardGO != null)
            {
                Vector3 targetPos = collectionPanel.transform.position;
                CardMoveAnimator.AnimateCardMovement(
                    deckCardGO,
                    targetPos,
                    animationMove,
                    false,  // Não destruir o original
                    null
                );
            }

            // Ativa e retorna o GameObject da carta na coleção (para referência visual)
            foreach (Transform child in collectionContent)
            {
                CardDisplay display = child.GetComponent<CardDisplay>();
                if (display != null && display.cardData.cardID == card.cardID)
                {
                    display.gameObject.SetActive(true);  // Ativa a carta para garantir visibilidade
                    return child.gameObject;
                }
            }

            return null;
        }
        public void UpdateCollectionVisualCounters()
        {
            if (editingDeckIndex < 0 || editingDeckIndex >= allDecks.Count)
                return;

            DeckData deck = allDecks[editingDeckIndex];
            List<DeckCardEntry> deckCards = new List<DeckCardEntry>();
            deckCards.AddRange(deck.mainDeck);
            deckCards.AddRange(deck.partnerDeck);

            foreach (Transform cardTransform in collectionContent)
            {
                CardDisplay display = cardTransform.GetComponent<CardDisplay>();
                if (display == null || display.cardData == null)
                    continue;

                DeckCardEntry deckEntry = deckCards.FirstOrDefault(d => d.cardID == display.cardData.cardID);
                if (deckEntry == null)
                    continue;

                TextMeshProUGUI counterText = cardTransform.GetComponentsInChildren<TextMeshProUGUI>(true)
                    .FirstOrDefault(t => t.gameObject.name == "CounterText");

                if (counterText != null)
                {
                    string rawText = counterText.text.Trim().ToLower();
                    if (rawText.StartsWith("x") && int.TryParse(rawText.Substring(1), out int currentValue))
                    {
                        int newValue = Mathf.Max(0, currentValue - deckEntry.quantity);
                        counterText.text = $"x{newValue}";
                    }
                }
            }
        }
        private int CountSecurityEffectCards(List<DeckCardEntry> deck) // security control
        {
            int count = 0;

            foreach (var entry in deck)
            {
                Card card = cardDatabase.FirstOrDefault(c => c.cardID == entry.cardID);
                if (card != null && card.effects != null)
                {
                    bool hasSecurityEffect = card.effects.Any(e => e.trigger == CardEffects.Trigger.Security);
                    if (hasSecurityEffect)
                        count += entry.quantity;
                }
            }

            return count;
        }
        #region(Sort Config)
        private void OnSortApplied(List<Card> sortedCards)
        {
            Debug.Log("[DeckEditorUI] OnSortApplied chamado. Cartas ordenadas: " + sortedCards.Count);

            if (collectionContent != null)
            {
                Dictionary<string, Transform> cardToTransform = new Dictionary<string, Transform>();
                foreach (Transform child in collectionContent)
                {
                    var cardDisplay = child.GetComponent<CardDisplay>();
                    if (cardDisplay != null && cardDisplay.cardData != null)
                        cardToTransform[cardDisplay.cardData.cardID] = child;
                }

                for (int i = 0; i < sortedCards.Count; i++)
                {
                    if (cardToTransform.TryGetValue(sortedCards[i].cardID, out Transform t))
                        t.SetSiblingIndex(i);
                }

                var layoutGroup = collectionContent.GetComponent<LayoutGroup>();
                if (layoutGroup != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(collectionContent);
            }

            if (sortStatusText != null)
            {
                string primary = GetSortModeLabel(DeckCardSorter.GetCurrentSortMode());
                string secondary = DeckCardSorter.GetPreviousSortMode() is SortMode prev && prev != SortMode.Default
                    ? GetSortModeLabel(prev)
                    : null;

                sortStatusText.text = secondary != null
                    ? $"Sort: {primary} / {secondary}"
                    : $"Sort: {primary}";
            }

            UpdateCollectionVisualCounters();
        }
        private string GetSortModeLabel(SortMode mode)
        {
            return mode switch
            {
                SortMode.LevelAsc => "Lv >",
                SortMode.LevelDesc => "Lv <",
                SortMode.TypeMode1 => "type Mode 1",
                SortMode.TypeMode2 => "type Mode 2",
                SortMode.TypeMode3 => "type Mode 3",
                SortMode.NameAZ => "Name A-Z",
                SortMode.NameZA => "Name Z-A",
                SortMode.CostAsc => "Cost >",
                SortMode.CostDesc => "Cost <",
                SortMode.Color => "Color",
                SortMode.StageAsc => "Stage >",
                SortMode.StageDesc => "Stage <",
                SortMode.QuantityAsc => "Quantity >",
                SortMode.QuantityDesc => "Quantity <",
                SortMode.PowerAsc => "Power <",
                SortMode.PowerDesc => "Power <",
                SortMode.CardIDAsc => "Number >",
                SortMode.CardIDDesc => "Number <",
                SortMode.Field => "Field",
                SortMode.TypeDigimon => "type",
                SortMode.Attribute => "Attribute",
                _ => "Default",
            };
        }
        #endregion

        public void OpenSaveDeckPanel()
        {
            if (saveDeckPanel == null) return;

            saveDeckPanel.SetActive(true);

            if (deckNameInputField != null)
            {
                deckNameInputField.text = (editingDeckIndex >= 0 && editingDeckIndex < allDecks.Count)
                    ? allDecks[editingDeckIndex].deckName
                    : "";
                deckNameInputField.ActivateInputField();
            }
        }
        public void CloseSaveDeckPanel()
        {
            if (saveDeckPanel != null)
                saveDeckPanel.SetActive(false);

            ObjShowEditor(false);
            deckListPanel.SetActive(true);
            createDeckButton.gameObject.SetActive(true);
            collectionButton.gameObject.SetActive(true);


            // Ordena a coleção pela ordenação padrão
            if (cardDatabase != null && cardDatabase.Count > 0)
            {
                DeckCardSorter.SortCardsDefault(cardDatabase);
                collectionDisplay.ShowCardList(cardDatabase, layerMask, "Todas as Cartas", false);

                if (sortStatusText != null)
                    sortStatusText.text = "Sort: Default";

                UpdateCollectionVisualCounters();
            }
            GameManager.isInDeckEditScreen = false;
        }
        private void OnSaveDeckConfirmed()
        {
            if (editingDeckIndex < 0 || editingDeckIndex >= allDecks.Count)
            {
                Debug.LogWarning("[DeckEditorUI] Nenhum deck selecionado para salvar.");
                return;
            }
            if (SaveManager.Instance == null)
            {
                Debug.LogWarning("[DeckEditorUI] SaveManager não encontrado na cena.");
                return;
            }

            if (SaveManager.Instance.CurrentData == null)
            {
                Debug.LogWarning("[DeckEditorUI] Dados atuais do save não carregados.");
                return;
            }
            string newName = deckNameInputField.text.Trim();

            if (string.IsNullOrEmpty(newName))
            {
                Debug.LogWarning("[DeckEditorUI] Nome do deck não pode ser vazio.");
                return;
            }

            // Atualiza o nome do deck na lista local
            allDecks[editingDeckIndex].deckName = newName;

            // Atualiza o SaveData atual no SaveManager
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentData != null)
            {
                SaveManager.Instance.CurrentData.savedDecks = allDecks;
                SaveManager.Instance.SaveGame();
            }
            else
            {
                Debug.LogWarning("[DeckEditorUI] SaveManager ou CurrentData não encontrados para salvar.");
            }

            RefreshDeckList();  // Atualiza visual da lista

            CloseSaveDeckPanel();
        }
        #endregion
    }
}
