
using System;
using System.Collections.Generic;

public class AIManager
{
    #region AI configuration
    // At build step, randomly pick the building.
    public bool RandomBuilding { get; set; }
    // At wonder building step, randomly pick the building to use for it.
    public bool RandomWonder { get; set; }
    // When discarding, randomly pick the card to discard.
    public bool RandomDiscard { get; set; }
    #endregion

    // Used to define the different actions AI could perform.
    public enum Action
    {
        BUILD_CITY = 0,
        BUILD_WONDER = 1,
        DISCARD = 2
    }
    // Used to represent the move performed by the AI.
    public struct Choice
    {
        public Action Action { get; set; }
        public Card CardToPlay { get; set; }
    }
    // The player attached to the AI.
    public Player Player { get; set; }

    public AIManager(Player p)
    {
        this.Player = p;
        this.RandomBuilding = false;
        this.RandomWonder = false;
        this.RandomDiscard = false;
    }

    /// <summary>
    /// Choose the move to perform for this turn, for this player.
    /// </summary>
    /// <returns>AI's move choice for this turn.</returns>
    public Choice Play()
    {
        Choice choice = new Choice()
        {
            Action = Action.DISCARD,
            CardToPlay = this.Player.Hand[0]
        };

        // Hack: TEMP AI cannot buy resources to build cards.
        List<Card> buildableCards = this.GetBuildableCards();

        if (buildableCards.Count > 0)
        {
            if (this.RandomBuilding)
            {
                choice.Action = Action.BUILD_CITY;
                choice.CardToPlay = GetRandomCard(buildableCards);
            }
        }
        else if (!this.Player.WonderManager.IsWonderBuilt() && this.Player.WonderManager.IsNextStepBuildable())
        {
            if (this.RandomWonder)
            {
                choice.Action = Action.BUILD_WONDER;
                choice.CardToPlay = GetRandomCard();
            }
        }
        else
        {
            choice.CardToPlay = GetRandomCard();
        }
        return choice;
    }

    /// <summary>
    /// Check which card is buildable in player's hand.
    /// </summary>
    /// <returns>The list of buildable cards.</returns>
    public List<Card> GetBuildableCards()
    {
        List<Card> buildableCards = new List<Card>();

        foreach (Card card in this.Player.Hand)
            if (this.Player.City.IsBuildable(card, false))
                buildableCards.Add(card);

        return buildableCards;
    }

    /// <summary>
    /// Pick a card randomly from the given collection, or if empty, the player's hand.
    /// </summary>
    /// <param name="availableCards">The collection in which the card is picked.</param>
    /// <returns>The random card.</returns>
    private Card GetRandomCard(List<Card> availableCards = null)
    {
        availableCards = availableCards ?? this.Player.Hand;
        Random rand = new Random();
        return availableCards[rand.Next(availableCards.Count)];
    }
}
