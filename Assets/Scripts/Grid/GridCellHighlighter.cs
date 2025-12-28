using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GridCellHighlighter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Color highlighColor = Color.yellow;
    public Color posColor = Color.green;
    public Color negColor = Color.red;


    private Color originalColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void OnMouseEnter()
    {
        
        spriteRenderer.color = highlighColor;
    }
    void OnMouseExit()
    {
        spriteRenderer.color = originalColor;
    }
}
