using ProjectScript.Interfaces;
using ProjectScript.Selection;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    private SelectionRequest currentRequest;
    private readonly List<ISelectable> selected = new();

    public bool IsSelecting => currentRequest != null;
    public IReadOnlyList<ISelectable> CurrentSelection => selected;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartSelection(SelectionRequest request)
    {
        currentRequest = request;
        ClearSelection();
    }

    public void TrySelect(ISelectable selectable)
    {
        if (currentRequest == null || selectable == null)
            return;

        if (selected.Contains(selectable))
        {
            selected.Remove(selectable);
            selectable.OnDeselected();
            return;
        }

        if (selected.Count >= currentRequest.amount)
            return;

        selected.Add(selectable);
        selectable.OnSelected();
    }

    public void ConfirmSelection()
    {
        if (currentRequest == null)
            return;

        currentRequest.onComplete?.Invoke(new List<ISelectable>(selected));

        currentRequest = null;
        ClearSelection();
    }

    public void CancelSelection()
    {
        ClearSelection();
        currentRequest = null;
    }

    private void ClearSelection()
    {
        foreach (var selectable in selected)
        {
            selectable.OnDeselected();
        }

        selected.Clear();
    }

    // Utilitário opcional
    public void DeselectAllByTag(string tag)
    {
        var objects = GameObject.FindGameObjectsWithTag(tag);

        foreach (var obj in objects)
        {
            if (obj.TryGetComponent<ISelectable>(out var selectable))
            {
                selectable.OnDeselected();
            }
        }
    }
}
