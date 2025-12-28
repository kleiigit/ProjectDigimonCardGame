using ProjectScript.Enums;
using SinuousProductions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } 
    public static bool isInDeckEditScreen;
    public List<GameObject> cardsInFieldBlue = new List<GameObject>();
    public List<GameObject> cardsInFieldRed = new List<GameObject>();
    [SerializeField] GameObject refCardPrefab;
    public static GameObject cardPrefab;

    public GameObject saveMenu;
    public GameObject editDeckMenu;
    public GameObject optionsMenu;
    public GameObject menuOptions;
    public GameObject infoDisplay;
    public GameObject deckList;
    public GameObject battleSetup;

    private int difficulty = 5;
    

    public OptionsManager OptionsManager { get; private set; }
    public DeckManager DeckManager { get; private set; }
    public AudioManager AudioManager { get; private set; }
    private DeckEditorUI deckEditorUI;

    private void Awake()
    {
        deckEditorUI = FindFirstObjectByType<DeckEditorUI>();
        cardPrefab = refCardPrefab;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        
        
    }
    public void ButtonActiveDesative(string menu)
    {
        deckEditorUI = FindFirstObjectByType<DeckEditorUI>();

        saveMenu.SetActive(false);
        optionsMenu.SetActive(false);
        editDeckMenu.SetActive(false);
        menuOptions.SetActive(false);
        battleSetup.SetActive(false);
        if(deckEditorUI != null)
        deckEditorUI.ObjShowEditor(false);
        isInDeckEditScreen = false;
        infoDisplay.SetActive(false);
        deckList.SetActive(false);
        
        
        switch (menu)
        {
            case "save":
                saveMenu.SetActive(true);
                break;
            case "option":
                optionsMenu.SetActive(true);
                break;
            case "editDeck":
                isInDeckEditScreen = true;
                editDeckMenu.SetActive(true);
                infoDisplay.SetActive(true);
                deckList.SetActive(true);
                break;
            case "battle":
                battleSetup.SetActive(true);
                break;
            default:

                if (isInDeckEditScreen == true) {
                    Debug.Log("Back to Editing deck. " + isInDeckEditScreen);
                    deckEditorUI.CloseSaveDeckPanel(); 
                    editDeckMenu.SetActive(true);
                    infoDisplay.SetActive(true);
                    deckList.SetActive(true);
                }
                else menuOptions.SetActive(true);
                break;
        }
                
        
    }
    public void UpdateCardField()
    {
        cardsInFieldBlue.Clear();
        cardsInFieldRed.Clear();

        FieldCard[] allFieldCards = FindObjectsByType<FieldCard>(FindObjectsSortMode.None);

        foreach (FieldCard fieldCard in allFieldCards)
        {
            if (fieldCard == null || fieldCard.parentCell == null)
                continue;

            var cardDisplay = fieldCard.GetComponent<CardDisplay>();
            if (cardDisplay == null || cardDisplay.cardData == null)
                continue;

            switch (fieldCard.parentCell.owner)
            {
                case PlayerSide.PlayerBlue:
                    if (fieldCard.gameObject != null)
                        cardsInFieldBlue.Add(fieldCard.gameObject);
                    break;
                case PlayerSide.PlayerRed:
                    if (fieldCard.gameObject != null)
                        cardsInFieldRed.Add(fieldCard.gameObject);
                    break;
            }
        }

        // Remove qualquer referência "Missing (GameObject)" residual
        cardsInFieldBlue.RemoveAll(obj => obj == null);
        cardsInFieldRed.RemoveAll(obj => obj == null);
    }

    public void AddCardToField(GameObject cardObject)
    {
        if (cardObject == null) return;

        // Atualiza as listas reconstruindo do zero para garantir consistência
        UpdateCardField();
    }

    /// <summary>
    /// Chame este método para remover uma carta do campo.
    /// Atualiza automaticamente as listas após remoção.
    /// Use coroutine para lidar com Destroy() que só ocorre no fim do frame.
    /// </summary>
    public void RemoveCardFromField(GameObject cardObject, bool isDestroyed = false, MonoBehaviour caller = null)
    {
        if (cardObject == null) return;

        if (isDestroyed && caller != null)
        {
            // Como Destroy() só ocorre no final do frame, espera para atualizar a lista
            caller.StartCoroutine(RemoveCardCoroutine());
        }
        else
        {
            // Atualiza imediatamente a lista após remoção
            UpdateCardField();
        }

        IEnumerator RemoveCardCoroutine()
        {
            yield return new WaitForEndOfFrame();
            UpdateCardField();
        }
    }
    private void InitializeManagers()
    {
        OptionsManager = GetComponentInChildren<OptionsManager>();
        AudioManager = GetComponentInChildren<AudioManager>();
        DeckManager = GetComponentInChildren<DeckManager>();

        if (OptionsManager == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/OptionsManager");
            if(prefab == null)
            {
                Debug.Log("OptionsManager prefab not found");
            }
            else
            {
                Instantiate(prefab, transform.position, Quaternion.identity, transform);
                OptionsManager = GetComponentInChildren<OptionsManager>();
            }
        }
        if (AudioManager == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/AudioManager");
            if (prefab == null)
            {
                Debug.Log("AudioManager prefab not found");
            }
            else
            {
                Instantiate(prefab, transform.position, Quaternion.identity, transform);
                AudioManager = GetComponentInChildren<AudioManager>();
            }
        }
        if (DeckManager == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/DeckManager");
            if (prefab == null)
            {
                Debug.Log("DeckManager prefab not found");
            }
            else
            {
                Instantiate(prefab, transform.position, Quaternion.identity, transform);
                DeckManager = GetComponentInChildren<DeckManager>();
            }
        }
    }

    public static bool ClickedOnCard()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.GetComponent<CardDisplay>() != null;
        }
        return false;
    }



    public int Difficulty
    {
        get { return difficulty; }
        set { difficulty = value; }
    }




}
