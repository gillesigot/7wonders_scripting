using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    // Used to save the parent to which the draggable will return.
    public Transform ParentToReturnTo { get; set; }
    // Used to store the active zoom panel.
    private GameObject ZoomPanel { get; set; }

    /// <summary>
    /// Save this as parent and start drag.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        ParentToReturnTo = this.transform.parent;
        this.transform.SetParent(this.transform.root);
        this.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    /// <summary>
    /// Move the draggable according to pointer.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        this.transform.position = eventData.position;
    }

    /// <summary>
    /// Set parentToReturnTo as current parent and stop drag.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        this.transform.SetParent(ParentToReturnTo);
        this.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    /// <summary>
    /// Set parentToReturnTo as current parent and lock the draggable.
    /// </summary>
    public void StopDraggable()
    {
        this.transform.SetParent(ParentToReturnTo);
        this.GetComponent<CanvasGroup>().blocksRaycasts = true;
        Destroy(this.GetComponent<Draggable>());
    }

    /// <summary>
    /// Zoom in on component when hover
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            this.ZoomOnComponent(true);
    }

    /// <summary>
    /// Zoom out when point is no longer in the component.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
            this.ZoomOnComponent(false);
    }

    /// <summary>
    /// Magnify component on top of UI.
    /// </summary>
    /// <param name="zoomIn">True to zoom in/ False to zoom out.</param>
    private void ZoomOnComponent(bool zoomIn)
    {
        if (zoomIn)
        {
            GameObject zoomPanel = Resources.Load<GameObject>("Zoom");
            ZoomPanel = Instantiate(zoomPanel, this.transform.root);

            RectTransform panel_rt = ZoomPanel.GetComponent<RectTransform>();
            Vector2 size = this.GetComponent<RectTransform>().sizeDelta;
            panel_rt.sizeDelta = new Vector2(size.x, size.y);
            ZoomPanel.transform.position = new Vector3(
                this.transform.position.x,
                this.transform.position.y + size.y / 6
            );

            Image panelImage = ZoomPanel.GetComponent<Image>();
            panelImage.sprite = this.GetComponent<Image>().sprite;
        }
        else
            Destroy(ZoomPanel);
    }
}