﻿using UnityEngine;
using UnityEngine.UI;

public class DropController
{
    // Used to define existing drop zone functions.
    public enum Function
    {
        REGULAR_BUILD,
        WONDER_BUILD,
        DISCARD,
    };
    // The location of the related drop zone.
    public Transform DropZone { get; set; }
    // The location of the game discard pile in the current game.
    public Transform DiscardPile { get; set; }
    // The human player interacting with the drop zone.
    public Player Player { get; set; }
    // Define the name of the resource to use for displaying the card back depending on the current age.
    private const string CARD_BACK_1 = "card_back_I";
    private const string CARD_BACK_2 = "card_back_II";
    private const string CARD_BACK_3 = "card_back_III";

    public DropController(Transform dropZone, Transform discardPile)
    {
        this.DropZone = dropZone;
        this.DiscardPile = discardPile;
        this.Player = GameManager.Instance().GetHumanPlayer();
    }

    /// <summary>
    /// Define which action to execute when a card has been dropped according to the drop zone function.
    /// </summary>
    /// <param name="function">The drop zone function.</param>
    /// <param name="card">The card that has been dropped.</param>
    /// <returns>The new card location.</returns>
    public Transform HasDropped(Function function, GameObject card)
    {
        Transform newCardParent = null;
        switch (function)
        {
            case Function.REGULAR_BUILD:
                newCardParent = this.RegularBuild(card);
                break;
            case Function.WONDER_BUILD:
                newCardParent = this.WonderBuild(card);
                break;
            case Function.DISCARD:
                newCardParent = this.Discard(card);
                break;
        }
        Playable cardInfo = card.GetComponent<Playable>();
        GameManager.Instance().EndTurn(cardInfo.id);
        PlayerBoardController.RefreshHand();
        if (this.Player.Hand.Count == 1)
            PlayerBoardController.DiscardLastCard();

        return newCardParent;
    }

    /// <summary>
    /// Sort the card according to its type and define its new location accordingly.
    /// </summary>
    /// <param name="card">The card to sort.</param>
    /// <returns>The new card location.</returns>
    private Transform RegularBuild(GameObject card)
    {
        Playable playable = card.GetComponent<Playable>();
        Transform newParent = GameObject.Find("build_zones").transform.GetChild((int)playable.buildType);

        if (playable.buildType == Card.CardType.RESOURCE)
        {
            ShiftLayout layout = newParent.GetComponent<ShiftLayout>();
            newParent = layout.Shift(card);
        }
        return newParent;
    }

    /// <summary>
    /// Put the card under the wonder board and flip it.
    /// </summary>
    /// <param name="card">The card to move under the board.</param>
    /// <returns>The new card location.</returns>
    private Transform WonderBuild(GameObject card)
    {
        Transform childLayout = this.DropZone.parent.GetChild(0);
        Image cardAppearance = card.GetComponent<Image>();
        string cardBackPath = CARD_BACK_1;
        switch (GameManager.Age)
        {
            case 1: cardBackPath = CARD_BACK_1;
                break;
            case 2: cardBackPath = CARD_BACK_2;
                break;
            case 3: cardBackPath = CARD_BACK_3;
                break;
        }
        Sprite cardBack = Resources.Load<Sprite>(cardBackPath);
        cardAppearance.sprite = cardBack;
        return childLayout;
    }

    /// <summary>
    /// Move the card on the discard pile and gets its counterparty in coins.
    /// </summary>
    /// <param name="card">The card to put on the discard pile.</param>
    /// <returns>The new card location.</returns>
    private Transform Discard(GameObject card)
    {
        card.transform.position = this.DiscardPile.position;
        card.SetActive(false);

        Player.City.Discard();
        PlayerBoardController.RefreshCoinAmount();

        return DiscardPile;
    }
}
