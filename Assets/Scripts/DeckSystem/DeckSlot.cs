using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SinuousProductions
{
    public class DeckSlot : MonoBehaviour
    {
        [SerializeField] private Button editButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button viewButton;
        [SerializeField] private Button copyButton;

        [SerializeField] private int deckIndex;
        public TextMeshProUGUI deckNameText;
        private DeckEditorUI deckEditorUI;

        public void Setup(int index, DeckEditorUI editor)
        {
            deckIndex = index;
            deckEditorUI = editor;

            if (editButton != null)
                editButton.onClick.AddListener(() => deckEditorUI.OnEditDeck(deckIndex));

            if (deleteButton != null)
                deleteButton.onClick.AddListener(() => deckEditorUI.OnDeleteDeck(deckIndex));

            if (viewButton != null)
                viewButton.onClick.AddListener(() => deckEditorUI.OnViewDeck(deckIndex));
            if (copyButton != null)
                copyButton.onClick.AddListener(() => deckEditorUI.OnCopyDeck(deckIndex));
        }
        public void SetCopyButtonInteractable(bool state)
        {
            if (copyButton != null)
                copyButton.interactable = state;
        }
    }
}
