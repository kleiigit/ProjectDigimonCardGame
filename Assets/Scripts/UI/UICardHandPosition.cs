using UnityEngine;

public class UICardHandPosition : MonoBehaviour
{
    [Header("Configuração de Layout")]
    public float spacing = 120f;
    public bool updatePosition = false;

    void Start()
    {
        OrganizeChildren();
    }

    void Update()
    {
        if (updatePosition)
        {
            OrganizeChildren();
        }
    }

    private void OrganizeChildren()
    {
        int count = transform.childCount;
        if (count == 0)
            return;

        float totalWidth = (count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child == null)
                continue;

            child.anchorMin = new Vector2(0.5f, 0.5f);
            child.anchorMax = new Vector2(0.5f, 0.5f);
            child.pivot = new Vector2(0.5f, 0.5f);

            child.anchoredPosition = new Vector2(startX + i * spacing, 0f);
        }
    }
}

