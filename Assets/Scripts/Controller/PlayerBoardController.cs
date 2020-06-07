using UnityEngine;
using UnityEngine.UI;
public class PlayerBoardController
{
    // Used to represent the human player interacting with the board.
    public static Player Player { get; set; }
    // Used to store the UI component that display the coins amount.
    public static Text Coins { get; set; }
    // Used to store the UI component that display the war points.
    public static Text WarPoints { get; set; }
    // Used to store the player's cards representation.
    public static CardFactory CardFactory { get; set; }
    // Used to store left discard pile representation.
    public static Image LeftDiscardPile { get; set; }
    // Used to store right discard pile representation.
    public static Image RightDiscardPile { get; set; }

    public PlayerBoardController()
    {
        Player = GameManager.Instance().GetHumanPlayer();
        Coins = GameObject.Find("coin_text").GetComponent<Text>();
        WarPoints = GameObject.Find("war_text").GetComponent<Text>();
        CardFactory = GameObject.Find("hand").GetComponent<CardFactory>();
        LeftDiscardPile = GameObject.Find("discard_left").GetComponent<Image>();
        RightDiscardPile = GameObject.Find("discard_right").GetComponent<Image>();
    }

    /// <summary>
    /// Refresh the coin amount on the player board.
    /// </summary>
    public static void RefreshCoinAmount()
    {
        Coins.text = Player.Coins.ToString();
    }

    /// <summary>
    /// Refresh the war points count on the player board.
    /// </summary>
    public static void RefreshWarPoints()
    {
        WarPoints.text = Player.VictoryWarPoints.ToString();
    }

    /// <summary>
    /// Refresh the cards display in player's hand.
    /// </summary>
    public static void RefreshHand()
    {
        foreach (Card card in Player.Hand)
            CardFactory.CreateCard(card);
    }

    /// <summary>
    /// Display the discard piles according to the current age.
    /// </summary>
    /// <param name="age">The current age of the game.</param>
    public static void RefreshDiscardPiles(int age)
    {
        LeftDiscardPile.sprite = Resources.Load<Sprite>("discard_pile_" + age);
        RightDiscardPile.sprite = Resources.Load<Sprite>("discard_pile_" + age);
    }
}
