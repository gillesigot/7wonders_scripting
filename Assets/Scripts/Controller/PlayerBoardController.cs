using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
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

    public PlayerBoardController()
    {
        Player = GameManager.Instance().GetHumanPlayer();
        Coins = GameObject.Find("coin_text").GetComponent<Text>();
        WarPoints = GameObject.Find("war_text").GetComponent<Text>();
        CardFactory = GameObject.Find("hand").GetComponent<CardFactory>();
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
}
