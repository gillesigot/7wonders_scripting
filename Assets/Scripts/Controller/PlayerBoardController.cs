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

    public PlayerBoardController()
    {
        Player = GameManager.Instance().getHumanPlayer();
        Coins = GameObject.Find("coin_text").GetComponent<Text>();
        WarPoints = GameObject.Find("war_text").GetComponent<Text>();
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
}
