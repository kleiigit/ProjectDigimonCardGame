using SinuousProductions;
using UnityEngine;

public class ActionPanelScript : MonoBehaviour
{
    public static ActionPanelScript Instance;

    [SerializeField] GameObject cardPanel;
    [SerializeField] GameObject actionPanel;
    [SerializeField] GameObject buttonPlace;

    private CardDisplay cardDisplay;


    public void ActivePanel(GameObject cardObject)
    {
        actionPanel.SetActive(true);
        cardDisplay = cardObject.GetComponent<CardDisplay>();
    }
}
