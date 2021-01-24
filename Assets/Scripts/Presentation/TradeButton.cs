using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static PlayerBoardController;

public class TradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public CityManager.TradeLocation location;
    public GameObject tradePanel;
    public List<Text> resLabels = new List<Text>();
    public Text costLabel;

    // Plug trade panel UI.
    public void Start()
    { 
        Text totalLabel = this.tradePanel.GetComponentsInChildren<Text>()
            .Select(o => o)
            .Where(o => o.name == "total")
            .First();

        foreach (Button b in this.tradePanel.GetComponentsInChildren<Button>())
        {
            b.onClick.RemoveAllListeners();
            if (b.name == "ok" || b.name == "cancel")
            {
                b.onClick.AddListener(delegate { CloseTradePanel(tradePanel, b.name == "ok", this.resLabels); });
                this.costLabel = b.transform.parent.GetComponentInChildren<Text>();
            }
            else
            {
                Text amount = b.transform.parent.GetComponentInChildren<Text>();
                int valueToAdd = (b.name == "add") ? 1 : -1;
                b.onClick.AddListener(delegate 
                { 
                    UpdateTradeResourceAmount(amount, valueToAdd, totalLabel, this.resLabels); 
                });
                if (b.name == "add")  // Add label to list, only once
                    this.resLabels.Add(amount);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        this.tradePanel.GetComponentInChildren<Text>().text = location + " TRADE";
        foreach (Text label in this.resLabels)
            label.text = label.text.Split(' ')[0] + " 0";
        this.costLabel.text = this.costLabel.text.Split(' ')[0] + " 0";

        InitTradePanel(this.tradePanel, location, this.resLabels);
    }
}
