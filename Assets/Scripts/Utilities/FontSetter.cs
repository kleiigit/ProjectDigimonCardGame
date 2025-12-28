using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class FontSetter : MonoBehaviour
{
    public string fontClass;
    private void OnEnable()
    {
        OptionsManager.FontUpdated += SetFont;
        SetFont();
    }
    private void OnDisable()
    {
        OptionsManager.FontUpdated -= SetFont;
    }
    private void SetFont()
    {
        TMP_Text textComponent = GetComponent<TMP_Text>();
        if (textComponent && GameManager.Instance.OptionsManager != null)
        {
            textComponent.font = GameManager.Instance.OptionsManager.GetFontClass(fontClass);
        }
    }
}
