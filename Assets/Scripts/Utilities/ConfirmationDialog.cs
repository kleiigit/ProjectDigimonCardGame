using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SinuousProductions
{
    public class ConfirmationDialog : MonoBehaviour
    {
        public enum DialogMode
        {
            ConfirmOnly,
            SelectOption
        }

        [Header("UI Elements")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private Transform tabButtonContainer;
        [SerializeField] private GameObject tabButtonPrefab;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        // ConfirmOnly mode
        private Action<bool> confirmCallback;

        // SelectOption mode
        private DialogMode currentMode;
        private int selectedIndex = -1;

        private List<string> optionDescriptions = new List<string>();
        private List<object> optionData = new List<object>();
        private Action<object> optionCallback;

        private void Awake()
        {
            dialogPanel.SetActive(false);

            confirmButton.onClick.AddListener(OnConfirmClick);
            cancelButton.onClick.AddListener(OnCancelClick);
        }

        #region ConfirmOnly Mode

        public void Show(string message, Action<bool> onResult)
        {
            dialogPanel.SetActive(true);
            currentMode = DialogMode.ConfirmOnly;

            tabButtonContainer.gameObject.SetActive(false);
            descriptionText.text = message;

            confirmCallback = onResult;
            confirmButton.interactable = true;
        }

        #endregion

        #region SelectOption Mode

        public void ShowOptionSelection<T>(List<T> options, Func<T, string> getDescription, Action<T> onSelected)
        {
            dialogPanel.SetActive(true);
            currentMode = DialogMode.SelectOption;

            selectedIndex = -1;
            confirmButton.interactable = false;

            optionData.Clear();
            optionDescriptions.Clear();
            tabButtonContainer.gameObject.SetActive(true);

            // Fill internal lists
            foreach (T option in options)
            {
                optionData.Add(option);
                optionDescriptions.Add(getDescription(option));
            }

            optionCallback = obj => onSelected((T)obj);
            descriptionText.text = "Select one of the options:";

            // Clear previous buttons
            foreach (Transform child in tabButtonContainer)
                Destroy(child.gameObject);

            // Create new buttons
            for (int i = 0; i < optionDescriptions.Count; i++)
            {
                int index = i;
                GameObject tab = Instantiate(tabButtonPrefab, tabButtonContainer);
                tab.GetComponentInChildren<TextMeshProUGUI>().text = $"Option {i + 1}";
                tab.GetComponent<Button>().onClick.AddListener(() => SelectTab(index));
            }
        }

        private void SelectTab(int index)
        {
            selectedIndex = index;
            descriptionText.text = optionDescriptions[index];
            confirmButton.interactable = true;
        }

        #endregion

        #region Confirm / Cancel Buttons

        private void OnConfirmClick()
        {
            dialogPanel.SetActive(false);

            if (currentMode == DialogMode.ConfirmOnly)
            {
                confirmCallback?.Invoke(true);
            }
            else if (currentMode == DialogMode.SelectOption)
            {
                if (selectedIndex >= 0 && selectedIndex < optionData.Count)
                    optionCallback?.Invoke(optionData[selectedIndex]);
            }

            ResetState();
        }

        private void OnCancelClick()
        {
            dialogPanel.SetActive(false);

            if (currentMode == DialogMode.ConfirmOnly)
            {
                confirmCallback?.Invoke(false);
            }
            else if (currentMode == DialogMode.SelectOption)
            {
                optionCallback?.Invoke(null);
            }

            ResetState();
        }

        private void ResetState()
        {
            descriptionText.text = "";
            tabButtonContainer.gameObject.SetActive(false);
            confirmButton.interactable = true;
            confirmCallback = null;
            optionCallback = null;
            selectedIndex = -1;

            foreach (Transform child in tabButtonContainer)
                Destroy(child.gameObject);
        }

        #endregion
    }
}
