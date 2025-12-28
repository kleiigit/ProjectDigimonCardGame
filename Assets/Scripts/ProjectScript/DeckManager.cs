using System.Collections.Generic;
using UnityEngine;
using SinuousProductions;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
using ProjectScript.Enums;

public class DeckManager : MonoBehaviour
{
    public TMP_Dropdown redDeckDropdown;
    public TMP_Dropdown blueDeckDropdown;

    public static Dictionary<PlayerSide,List<Card>> deckMain = new();
    public static Dictionary<PlayerSide,List<Card>> deckPartner = new();
    public List<DeckData> allDecks = new();

    public List<Card> cardDatabase = new List<Card>();

    private void Start()
    {
        if (CardsCollectionManager.Instance != null)
        {
            cardDatabase = CardsCollectionManager.Instance.GetAllCardsInGame();
        }
        else
        {
            Debug.LogWarning("[DeckEditorUI] CardsCollectionManager não encontrado.");
            cardDatabase = new List<Card>();
        }
    }

    public void StartGameCheck()
    {
        List<Card> _maindeck = new();
        List<Card> _partnerDeck = new();

        if (allDecks == null || allDecks.Count == 0)
        {
            Debug.LogError("[DeckManager] Lista de decks está vazia.");
            return;
        }

        if (redDeckDropdown == null || blueDeckDropdown == null)
        {
            Debug.LogError("[DeckManager] Dropdowns não estão atribuídos.");
            return;
        }


        int redIndex = redDeckDropdown.value;
        int blueIndex = blueDeckDropdown.value;

        if (redIndex < 0 || redIndex >= allDecks.Count || blueIndex < 0 || blueIndex >= allDecks.Count)
        {
            Debug.LogError("[DeckManager] Índices de dropdown inválidos.");
            return;
        }

        DeckData redDeckData = allDecks[redIndex];
        DeckData blueDeckData = allDecks[blueIndex];
        // Converte os DeckData em listas de cartas usando seu DeckConverter

        DeckConverter.FromDeckData(redDeckData, out _maindeck, out _partnerDeck, cardDatabase);
        deckMain[PlayerSide.PlayerRed] = _maindeck;
        deckPartner[PlayerSide.PlayerRed] = _partnerDeck;

        DeckConverter.FromDeckData(blueDeckData, out _maindeck, out _partnerDeck, cardDatabase);
        deckMain[PlayerSide.PlayerBlue] = _maindeck;
        deckPartner[PlayerSide.PlayerBlue] = _partnerDeck;

        SceneManager.LoadScene("BattleScene"); // Substitua pelo nome correto da sua cena
    }

    public void SetDeckListAndPopulate(List<DeckData> decks)
    {
        allDecks = decks;
        PopulateDeckDropdownsFromDeckList();
    }
    public void PopulateDeckDropdownsFromDeckList()
    {
        if (allDecks == null || allDecks.Count == 0)
        {
            Debug.LogWarning("[DeckManager] Nenhum deck encontrado para preencher os dropdowns.");
            return;
        }

        List<string> deckNames = allDecks.Select(d => d.deckName).ToList();

        if (redDeckDropdown != null)
        {
            redDeckDropdown.ClearOptions();
            redDeckDropdown.AddOptions(deckNames);
            redDeckDropdown.value = 0;
            redDeckDropdown.RefreshShownValue();
        }

        if (blueDeckDropdown != null)
        {
            blueDeckDropdown.ClearOptions();
            blueDeckDropdown.AddOptions(deckNames);
            blueDeckDropdown.value = 0;
            blueDeckDropdown.RefreshShownValue();
        }
    }


}