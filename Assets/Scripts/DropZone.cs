using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    // Used to define existing drop zone functions.
    public enum Function
    {
        REGULAR_BUILD = 0,
        WONDER_BUILD = 1,
        DESTROY = 2,
    };
    // Used to define the drop zone default action.
    public Function zoneType = Function.REGULAR_BUILD;

    /// <summary>
    /// Light/magnify drop zone when pointer enter.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            if (zoneType == Function.REGULAR_BUILD)
            {
                this.LightDropZone(true);
            }
            else
            {
                this.MagnifyDropZone(1.05f);
            }
        }
    }

    /// <summary>
    /// Darken/reduce drop zone when point leave.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (zoneType == Function.REGULAR_BUILD)
        {
            this.LightDropZone(false);
        }
        else
        {
            this.MagnifyDropZone(1.00f);
        }
    }

    /// <summary>
    /// Darken/reduce drop zone and execute default action.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        GameObject card = eventData.pointerDrag;
        switch (this.zoneType)
        {
            case Function.REGULAR_BUILD:
                this.RegularBuild(card);
                this.LightDropZone(false);
                break;
            case Function.WONDER_BUILD:
                this.WonderBuild(card);
                this.MagnifyDropZone(1.00f);
                break;
            case Function.DESTROY:
                this.Destroy(card);
                this.MagnifyDropZone(1.00f);
                break;
        }
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
    /// <param name="lighter">True to light the zone.</param>
    private void LightDropZone(bool lighter)
    {
        Image dropZoneAppearance = this.GetComponent<Image>();
        Color color = dropZoneAppearance.color;
        if (lighter)
        {
            color.a = 0.70f;
        }
        else 
        {
            color.a = 0f;
        }
        dropZoneAppearance.color = color;
    }

    /// <summary>
    /// Lock a draggable to a given object.
    /// </summary>
    /// <param name="card">The draggable card to lock.</param>
    /// <param name="card_parent">The object to attach the card to.</param>
    private void StopDragging(GameObject card, Transform card_parent)
    {
        Draggable drag = card.GetComponent<Draggable>();
        if (drag != null)
        {
            if (card_parent != null)
            {
                drag.parentToReturnTo = card_parent;
            }
            drag.StopDraggable();
        }
    }

    /// <summary>
    /// Lock the card and build it.
    /// </summary>
    /// <param name="card">The card to build.</param>
    private void RegularBuild(GameObject card)
    {
        Buildable buildable = card.GetComponent<Buildable>();
        if (buildable != null && buildable.Build())
        {
            this.StopDragging(card, null);
            if (buildable.buildType == Buildable.BuildType.RESOURCE)
            {
                Transform buildZone = buildable.GetBuildZone();
                ShiftLayout layout = buildZone.GetComponent<ShiftLayout>();
                layout.Shift(card);
            }
        }
    }

    /// <summary>
    /// Lock the card and move it under the wonder board.
    /// </summary>
    /// <param name="card">The card to assign to wonder build.</param>
    private void WonderBuild(GameObject card)
    {
        // TODO move part of this to a wonder game object (separate game logic)
        Transform childLayout = this.transform.parent.GetChild(0);
        this.StopDragging(card, childLayout);
        Image cardAppearance = card.GetComponent<Image>();
        Sprite cardBack = Resources.Load<Sprite>("card_back");
        cardAppearance.sprite = cardBack;
    }

    /// <summary>
    /// Lock the card and put it in the discard pile.
    /// </summary>
    /// <param name="card">The card to discard.</param>
    private void Destroy(GameObject card)
    {
        Transform pileObject = GameObject.Find("discard_pile").transform;
        DiscardPile discardPile = pileObject.GetComponent<DiscardPile>();
        discardPile.Discard(card);
        this.StopDragging(card, pileObject);
        // TODO Separate adding card to gameobject discard_pile and the logic of putting this card on the discard pile
        // TODO test: fire an event to tell 1 card has been added to the discard pile
    }
}
