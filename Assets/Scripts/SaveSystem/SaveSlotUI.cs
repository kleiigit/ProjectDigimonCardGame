using SinuousProductions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class SaveSlotUI : MonoBehaviour
{
    [Header("Referências UI")]
    public TMP_Text slotNameText;
    public TMP_Text infoText;
    public Button loadButton;
    public Button saveButton;
    public Button deleteButton; // Botão para deletar save
    public TMP_InputField playerNameInput;

    public int slotIndex;

    private void OnEnable()
    {
        SetupSlot(slotIndex);
    }

    public void SetupSlot(int index)
    {
        slotIndex = index;

        SaveData data = SaveManager.Instance.LoadSlotPreview(index);

        slotNameText.text = $"SAVE {index + 1}";

        if (data != null)
        {
            string playerName = string.IsNullOrWhiteSpace(data.playerName) ? "<sem nome>" : data.playerName;
            int discoveredCount = data.GetDiscoveredCardCount();

            infoText.text = $"{playerName} - {discoveredCount} carta(s)";
            loadButton.interactable = true;
            saveButton.interactable = true;
            deleteButton.interactable = true; // Ativa botão apagar
        }
        else
        {
            infoText.text = "<vazio>";
            loadButton.interactable = false;
            saveButton.interactable = true;
            deleteButton.interactable = false;
        }
    }

    public void OnLoadPressed()
    {
        string name = playerNameInput != null ? playerNameInput.text : "Jogador";
        SaveManager.Instance.LoadOrCreateSlot(slotIndex, name);
        SetupSlot(slotIndex);
    }

    public void OnSavePressed()
    {
        if (!SaveManager.Instance.SlotExists(slotIndex))
        {
            SaveManager.Instance.NewGame(slotIndex, $"Jogador_{slotIndex + 1}");
        }

        SaveManager.Instance.SaveToSlotFromButton(slotIndex);
        SetupSlot(slotIndex);
    }

    public void OnDeletePressed()
    {
        string path = Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex}.json");

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveSlotUI] Save do slot {slotIndex} deletado.");
        }

        SetupSlot(slotIndex);
    }
}
