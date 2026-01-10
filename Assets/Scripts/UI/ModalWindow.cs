using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModalWindow : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private RectTransform transformCardDisplay;
    [SerializeField] private RectTransform transformWindow;

    [SerializeField] private Button confirmButton;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private bool? result = null;
    private int requiredSelectionAmount;
    private Func<int> currentSelectionCountProvider;
    public bool HasResult => result.HasValue;
    public bool Result => result.Value;

    // Modal simples (OK)
    public void Setup(string title, string message, Action onConfirm, List<GameObject> cardDisplay)
    {
        titleText.text = title;
        bodyText.text = message;
        if(cardDisplay.Count > 0)
        {
            foreach (var card in cardDisplay)
            {
                GameObject currentCard = Instantiate(card, transformCardDisplay);
                SetupShowCard(currentCard);
                transformWindow.sizeDelta = new Vector2(transformWindow.sizeDelta.x, 700f);

            }
        }

        confirmButton.gameObject.SetActive(true);
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            Destroy(gameObject);
        });
    }

    // Modal Yes / No (para Coroutine)
    public void SetupYesNo(string title, string message, List<GameObject> cardDisplay)
    {
        result = null;
        titleText.text = title;
        bodyText.text = message;

        if (cardDisplay.Count > 0)
        {
            foreach (var card in cardDisplay)
            {
                GameObject currentCard = Instantiate(card, transformCardDisplay);
                transformWindow.sizeDelta = new Vector2(transformWindow.sizeDelta.x, 700f);
                SetupShowCard(currentCard);
            }
        }
        confirmButton.gameObject.SetActive(false);
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() => result = true);
        noButton.onClick.AddListener(() => result = false);
    }

    public void SetupSelectionModal(string title,string message, int requiredAmount, Func<List<GameObject>> getSelectedObjects, Action onConfirm)
    {
        titleText.text = title;
        bodyText.text = message;

        requiredSelectionAmount = requiredAmount;

        GetComponent<Image>().enabled = false;
        confirmButton.gameObject.SetActive(true);
        confirmButton.interactable = false;
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            Destroy(gameObject);
        });

        StartCoroutine(UpdateSelectionView(getSelectedObjects));
    }
    private IEnumerator UpdateSelectionView(Func<List<GameObject>> getSelectedObjects)
    {
        while (true)
        {
            foreach (Transform child in transformCardDisplay)
                Destroy(child.gameObject);

            List<GameObject> selectedObjects = getSelectedObjects.Invoke();

            foreach (var card in selectedObjects)
            {
                GameObject currentCard = Instantiate(card, transformCardDisplay);
                SetupShowCard(currentCard);
            }

            transformWindow.sizeDelta =
                selectedObjects.Count > 0
                    ? new Vector2(transformWindow.sizeDelta.x, 700f)
                    : new Vector2(transformWindow.sizeDelta.x, 500f);

            confirmButton.interactable =
                selectedObjects.Count == requiredSelectionAmount;

            yield return null;
        }
    }

    public void SetupShowCard(GameObject currentCard)
    {
        foreach (var button in currentCard.GetComponentsInChildren<Button>())
        {
            button.interactable = false;
        }
        currentCard.GetComponent<RectTransform>().localScale = new Vector3(60f, 60f, 100f);
        currentCard.GetComponent<CardSelectable>().enabled = false;
        currentCard.GetComponent<MenuCardManager>().enabled = false;
    }
}
