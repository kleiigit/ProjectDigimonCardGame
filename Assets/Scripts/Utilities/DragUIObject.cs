using UnityEngine;
using UnityEngine.EventSystems;

public class DragUIObject : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalLocalPointerPosition;
    private Vector3 originalPanelLocalPosition;
    public float movementSensitivity = 1.0f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Apenas processa se o botão for o esquerdo
        if (eventData.button != PointerEventData.InputButton.Left) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out originalLocalPointerPosition);

        originalPanelLocalPosition = rectTransform.localPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Apenas processa se o botão for o esquerdo
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPointerPosition))
        {
            localPointerPosition /= canvas.scaleFactor;

            Vector3 offsetToOriginal = (localPointerPosition - originalLocalPointerPosition) * movementSensitivity;
            rectTransform.localPosition = originalPanelLocalPosition + offsetToOriginal;
        }
    }
}
