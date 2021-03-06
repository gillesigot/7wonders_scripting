﻿using System;
using System.Collections.Generic;
using System.Linq;
using static Card;

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
    // Used to keep tracks of all cards put on the discard pile.
    public static List<Playable> DiscardPile { get; set; }

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
            player.Name = (i == 0) ? "You" : GameConsts.PLAYERS_NAME[i - 1];
            player.City = new CityManager(player);
            player.WonderManager = new WonderManager(player);
            if (i == 0) player.IsHuman = true;
            Players.Add(player);
        }
        Age = 1;
        DiscardPile = new List<Playable>();
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
    /// <param name="currentPlayer">The current player in players list.</param>
    /// <returns>The city on the left of the current player's city.</returns>
    public CityManager GetLeftCity(Player currentPlayer)
    {
        int idxPlayer = this.Players.IndexOf(currentPlayer);
        int idxLeftCity = idxPlayer - 1 < 0 ? NbPlayers - 1 : idxPlayer - 1;
        return this.Players[idxLeftCity].City;
    }

    /// <summary>
    /// Get the city on the right of the current player's city.
    /// </summary>
    /// <param name="currentPlayer">The current player in players list.</param>
    /// <returns>The city on the right of the current player's city.</returns>
    public CityManager GetRightCity(Player currentPlayer)
    {
        int idxPlayer = this.Players.IndexOf(currentPlayer);
        int idxRightCity = idxPlayer + 1 == NbPlayers ? 0 : idxPlayer + 1;
        return this.Players[idxRightCity].City;
    }

    /// <summary>
    /// Get the player on the left of the current player.
    /// </summary>
    /// <param name="currentPlayer">The current player.</param>
    /// <returns>The player on the left of the current player's city.</returns>
    public Player GetLeftPlayer(Player currentPlayer)
    {
        int idxPlayer = this.Players.IndexOf(currentPlayer);
        int idxLeftPlayer = idxPlayer - 1 < 0 ? NbPlayers - 1 : idxPlayer - 1;
        return this.Players[idxLeftPlayer];
    }

    /// <summary>
    /// Get the player on the right of the current player.
    /// </summary>
    /// <param name="currentPlayer">The current player.</param>
    /// <returns>The player on the right of the current player's city.</returns>
    public Player GetRightPlayer(Player currentPlayer)
    {
        int idxPlayer = this.Players.IndexOf(currentPlayer);
        int idxRightPlayer = idxPlayer + 1 == NbPlayers ? 0 : idxPlayer + 1;
        return this.Players[idxRightPlayer];
    }

    /// <summary>
    /// Get players not human and not direct neighbour of human player.
    /// </summary>
    /// <returns>List of distant players.</returns>
    public Player[] GetDistantPlayers()
    {
        List<Player> distantPlayers = this.Players.ToList();
        Player humanPlayer = this.GetHumanPlayer();

        distantPlayers.Remove(humanPlayer);
        distantPlayers.Remove(this.GetLeftPlayer(humanPlayer));
        distantPlayers.Remove(this.GetRightPlayer(humanPlayer));

        return distantPlayers.ToArray();
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
    /// Get a card by its id.
    /// </summary>
    /// <param name="cardID">The id of the card to retrieve.</param>
    /// <returns>The corresponding card.</returns>
    public static Card GetCardById(string cardID)
    {
        return CardsDAO.GetCardById(cardID);
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
    /// Distribute randomly a wonder to all players.
    /// </summary>
    /// <param name="side">Choose A/B side of wonders (both if not mentionned)</param>
    public void LoadWonders(char side = '\0')
    {
        List<Wonder> wonders = WondersDAO.GetWonders(side);

        Random rand = new Random();
        foreach (Player player in this.Players)
        {
            Wonder wonder = wonders.ElementAt(rand.Next(wonders.Count));
            List<Wonder> wondersToRemove = wonders.Where(w => w.Name.Contains(wonder.Name)).ToList();
            foreach (Wonder wonderToRemove in wondersToRemove)
                wonders.Remove(wonderToRemove); // Remove A & B side.
            player.WonderManager.Wonder = wonder;
            ResourceQuantity baseResource = new ResourceQuantity
            {
                Type = wonder.BaseResource,
                Quantity = 1
            };
            player.City.AddToResourceTree(new ResourceQuantity[] { baseResource }, false, false);
        }
    }

    /// <summary>
    /// Set an AI for each non human players according to AI level.
    /// </summary>
    /// <param name="level">The AI level of cleverness. </param>
    public void LoadAI(int level)
    {
        foreach (Player p in Players)
        {
            if (!p.IsHuman)
            {
                p.AI = new AIManager(p);
                switch (level)
                {
                    case 1:  // Easy - mostly random
                        p.AI.RandomBuilding = true;
                        p.AI.RandomWonder = true;
                        p.AI.RandomDiscard = true;
                        p.AI.TradeAbility = true;
                        break;
                    case 2: // Easy alternative - wonder first
                        p.AI.RandomBuilding = true;
                        p.AI.RandomWonder = true;
                        p.AI.RandomDiscard = true;
                        p.AI.WonderFocused = true;
                        p.AI.TradeAbility = true;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Perform the move decided by the AI.
    /// </summary>
    /// <param name="choice">Move chosen by the AI.</param>
    public void ApplyAIChoice(Player player, AIManager.Choice choice)
    {
        switch (choice.Action)
        {
            case AIManager.Action.BUILD_CITY:
                player.City.Build(choice.CardToPlay, false);
                break;
            case AIManager.Action.BUILD_WONDER:
                // Hack: TEMP AI action to perform is not taken into account.
                player.WonderManager.BuildWonder(choice.CardToPlay.ID);
                break;
            case AIManager.Action.DISCARD:
                player.City.Discard(choice.CardToPlay.ID);
                DiscardPile.Add(Playable.GetPlayable(choice.CardToPlay.ID, choice.CardToPlay.Type));
                break;
        }
    }

    /// <summary>
    /// Perform all end turn game actions.
    /// </summary>
    /// <param name="gc">Current game controller.</param>
    public void EndTurn(GameController gc)
    {
        bool skipRotating = false;
        foreach (Player p in Players)
        {
            if (!p.IsHuman && p.Hand.Count > 0)
            {
                AIManager.Choice choice = p.AI.Play();
                this.ApplyAIChoice(p, choice);
                gc.RefreshAIBoards();
                gc.SetLastMove(p, choice);
            }
            if (p.Hand.Count == 1)
                if (p.WonderManager.HasExtraBuildBonus())
                    skipRotating = true;
                else
                {
                    if (!p.IsHuman)
                        DiscardPile.Add(Playable.GetPlayable(p.Hand[0].ID, p.Hand[0].Type));
                    p.Hand.RemoveAt(0);
                }
            else if (p.Hand.Count < 1)
                p.Hand = new List<Card>();
            p.City.TradeResources = new Dictionary<CityManager.TradeLocation, Dictionary<ResourceType, int>>
            {
                { CityManager.TradeLocation.EAST, new Dictionary<ResourceType, int>() },
                { CityManager.TradeLocation.WEST, new Dictionary<ResourceType, int>() }
            };
        }
        Players.ForEach(p => p.City.IsLastCardResource = false);
        if (!skipRotating)
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
            CityManager leftCity = this.GetLeftCity(players[i]);
            CityManager rightCity = this.GetRightCity(players[i]);
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
            CityManager leftCity = this.GetLeftCity(players[i]);
            CityManager rightCity = this.GetRightCity(players[i]);
            int currentBonus = 0;
            List<BonusCard> guilds = players[i].City.Bonus;
            foreach (BonusCard guild in guilds)
            {
                switch (guild.Bonus)
                {
                    case BonusCard.BonusType.CARD_BONUS:
                        currentBonus += players[i].City.CalculateCardBonus(guild, leftCity, rightCity);
                        break;
                    case BonusCard.BonusType.DEFEAT_BONUS:
                        currentBonus += players[i].City.CalculateDefeatBonus(players, i);
                        break;
                    case BonusCard.BonusType.WONDER_BONUS:
                        currentBonus += players[i].City.CalculateWonderBonus(players, i, false);
                        break;
                }
            }
            bonusResults.Add(currentBonus);
        }
        return bonusResults;
    }

    /// <summary>
    /// Get all players bonus score (commercial buildings related).
    /// </summary>
    /// <returns>List of players bonus score.</returns>
    public List<int> GetCommercialBonusResults()
    {
        List<int> comResults = new List<int>();
        foreach (Player p in this.Players)
        {
            int commercialBonus = 0;
            List<Card> comCards = p.City.CommercialBuildings;
            foreach (Card c in comCards)
                if (c is BonusCard && ((BonusCard)c).Reward.Any(r => r.Reward == BonusCard.RewardType.VICTORY_POINT))
                {
                    if (((BonusCard)c).Bonus == BonusCard.BonusType.WONDER_BONUS)
                        commercialBonus += p.City.CalculateWonderBonus(
                            this.Players.ToArray(), 
                            this.Players.IndexOf(p),
                            true
                            );
                    else
                        commercialBonus += p.City.CalculateCardBonus(
                        (BonusCard)c,
                        this.GetLeftCity(p),
                        this.GetRightCity(p)
                        );
                }
            comResults.Add(commercialBonus);
        }
        return comResults;
    }

    /// <summary>
    /// Get all players treasure score.
    /// </summary>
    /// <returns>List of players treasure score.</returns>
    public List<int> GetTreasureResults()
    {
        List<int> treasureResults = new List<int>();
        foreach (Player p in this.Players)
            treasureResults.Add(p.Coins / 3);
        return treasureResults;
    }

    /// <summary>
    /// Get all players wonder score.
    /// </summary>
    /// <returns>List of players wonder score.</returns>
    public List<int> GetWonderResults()
    {
        List<int> wonderResults = new List<int>();
        foreach (Player p in this.Players)
            wonderResults.Add(p.WonderManager.GetWonderPoints());
        return wonderResults;
    }

    /// <summary>
    /// Reset the free build count for each player's city.
    /// </summary>
    public void ResetFreeBuildCount()
    {
        foreach (Player p in this.Players)
            p.City.FreeBuildCount = 0;
    }
}
