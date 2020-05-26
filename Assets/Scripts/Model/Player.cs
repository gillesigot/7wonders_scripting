public class Player
{
    // Used to define the starting amount of coins of all players.
    private const int STARTING_COINS_AMOUNT = 3;
    // Used to define the starting amount of war points of all players.
    private const int STARTING_WAR_POINTS = 0;
    // Amount of coins owned by the player
    public int Coins { get; set; }
    // Amount of victory points won by the players by doing war.
    public int VictoryWarPoints { get; set; }
    // True if the player is human, false for AI.
    public bool IsHuman { get; set; }
    // Represent the city of a player.
    public CityManager City { get; set; }
    // Represent the wonder of a player.
    public WonderManager Wonder { get; set; }

    public Player(
        bool isHuman = false,
        int coins = STARTING_COINS_AMOUNT, 
        int victoryWarPoints = STARTING_WAR_POINTS
        )
    {
        this.IsHuman = isHuman;
        this.Coins = coins;
        this.VictoryWarPoints = victoryWarPoints;
    }
}
