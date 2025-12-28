using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class CardContextMenu : MonoBehaviour
{
    public static CardContextMenu Instance;

    [Header("Configuração do Menu")]
    public GameObject menuPrefab; // Prefab base (um painel vazio)
    public GameObject buttonPrefab; // Prefab de botão

    private GameObject activeMenu;

    [Tooltip("Offset em relação à carta")]
    public Vector2 menuOffset = new Vector2(100f, 50f);

    [System.Serializable]
    public class MenuOption
    {
        public string label;
        public System.Action onClick;

        public MenuOption(string label, System.Action onClick)
        {
            this.label = label;
            this.onClick = onClick;
        }
    }

    public void ShowMenu(GameObject card, List<MenuOption> options, Vector2? customOffset = null)
    {
        if (menuPrefab == null || buttonPrefab == null || card == null)
        {
            Debug.LogError("Menu, botão ou carta não atribuídos!");
            return;
        }

        HideMenu();

        // Procura o filho "CardCanvas" dentro da carta
        Transform cardCanvas = card.transform.Find("CardCanvas");
        if (cardCanvas == null)
        {
            Debug.LogError("[CardContextMenu] A carta não possui um filho chamado 'CardCanvas'.");
            return;
        }

        // Instancia o menu dentro do CardCanvas
        activeMenu = Instantiate(menuPrefab, cardCanvas, false);
        RectTransform menuRect = activeMenu.GetComponent<RectTransform>();

        // Offset configurável (ou customizado)
        Vector2 finalOffset = customOffset ?? menuOffset;
        menuRect.anchoredPosition = finalOffset;

        // Cria os botões
        foreach (var option in options)
        {
            GameObject btn = Instantiate(buttonPrefab, menuRect);
            btn.GetComponentInChildren<TMPro.TMP_Text>().text = option.label;

            Button buttonComp = btn.GetComponent<Button>();
            buttonComp.onClick.AddListener(() =>
            {
                option.onClick?.Invoke();
                HideMenu();
            });
        }

        // Garante que o menu fique na frente de outros elementos
        menuRect.SetAsLastSibling();

        Instance = this;
    }

    public void HideMenu()
    {
        if (activeMenu != null)
            Destroy(activeMenu);

        activeMenu = null;
        Instance = null;
    }

    private void Update()
    {
        if (activeMenu == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUI())
            {
                HideMenu();
            }
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
