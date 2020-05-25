using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Used to save the parent to which the draggable will return.
    public Transform parentToReturnTo = null;
    // Used to lock a draggable item.
    private bool is_draggable = true;

    /// <summary>
    /// Save this as parent and start drag.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (is_draggable)
        {
            parentToReturnTo = this.transform.parent;
            this.transform.SetParent(this.transform.root);
            this.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Move the draggable according to pointer.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (is_draggable)
        {
            this.transform.position = eventData.position;
        }
    }

    /// <summary>
    /// Set parentToReturnTo as current parent and stop drag.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (is_draggable)
        {
            this.transform.SetParent(parentToReturnTo);
            this.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }

    /// <summary>
    /// Set parentToReturnTo as current parent and lock the draggable.
    /// </summary>
    public void StopDraggable()
    {
        this.is_draggable = false;
        this.transform.SetParent(parentToReturnTo);
        this.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

}
