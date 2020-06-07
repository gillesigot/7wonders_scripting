using System;
using System.Collections.Generic;
using System.Linq;

public class GameManager
{
    // Unique instance of the GameManager.
    private static GameManager _instance;
    // Used to tell the amount of players that will play in the current game.
    public static int NbPlayers { get; set; }
    // Used to gather the list of the player of the current game.
    public List<Player> Players { get; set; }
    // Used to determine which age of the game is currently played.
    public static int Age { get; set; }

    // Raised when invalid game settings are defined.
    public class WrongGameSettingsException : Exception
    {
        public WrongGameSettingsException(string message) : base(message) { }
    }

    private GameManager() 
    {
        if ((NbPlayers < GameConsts.MIN_PLAYERS) || (NbPlayers > GameConsts.MAX_PLAYERS))
            throw new WrongGameSettingsException("Wrong number of players: " + NbPlayers);

        this.Players = new List<Player>();
        for (int i = 0; i < NbPlayers; i++)
        {
            Player player = new Player();
            player.City = new CityManager(player);
            player.Wonder = new WonderManager(player);
            if (i == 0) player.IsHuman = true;
            Players.Add(player);
        }
        Age = 1;
    }
    /// <summary>
    /// Get unique instance of Game manager (create it of not created yet).
    /// </summary>
    /// <returns>The GameManager instance.</returns>
    public static GameManager Instance()
    {
        if (_instance == null)
        {
            _instance = new GameManager();
        }
        return _instance;
    }
    /// <summary>
    /// Retrieve the human player out of the players list.
    /// </summary>
    /// <returns>The human player as Player.</returns>
    public Player GetHumanPlayer()
    {
        for (int i = 0; i < Players.Count; i++)
            if (Players[i].IsHuman)
                return Players[i];
        return null;
    }

    /// <summary>
    /// Distribute appropriate cards to all players.
    /// </summary>
    public void DistributeCards()
    {
        List<Card> availableCards = CardsDAO.GetCards(NbPlayers, Age);
        // TODO randomize guilds (before giving cards, shuffle them and remove extra ones)
        Random rand = new Random();

        foreach (Player player in this.Players)
        {
            for (int i = 0; i < GameConsts.STARTING_CARDS_NUMBER; i++)
            {
                Card nextCard = availableCards.ElementAt(rand.Next(availableCards.Count));
                player.Hand.Add(nextCard);
                availableCards.Remove(nextCard);
            }
        }
    }
    // TODO ends of turn: -> give player's hand to player on the left/right
}
