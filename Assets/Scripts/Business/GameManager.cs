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
    /// Get the city on the left of the current player's city.
    /// </summary>
    /// <param name="players">The list of all players.</param>
    /// <param name="currentPlayer">The position of the current player in players list.</param>
    /// <returns></returns>
    private CityManager GetLeftCity(Player[] players, int currentPlayer)
    {
        int idxLeftCity = currentPlayer - 1 < 0 ? NbPlayers - 1 : currentPlayer - 1;
        return players[idxLeftCity].City;
    }

    /// <summary>
    /// Get the city on the right of the current player's city.
    /// </summary>
    /// <param name="players">The list of all players.</param>
    /// <param name="currentPlayer">The position of the current player in players list.</param>
    /// <returns></returns>
    private CityManager GetRightCity(Player[] players, int currentPlayer)
    {
        int idxRightCity = currentPlayer + 1 == NbPlayers ? 0 : currentPlayer + 1;
        return players[idxRightCity].City;
    }

    /// <summary>
    /// Distribute appropriate cards to all players.
    /// </summary>
    public void DistributeCards()
    {
        List<Card> availableCards = CardsDAO.GetCards(NbPlayers, Age);

        if (Age == 3)
            availableCards = this.RemoveExtraGuildCards(availableCards);

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

    /// <summary>
    /// Remove randomly extra guilds to fit number of players.
    /// </summary>
    /// <param name="cards">All cards from Age 3.</param>
    /// <returns>All cards except the extra guilds.</returns>
    private List<Card> RemoveExtraGuildCards(List<Card> cards)
    {
        List<Card> guilds = new List<Card>();
        foreach (Card card in cards.ToList())
            if (card.Type == Card.CardType.GUILD)
                guilds.Add(card);

        int numberOfAllowedGuilds = NbPlayers + 2;
        Random rand = new Random();
        for (int i = 0; i < guilds.Count - numberOfAllowedGuilds; i++)
            cards.Remove(guilds.ElementAt(rand.Next(guilds.Count)));
        return cards;
    }

    /// <summary>
    /// Perform all end turn game actions.
    /// </summary>
    public void EndTurn()
    {
        foreach (Player p in Players)
        {
            // Hack: TEMP Mocking IA playing
            if (!p.IsHuman)
                p.Hand.RemoveAt(0);
            if (p.Hand.Count == 1)
                p.Hand = new List<Card>();
            p.City.TradeResources = new Dictionary<Card.ResourceType, int>();
        }
        this.RotateHands();
    }

    /// <summary>
    /// Give the player's hand to the player on his side (according to current age).
    /// </summary>
    public void RotateHands()
    {
        List<Card>[] hands = new List<Card>[NbPlayers];
        for (int i = 0; i < hands.Length; i++)
            hands[i] = Players.ElementAt(i).Hand;

        List<Card>[] shiftedHands = new List<Card>[NbPlayers];
        if (Age == 2)
        {
            Array.Copy(hands, 1, shiftedHands, 0, hands.Length - 1);
            shiftedHands[hands.Length - 1] = hands[0];
        }
        else
        {
            Array.Copy(hands, 0, shiftedHands, 1, hands.Length - 1);
            shiftedHands[0] = hands[hands.Length - 1];
        }

        for (int i = 0; i < hands.Length; i++)
            Players.ElementAt(i).Hand = shiftedHands[i];
    }

    /// <summary>
    /// Compare each player's war score and return victory/defeat matrix.
    /// </summary>
    /// <returns>The war score matrix.</returns>
    public List<int[]> CaculateWarResults()
    {
        const int VICTORY = 1, EQUALITY = 0, DEFEAT = -1;
        List<int[]> warResults = new List<int[]>();
        Player[] players = this.Players.ToArray<Player>();

        for (int i = 0; i < NbPlayers; i++)
        {
            CityManager leftCity = this.GetLeftCity(players, i);
            CityManager rightCity = this.GetRightCity(players, i);
            int[] currentResults = new int[] { EQUALITY, EQUALITY };

            if (leftCity.GetWarPoints() < players[i].City.GetWarPoints())
                currentResults[0] = VICTORY;
            else if (leftCity.GetWarPoints() > players[i].City.GetWarPoints())
                currentResults[0] = DEFEAT;

            if (rightCity.GetWarPoints() < players[i].City.GetWarPoints())
                currentResults[1] = VICTORY;
            else if (rightCity.GetWarPoints() > players[i].City.GetWarPoints())
                currentResults[1] = DEFEAT;

            warResults.Add(currentResults);
        }
        return warResults;
    }

    /// <summary>
    /// Get all players civil score.
    /// </summary>
    /// <returns>List of players civil score.</returns>
    public List<int> GetCivilResults()
    {
        List<int> civilResults = new List<int>();
        foreach (Player p in this.Players)
            civilResults.Add(p.City.GetCivilPoints());
        return civilResults;
    }

    /// <summary>
    /// Get all players science score.
    /// </summary>
    /// <returns>List of players science score.</returns>
    public List<int> GetScienceResults()
    {
        List<int> scienceResults = new List<int>();
        foreach (Player p in this.Players)
            scienceResults.Add(p.City.GetSciencePoints());
        return scienceResults;
    }

    /// <summary>
    /// Get all players bonus score (guilds related).
    /// </summary>
    /// <returns>List of players bonus score.</returns>
    public List<int> GetBonusResults()
    {
        List<int> bonusResults = new List<int>();
        Player[] players = this.Players.ToArray<Player>();

        for (int i = 0; i < NbPlayers; i++)
        {
            CityManager leftCity = this.GetLeftCity(players, i);
            CityManager rightCity = this.GetRightCity(players, i);
            int currentBonus = 0;
            List<BonusCard> guilds = players[i].City.Bonus;
            foreach (BonusCard guild in guilds)
            {
                switch (guild.Bonus)
                {
                    case BonusCard.BonusType.CARD_BONUS:
                        currentBonus += this.CalculateCardBonus(guild, players[i].City, leftCity, rightCity);
                        break;
                    case BonusCard.BonusType.DEFEAT_BONUS:
                        currentBonus += this.CalculateDefeatBonus(players, i);
                        break;
                    case BonusCard.BonusType.WONDER_BONUS:
                        // Hack: TEMP compute wonder bonus
                        break;
                }
            }
            bonusResults.Add(currentBonus);
        }
        return bonusResults;
    }

    /// <summary>
    /// Calculate bonus points based on cards built on current/left/right cities.
    /// </summary>
    /// <param name="guild">The card giving the bonus.</param>
    /// <param name="currentCity">The player's city.</param>
    /// <param name="leftCity">The left player's city.</param>
    /// <param name="rightCity">The right player's city.</param>
    /// <returns></returns>
    private int CalculateCardBonus(BonusCard guild, CityManager currentCity, CityManager leftCity, CityManager rightCity)
    {
        List<Card> cardsToCheck = new List<Card>();
        int bonusPoints = 0;
        
        //if (guild.CheckSelf) TODO uncomment
        cardsToCheck.AddRange(currentCity.GetAllBuildings());
        if (guild.CheckLeft)
            cardsToCheck.AddRange(leftCity.GetAllBuildings());
        if (guild.CheckRight)
            cardsToCheck.AddRange(rightCity.GetAllBuildings());

        foreach (Card c in cardsToCheck)
            foreach (Card.CardType bonusType in guild.BonusCardType)
                if (c.Type == bonusType)
                    bonusPoints += GetBonusPoints(bonusType, c, guild);

        return bonusPoints;
    }

    /// <summary>
    /// Get amount of bonus points according to bonus card type.
    /// </summary>
    /// <param name="bonusCardType">The bonus card type.</param>
    /// <param name="card">The current card being checked.</param>
    /// <param name="guild">The guild card giving the current bonus.</param>
    /// <returns>The amount of points earned.</returns>
    private int GetBonusPoints(Card.CardType bonusCardType, Card card, BonusCard guild)
    {
        int points = 0;
        string[] manufacturedResourcesCardNames = new string[] { "Presse", "Metier a tisser", "Verrerie" };

        if (bonusCardType == Card.CardType.RESOURCE)
        {
            if (guild.ResourceKind == BonusCard.ResourceMetaType.MANUFACTURED)
            {
                if (manufacturedResourcesCardNames.Contains(card.Name))
                    points += 2;
            }
            else if (guild.ResourceKind == BonusCard.ResourceMetaType.RAW)
            {
                if (!manufacturedResourcesCardNames.Contains(card.Name))
                    points++;
            }
            else  // "Guilde des armateurs"
                points++;
        }
        else
            points++;

        return points;
    }

    /// <summary>
    /// Calculate bonus points based on war opponents defeats.
    /// </summary>
    /// <param name="players">List of all players.</param>
    /// <param name="currentPlayerIdx">Position of current player in the list.</param>
    /// <returns></returns>
    private int CalculateDefeatBonus(Player[] players, int currentPlayerIdx)
    {
        int bonusPoints = 0;
        int leftPlayer = currentPlayerIdx - 1 < 0 ? NbPlayers - 1 : currentPlayerIdx - 1;
        int rightPlayer = currentPlayerIdx + 1 == NbPlayers ? 0 : currentPlayerIdx + 1;

        bonusPoints += players[leftPlayer].EastDefeatWarTokens;
        bonusPoints += players[rightPlayer].WestDefeatWarTokens;

        return bonusPoints;
    }
}
