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

        // Raycast 2D a partir da posição da carta
        RaycastHit2D hit = Physics2D.Raycast(card.transform.position, Vector2.down);

        // Instancia o menu dentro do Canvas
        activeMenu = Instantiate(menuPrefab, transform, false);
        RectTransform menuRect = activeMenu.GetComponent<RectTransform>();
        activeMenu.GetComponent<Transform>().localScale = new Vector3(100f, 100f, 100f);

        Vector2 finalOffset = customOffset ?? menuOffset;

        if (hit.collider != null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

            RectTransform parentRect = menuRect.parent as RectTransform;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, hit.point);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                screenPoint,
                cam,
                out Vector2 localPoint))
            {
                float angleZ = card.transform.eulerAngles.z;
                Quaternion rotation = Quaternion.Euler(0f, 0f, angleZ);

                // 1. Rotaciona o menu
                menuRect.localRotation = rotation;

                // 2. Rotaciona o offset no mesmo espaço
                Vector2 rotatedOffset = rotation * finalOffset;

                // 3. Aplica posição final
                menuRect.anchoredPosition = localPoint + rotatedOffset;
            }
        }
        else
        {
            // fallback seguro
            menuRect.anchoredPosition = finalOffset;
        }
        // Criação dos botões
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
