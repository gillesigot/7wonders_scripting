using UnityEngine;
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
    public GameController GameController { get; set; }
    // Define the name of the resource to use for displaying the card back depending on the current age.
    private const string CARD_BACK_1 = "card_back_I";
    private const string CARD_BACK_2 = "card_back_II";
    private const string CARD_BACK_3 = "card_back_III";

    public DropController(Transform dropZone, Transform discardPile, GameController gc)
    {
        this.DropZone = dropZone;
        this.DiscardPile = discardPile;
        this.Player = GameManager.Instance().GetHumanPlayer();
        this.GameController = gc;
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
        if (newCardParent != null)
            this.EndTurn();

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
        if (Player.City.Build(playable.id))
        {
            PlayerBoardController.RefreshCoinAmount();
            Transform newParent = GameObject.Find("build_zones").transform.GetChild((int)playable.buildType);

            if (playable.buildType == Card.CardType.RESOURCE)
            {
                ShiftLayout layout = newParent.GetComponent<ShiftLayout>();
                newParent = layout.Shift(card);
            }
            return newParent;
        }
        else
        {
            Debug.Log("Build impossible: build conditions are not met.");
            return null;
        }
    }

    /// <summary>
    /// Put the card under the wonder board and flip it.
    /// </summary>
    /// <param name="card">The card to move under the board.</param>
    /// <returns>The new card location.</returns>
    private Transform WonderBuild(GameObject card)
    {
        if (Player.WonderManager.IsWonderBuilt())
            return null;

        Playable playable = card.GetComponent<Playable>();
        int actionToPerform = Player.WonderManager.BuildWonder(playable.id);

        if (actionToPerform >= 0)
        {
            switch (actionToPerform)
            {
                case 1: PlayerBoardController.RefreshCoinAmount();
                    break;
            }

            Transform childLayout = this.DropZone.parent.GetChild(0);
            Image cardAppearance = card.GetComponent<Image>();
            string cardBackPath = CARD_BACK_1;
            switch (GameManager.Age)
            {
                case 1:
                    cardBackPath = CARD_BACK_1;
                    break;
                case 2:
                    cardBackPath = CARD_BACK_2;
                    break;
                case 3:
                    cardBackPath = CARD_BACK_3;
                    break;
            }
            Sprite cardBack = Resources.Load<Sprite>(cardBackPath);
            cardAppearance.sprite = cardBack;
            return childLayout;
        }
        else
        {
            Debug.Log("Wonder build impossible: build conditions are not met.");
            return null;
        }
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

        Playable playable = card.GetComponent<Playable>();
        Player.City.Discard(playable.id);
        PlayerBoardController.RefreshCoinAmount();

        return DiscardPile;
    }

    /// <summary>
    /// Rotate hands & discard last card if applicable.
    /// </summary>
    private void EndTurn()
    {
        if (this.Player.Hand.Count == 1)
            PlayerBoardController.DiscardLastCard();

        GameManager.Instance().EndTurn();
        PlayerBoardController.RefreshHand();
        PlayerBoardController.CleanTradeBoards();

        if (this.Player.Hand.Count == 0)
        {
            this.GameController.ResolveConflicts(GameManager.Age);
            GameManager.Age++;
            this.GameController.StartAge(GameManager.Age);
        }
    }
}
