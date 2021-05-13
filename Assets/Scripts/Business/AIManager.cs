
using System;
using System.Collections.Generic;
using System.Linq;
using static Card;

public class AIManager
{
    #region AI configuration
    // At build step, randomly pick the building.
    public bool RandomBuilding { get; set; }
    // At wonder building step, randomly pick the building to use for it.
    public bool RandomWonder { get; set; }
    // When discarding, randomly pick the card to discard.
    public bool RandomDiscard { get; set; }
    // Balance choice in favour of wonder construction.
    public bool WonderFocused { get; set; }
    // At wonder/card built, AI can trade if missing resource.
    public bool TradeAbility { get; set; }
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
    // Reference to the game manager.
    private GameManager GM { get; set; }
    // Convention for left/right player.
    private const int LEFT = 0, RIGHT = 1;
    // Convention for raw(wood, stone, clay, ore)/manufactured (loom, glass, papyrus) resources.
    private const int RAW = 0, MANUFACTURED = 1;

    public AIManager(Player p)
    {
        this.Player = p;
        this.RandomBuilding = false;
        this.RandomWonder = false;
        this.RandomDiscard = false;
        this.WonderFocused = false;
        this.TradeAbility = false;
        this.GM = GameManager.Instance();
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

        List<Card> buildableCards = this.GetBuildableCards();

        bool regularBuildAvailable = buildableCards.Count > 0;
        bool wonderBuildAvailable = !this.Player.WonderManager.IsWonderBuilt() && this.Player.WonderManager.IsNextStepBuildable();

        if (
            this.TradeAbility &&
            this.WonderFocused &&
            !this.Player.WonderManager.IsNextStepBuildable() &&
            !this.Player.WonderManager.IsWonderBuilt()
            )
        {

            List<ResourceQuantity[]> missingResources = new List<ResourceQuantity[]>();
            foreach (CityManager.ResourceTreeNode rtn in this.Player.City.ResourceTreeLeaves)
                missingResources.Add(this.Player.City.GetMissingResources(rtn.Resources, this.Player.WonderManager.GetNextStep().BuildCondition, false));

            ResourceQuantity[] lessMR = missingResources.ToArray().OrderByDescending(mr => mr.Sum(rq => rq.Quantity)).Last();
            CityManager leftCity = this.GM.GetLeftCity(this.Player);
            CityManager rightCity = this.GM.GetRightCity(this.Player);
            // Ignore just played resource when computing trading possibilities.
            List<Dictionary<ResourceType, int>> leftDictResources = (leftCity.IsLastCardResource) ? leftCity.GetBuyableResources(true) : leftCity.GetBuyableResources();
            List<Dictionary<ResourceType, int>> rightDictResources = (rightCity.IsLastCardResource) ? rightCity.GetBuyableResources(true) : rightCity.GetBuyableResources();
            // Cast List of dict to List of ResourceQuantity.
            List<ResourceQuantity[]> leftResources = leftDictResources.Select(dict => dict.Select(item => new ResourceQuantity { Type = item.Key, Quantity = item.Value }).ToArray()).ToList();
            List<ResourceQuantity[]> rightResources = rightDictResources.Select(dict => dict.Select(item => new ResourceQuantity { Type = item.Key, Quantity = item.Value }).ToArray()).ToList();

            // Used to store available resources like this: quantity, (int)resourceType, location(LEFT/RIGHT), price.
            List<int[]> tradeResPrice = GetOptimizedTrading(lessMR, leftResources, rightResources);

            if (tradeResPrice.Count > 0)
            {
                int totalPrice = tradeResPrice.Sum(x => x[0] * x[3]);
                if (totalPrice <= this.Player.Coins)
                {
                    this.Player.Coins -= totalPrice;
                    foreach (int[] debt in tradeResPrice)
                    {
                        // TODO: DEBUG
                        string logline = this.Player.Name + " - Quantity: " + debt[0] + ", Resource: " + (ResourceType)debt[1] + ", " + ((debt[2] == 0) ? "LEFT" : "RIGHT") + ", Price: " + debt[3];
                        UnityEngine.Debug.Log(logline);
                        // ---------------
                        if (debt[2] == LEFT)
                            this.GM.GetLeftPlayer(this.Player).Coins += debt[0] * debt[3];
                        else
                            this.GM.GetRightPlayer(this.Player).Coins += debt[0] * debt[3];
                    }

                    choice.Action = Action.BUILD_WONDER;
                    choice.CardToPlay = GetRandomCard();
                    return choice;
                }
            }
        }

        if (wonderBuildAvailable && this.WonderFocused)
        {
            if (this.RandomWonder)
            {
                choice.Action = Action.BUILD_WONDER;
                choice.CardToPlay = GetRandomCard();
            }
        }
        else if (regularBuildAvailable)
        {
            if (this.RandomBuilding)
            {
                choice.Action = Action.BUILD_CITY;
                choice.CardToPlay = GetRandomCard(buildableCards);
            }
        }
        else if (wonderBuildAvailable)
        {
            if (this.RandomWonder)
            {
                choice.Action = Action.BUILD_WONDER;
                choice.CardToPlay = GetRandomCard();
            }
        }
        else
        {
            if (this.TradeAbility)
            { } // TODO: try to build anything with traded resources (if possible).
            choice.CardToPlay = GetRandomCard();
        }
        return choice;
    }

