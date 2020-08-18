public static class GameConsts
{
    // Define the minimum of players allowed in a game.
    public const int MIN_PLAYERS = 3;
    // Define the maximum of players allowed in a game.
    public const int MAX_PLAYERS = 7;
    // Define the coins value of a discarded card.
    public const int DISCARDED_CARD_VALUE = 3;
    // Define the number of cards a player has when starting an age.
    public const int STARTING_CARDS_NUMBER = 7;
    // Define the amount of victory points you get when winning a war, depending on the age.
    public static readonly int[] WAR_VICTORY_POINTS = new int[] {1, 3, 5};
    // Define the amount of victory points you get when losing a war.
    public const int WAR_DEFEAT_POINTS = -1;
}
