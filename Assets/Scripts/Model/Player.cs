using System.Collections.Generic;

public class Player
{
    // The player's name.
    public string Name { get; set; }
    // Used to define the starting amount of coins of all players.
    private const int STARTING_COINS_AMOUNT = 3;
    // Amount of coins owned by the player
    public int Coins { get; set; }
    // Amount of victory points won by the players by doing war.
    public int VictoryWarPoints { get; set; }
    // Amount of defeats suffered by the players by failing in eastern war.
    public int EastDefeatWarTokens { get; set; }
    // Amount of defeats suffered by the players by failing in western war.
    public int WestDefeatWarTokens { get; set; }
    // True if the player is human, false for AI.
    public bool IsHuman { get; set; }
    // Represent the city of a player.
    public CityManager City { get; set; }
    // Represent the wonder manager of a player.
    public WonderManager WonderManager { get; set; }
    // Represent the cards in player's hand.
    public List<Card> Hand { get; set; }
    // Player's way of playing (if not human).
    public AIManager AI { get; set; }

    public Player(
        bool isHuman = false,
        int coins = STARTING_COINS_AMOUNT, 
        int victoryWarPoints = 0,
        int eastWarTokens = 0,
        int westWarTokens = 0
        )
    {
        this.IsHuman = isHuman;
        this.Coins = coins;
        this.VictoryWarPoints = victoryWarPoints;
        this.EastDefeatWarTokens = eastWarTokens;
        this.WestDefeatWarTokens = westWarTokens;
        this.Hand = new List<Card>();
    }
}
