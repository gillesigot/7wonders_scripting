using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static DropController;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    // Used to define default action for the drop zone.
    public Function ZoneType;
    // Used to represent the location of the discard pile.
    private Transform PileObject { get; set; }
    // Used to attach the related controller to this drop zone.
    private DropController DropController { get; set; }

    /// <summary>
    /// Initialize class attributes.
    /// </summary>
    void Start() 
    {
        this.PileObject = GameObject.Find("discard_pile").transform;
        this.DropController = new DropController(this.transform, PileObject);
    }

    /// <summary>
    /// Light/magnify drop zone when pointer enter.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            if (this.ZoneType == Function.REGULAR_BUILD)
                this.LightDropZone(0.70f);
            else
                this.MagnifyDropZone(1.05f);
        }
    }

    /// <summary>
    /// Darken/reduce drop zone when point leave.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (this.ZoneType == Function.REGULAR_BUILD)
            this.LightDropZone(0f);
        else
            this.MagnifyDropZone(1.00f);
    }

    /// <summary>
    /// Darken/reduce drop zone and execute default action.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        GameObject card = eventData.pointerDrag;
        Transform parent = this.DropController.HasDropped(this.ZoneType, card);
        this.StopDragging(card, parent);

        if (this.ZoneType == Function.REGULAR_BUILD)
            this.LightDropZone(0f);
        else 
            this.MagnifyDropZone(1.00f);
    }

    /// <summary>
    /// Scale up/down drop zone.
    /// </summary>
    /// <param name="coeff">The coefficient to apply.</param>
    private void MagnifyDropZone(float coeff)
    {
        this.transform.localScale = new Vector3(coeff, coeff, coeff);
    }

    /// <summary>
    /// Set the drop zone lighter/darker.
    /// </summary>
    /// <param name="lighter">The alpha to apply to drop zone.</param>
    private void LightDropZone(float alpha)
    {
        Image dropZoneAppearance = this.GetComponent<Image>();
        Color color = dropZoneAppearance.color;
        color.a = alpha;
        dropZoneAppearance.color = color;
    }

    /// <summary>
    /// Lock a draggable to a given object.
    /// </summary>
    /// <param name="card">The draggable card to lock.</param>
    /// <param name="parent">The object to attach the card to.</param>
    private void StopDragging(GameObject card, Transform parent)
    {
        Draggable drag = card.GetComponent<Draggable>();
        if (drag != null)
        {
            if (parent != null)
            {
                drag.ParentToReturnTo = parent;
            }
            drag.StopDraggable();
        }
    }
}
