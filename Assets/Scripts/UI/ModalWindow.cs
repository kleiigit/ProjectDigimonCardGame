using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ModalWindow : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    [SerializeField] private Button confirmButton;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private bool? result = null;

    public bool HasResult => result.HasValue;
    public bool Result => result.Value;

    // Modal simples (OK)
    public void Setup(string title, string message, Action onConfirm)
    {
        titleText.text = title;
        bodyText.text = message;

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
    public void SetupYesNo(string title, string message)
    {
        result = null;
        titleText.text = title;
        bodyText.text = message;

        confirmButton.gameObject.SetActive(false);
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() => result = true);
        noButton.onClick.AddListener(() => result = false);
    }
}
