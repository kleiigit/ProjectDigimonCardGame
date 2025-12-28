using SinuousProductions;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    

    public SaveData CurrentData { get; private set; }
    public int CurrentSlotIndex { get; private set; } = -1;

    public GameObject saveButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    public void NewGame(int slotIndex, string playerName)
    {
        CurrentData = new SaveData
        {
            playerName = playerName,
            inventory = new List<string>(),
            position = new float[3],
            discoveredCards = new List<SavedCardEntry>() // não possui cartas
        };

        CurrentSlotIndex = slotIndex;

        // Aplica uma lista vazia de cartas para garantir início limpo
        CardsCollectionManager.Instance.ApplyDiscoveredCardEntries(CurrentData.discoveredCards);

        SaveGame();
    }

    public void SaveGame()
    {
        if (CurrentSlotIndex < 0) return;

        CurrentData.discoveredCards = CardsCollectionManager.Instance.GetDiscoveredCardEntries();
        SaveUtility.SaveToSlot(CurrentData, CurrentSlotIndex);
    }

    public void LoadGame(int slotIndex)
    {
        if (!SaveUtility.SlotExists(slotIndex))
        {
            Debug.LogWarning($"Slot {slotIndex} não encontrado.");
            return;
        }

        CurrentData = SaveUtility.LoadFromSlot<SaveData>(slotIndex);
        CurrentSlotIndex = slotIndex;

        CardsCollectionManager.Instance.ApplyDiscoveredCardEntries(CurrentData.discoveredCards);
    }

    public bool SlotExists(int index) => SaveUtility.SlotExists(index);

    public SaveData LoadSlotPreview(int index)
    {
        return SaveUtility.SlotExists(index) ? SaveUtility.LoadFromSlot<SaveData>(index) : null;
    }
    public void SaveToSlotFromButton(int slotIndex)
    {
        if (CurrentData == null)
        {
            Debug.LogWarning("[SaveManager] Nenhum dado carregado. Criação de novo save deve passar por NewGame ou LoadOrCreateSlot.");
            return;
        }

        // Atualiza as cartas descobertas
        CurrentData.discoveredCards = CardsCollectionManager.Instance.GetDiscoveredCardEntries();

        CurrentSlotIndex = slotIndex;
        SaveUtility.SaveToSlot(CurrentData, slotIndex);
        Debug.Log($"[SaveManager] Save feito no slot {slotIndex}.");
    }
    public void ButtonActiveDesative()
    {
        if(saveButton.activeSelf)
        {
            this.gameObject.SetActive(true);
            saveButton.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(false);
            saveButton.SetActive(true);

        }
    }
    public void UnlockAllCardsForTesting()
    {
        var allCards = CardsCollectionManager.Instance.GetAllCardsInGame();

        List<SavedCardEntry> allDiscovered = new();

        foreach (var card in allCards)
        {
            if (card == null) continue;

            allDiscovered.Add(new SavedCardEntry(
                card.cardID,
                CardStatus.Owned,
                4
            ));
        }

        if (CurrentData == null)
        {
            CurrentData = new SaveData
            {
                playerName = "Debug Tester",
                discoveredCards = allDiscovered
            };
        }
        else
        {
            CurrentData.discoveredCards = allDiscovered;
        }

        CardsCollectionManager.Instance.ApplyDiscoveredCardEntries(CurrentData.discoveredCards);
        Debug.Log($"[TESTE] Todas as cartas foram desbloqueadas e marcadas como Owned ({allDiscovered.Count}).");
    } // Teste
    public void LoadOrCreateSlot(int slotIndex, string playerName)
    {
        if (SlotExists(slotIndex))
        {
            LoadGame(slotIndex);
            Debug.Log($"[SaveManager] Slot {slotIndex} carregado com sucesso.");
        }
        else
        {
            Debug.Log($"[SaveManager] Slot {slotIndex} está vazio. Criando novo save.");
            CreateNewEmptySave(slotIndex, playerName);
        }
    }
    private void CreateNewEmptySave(int slotIndex, string playerName)
    {
        // Não adiciona cartas, mantém lista vazia
        SaveData newData = new SaveData
        {
            playerName = playerName,
            discoveredCards = new List<SavedCardEntry>(), // nenhuma carta descoberta
            inventory = new List<string>(),
            position = new float[3]
        };

        SaveUtility.SaveToSlot(newData, slotIndex);

        CurrentSlotIndex = slotIndex;
        CurrentData = newData;

        // Nenhuma carta descoberta para aplicar

        Debug.Log($"[SaveManager] Novo save criado no slot {slotIndex} com cartas não descobertas.");
    }
}