    /// <summary>
    /// Check if missing resources available to neighbours and get cheaper price for them.
    /// </summary>
    /// <param name="missingResources">The resources to look for.</param>
    /// <param name="leftResources">The resources available at left player.</param>
    /// <param name="rightResources">The resources available at right player.</param>
    /// <returns>List of quantity, resource, location & price where missing resources have to be bought.</returns>
    private List<int[]> GetOptimizedTrading(ResourceQuantity[] missingResources, List<ResourceQuantity[]> leftResources, List<ResourceQuantity[]> rightResources)
    {
        List<int[]> tradeResPrice = new List<int[]>();

        foreach (ResourceQuantity[] leftRes in leftResources)
        {
            foreach (ResourceQuantity[] rightRes in rightResources)
            {
                List<ResourceQuantity> allResLst = new List<ResourceQuantity>(leftRes.ToList());

                // Add or merge if resource is existing.
                foreach (ResourceQuantity rq in rightRes)
                {
                    int resIdx = allResLst.FindIndex(lr => lr.Type == rq.Type);
                    if (resIdx > -1)
                    {
                        ResourceQuantity tempRes = allResLst.ElementAt(resIdx);
                        tempRes.Quantity += rq.Quantity;
                        allResLst[resIdx] = tempRes;
                    }
                    else
                        allResLst.Add(rq);
                }

                // Check if all missing resources are available.
                bool tradingPossible = true;
                foreach (ResourceQuantity mr in missingResources)
                {
                    int mrIdx = allResLst.FindIndex(rq => rq.Type == mr.Type);
                    if (mrIdx < 0 || allResLst[mrIdx].Quantity < mr.Quantity)
                    {
                        tradingPossible = false;
                        break;
                    }
                }

                // Calculate price and keep track of debts.
                if (tradingPossible)
                {
                    foreach (ResourceQuantity mr in missingResources)
                    {
                        for (int i = 0; i < mr.Quantity; i++)
                        {
                            int typeRes = CityManager.RAW_RESOURCES.Contains(mr.Type) ? RAW : MANUFACTURED;
                            int[] firstPlayer = new int[4];
                            int[] secondPlayer = new int[4];
                            ResourceQuantity[] firstRes;
                            ResourceQuantity[] secondRes;
                            if (this.Player.City.EastTradePrice[typeRes] < this.Player.City.WestTradePrice[typeRes])
                            {
                                firstPlayer = new int[] { 1, (int)mr.Type, RIGHT, this.Player.City.EastTradePrice[typeRes] };
                                firstRes = rightRes;
                                secondPlayer = new int[] { 1, (int)mr.Type, LEFT, this.Player.City.WestTradePrice[typeRes] };
                                secondRes = leftRes;
                            }
                            else
                            {
                                firstPlayer = new int[] { 1, (int)mr.Type, LEFT, this.Player.City.WestTradePrice[typeRes] };
                                firstRes = leftRes;
                                secondPlayer = new int[] { 1, (int)mr.Type, RIGHT, this.Player.City.EastTradePrice[typeRes] };
                                secondRes = rightRes;
                            }

                            int mrIdx = firstRes.ToList().FindIndex(rq => rq.Type == mr.Type);
                            if (mrIdx > -1)
                            {
                                if (firstRes[mrIdx].Quantity == 0)
                                    firstRes.ToList().RemoveAt(mrIdx);
                                else
                                    firstRes[mrIdx].Quantity--;
                                tradeResPrice.Add(firstPlayer);
                            }
                            else
                            {
                                mrIdx = secondRes.ToList().FindIndex(rq => rq.Type == mr.Type);
                                if (secondRes[mrIdx].Quantity == 0)
                                    secondRes.ToList().RemoveAt(mrIdx);
                                else
                                    secondRes[mrIdx].Quantity--;
                                tradeResPrice.Add(secondPlayer);
                            }
                        }
                    }
                    return tradeResPrice;
                }
            }
        }
        return tradeResPrice;
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
