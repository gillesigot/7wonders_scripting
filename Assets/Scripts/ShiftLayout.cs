using UnityEngine;

public class ShiftLayout : MonoBehaviour
{
    // Offset used to shift each items.
    public int shift = 17;
    // Tag used to identify shifted items.
    private const string SHIFTED_TAG = "ShiftedItem";

    /// <summary>
    /// Wrap items in panels and resize them to shift items to the left.
    /// </summary>
    /// <param name="item">The new item added in the layout.</param>
    public void Shift(GameObject item)
    {
        // Shift tagged parents.
        Transform parent = this.transform.parent;
        RectTransform[] items = parent.GetComponentsInChildren<RectTransform>();
        foreach (RectTransform parentItem in items)
        {
            if (parentItem.CompareTag(SHIFTED_TAG))
            {
                parentItem.sizeDelta = new Vector2(
                    parentItem.sizeDelta.x + shift,
                    parentItem.sizeDelta.y
                );
            }
        }

        // Wrap current in a panel and tag it.
        GameObject panel = new GameObject();
        panel.AddComponent<RectTransform>();

        RectTransform panel_rt = panel.GetComponent<RectTransform>();
        RectTransform item_rt = item.GetComponent<RectTransform>();
        panel_rt.sizeDelta = new Vector2(item_rt.sizeDelta.x, item_rt.sizeDelta.y);

        panel.transform.position = item.transform.position;
        panel.transform.SetParent(item.transform.parent);
        panel.tag = SHIFTED_TAG;
        item.transform.SetParent(panel.transform);
    }
}
