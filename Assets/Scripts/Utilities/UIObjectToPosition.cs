using System;
using UnityEngine;

public class UIObjectToPosition : MonoBehaviour
{
    public RectTransform objectToPosition;
    public int widthDivider = 2;     // Divisor da largura
    public int heightDivider = 2;    // Divisor da altura
    public float widthMultiplier = 1f;   // Multiplicador da largura
    public float heightMultiplier = 1f;  // Multiplicador da altura

    public bool updatePosition = false;

    void Start()
    {
        SetUIObjectPosition();
    }
    void Update()
    {
        if(updatePosition)
        {
            SetUIObjectPosition();
        }
    }

    private void SetUIObjectPosition()
    {
        if(objectToPosition != null && widthDivider != 0 && heightDivider != 0)
        {
            float anchorX = widthMultiplier / widthDivider;
            float anchorY = heightMultiplier / heightDivider;

            objectToPosition.anchorMin = new Vector2(anchorX, anchorY);
            objectToPosition.anchorMax = new Vector2(anchorX, anchorY);
            objectToPosition.pivot = new Vector2(0.5f, 0.5f);

            objectToPosition.anchoredPosition = Vector2.zero;
        }
    }
}
