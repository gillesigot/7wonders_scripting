using System;
using System.Collections.Generic;
using System.Linq;
using static BonusCard;
using static Card;
public class CityManager
{
    // Define the city resource tree
    public class ResourceTreeNode
    {
        // The type and quantity of resources that the node represents.
        public Dictionary<ResourceType, int> Resources { get; set; }
        // Same as Resources, but keep only buyable resources.
        public Dictionary<ResourceType, int> BuyableResources { get; set; }
        // The node parent.
        public ResourceTreeNode Parent { get; set; }
        // The node children (only several children if optional resources).
        List<ResourceTreeNode> Children { get; set; }

        public ResourceTreeNode(ResourceQuantity rq, bool buyable)
        {
            this.Resources = new Dictionary<ResourceType, int>() { [rq.Type] = rq.Quantity };
            if (buyable)
                this.BuyableResources = new Dictionary<ResourceType, int>() { [rq.Type] = rq.Quantity };
            else
                this.BuyableResources = new Dictionary<ResourceType, int>();
            this.Children = new List<ResourceTreeNode>();
        }

        // Attach a new node as child of the current node.
        public ResourceTreeNode Add(ResourceTreeNode node, bool buyable)
        {
            node.Parent = this;
            node.Resources = this.Resources
                .Concat(node.Resources)
                .GroupBy(o => o.Key)
                .ToDictionary(o => o.Key, o => o.Sum(v => v.Value));
            node.BuyableResources = this.BuyableResources
                .Concat(node.BuyableResources)
                .GroupBy(o => o.Key)
                .ToDictionary(o => o.Key, o => o.Sum(v => v.Value));
            this.Children.Add(node);

            return node;
        }
    }
    // Represent the trade location.
    public enum TradeLocation
    {
        EAST,
        WEST
    }
    // The price to pay for resources on east trading.
    public int[] EastTradePrice { get; set; }
    // The price to pay for resources on west trading.
    public int[] WestTradePrice { get; set; }
    // Represents the player owning the city.
    public Player Owner { get; set; }
    // Represents the city resources.
    public List<ResourceCard> Resources { get; set; }
    // Represents temporary resources get by trading.
    public Dictionary<TradeLocation, Dictionary<ResourceType, int>> TradeResources { get; set; }
    // Define the raw type resources.
    public static readonly ResourceType[] RAW_RESOURCES = new ResourceType[] {
            ResourceType.CLAY,
            ResourceType.ORE,
            ResourceType.STONE,
            ResourceType.WOOD
        };
    // Represents the city war buildings.
    public List<WarCard> WarBuildings { get; set; }
    // Represents the city civil buildings.
    public List<CivilCard> CivilBuildings { get; set; }
    // Represents the city commercial buildings.
    public List<Card> CommercialBuildings { get; set; }
    // Represents the city science buildings.
    public List<ScienceCard> ScienceBuildings { get; set; }
    // Represents the city bonuses (guilds, commercial bonuses, ...).
    public List<BonusCard> Bonus { get; set; }
    // Represents all resources of the city. As some are switchable, all possible values are gathered.
    public List<ResourceTreeNode> ResourceTreeLeaves { get; set; }
    // Represent the number of free build performed during the current age.
    public int FreeBuildCount { get; set; }
    // Used to tell AI if last built card was a resource.
    public bool IsLastCardResource { get; set; }

    public CityManager(Player player)
    {
        this.Owner = player;
        this.Resources = new List<ResourceCard>();
        this.TradeResources = new Dictionary<TradeLocation, Dictionary<ResourceType, int>>
        {
            { TradeLocation.EAST, new Dictionary<ResourceType, int>() },
            { TradeLocation.WEST, new Dictionary<ResourceType, int>() }
        };
        this.WarBuildings = new List<WarCard>();
        this.CivilBuildings = new List<CivilCard>();
        this.CommercialBuildings = new List<Card>();
        this.ScienceBuildings = new List<ScienceCard>();
        this.Bonus = new List<BonusCard>();
        this.ResourceTreeLeaves = new List<ResourceTreeNode>();
        this.EastTradePrice = new int[] { GameConsts.DEFAULT_TRADE_PRICE, GameConsts.DEFAULT_TRADE_PRICE };
        this.WestTradePrice = new int[] { GameConsts.DEFAULT_TRADE_PRICE, GameConsts.DEFAULT_TRADE_PRICE };
    }

    #region City management

    /// <summary>
    /// Notify the city that a card has been dicarded. It's considered as sold in aid of the city.
    /// </summary>
    public void Discard(string building_id) 
    {
        Card building = this.Owner.Hand.Select(o => o).Where(o => o.ID == building_id).First();
        this.Owner.Hand.Remove(building);
        this.Owner.Coins += GameConsts.DISCARDED_CARD_VALUE;
    }

    /// <summary>
    /// Check how much money is needed to build a given building.
    /// </summary>
    /// <param name="building">The building to build.</param>
    /// <returns>The amount of coins needed.</returns>
    public int IsMoneyRequired(Card building)
    {
        if (building.CardBuildCondition.Resources.Length == 1)
        {
            ResourceQuantity firstRes = building.CardBuildCondition.Resources.ElementAt(0);
            if (firstRes.Type == ResourceType.GOLD)
                return firstRes.Quantity;
        }
        return 0;
    }

    /// <summary>
    /// Build the building within the city if building conditions are met.
    /// </summary>
    /// <param name="building">The building card to build.</param>
    /// <param name="isFreeBuild">If the building can be built for free.</param>
    /// <returns>True if built.</returns>
    public bool Build(Card building, bool isFreeBuild)
    {
        if (IsBuildable(building, isFreeBuild))
        {
            this.Owner.Coins -= this.IsMoneyRequired(building);
            switch (building.Type)
            {
                case CardType.RESOURCE:
                    ResourceCard rc = (ResourceCard)building;
                    this.Resources.Add(rc);
                    this.IsLastCardResource = true;
                    this.AddToResourceTree(rc.Resources, rc.IsOptional, rc.IsBuyable);
                    break;
                case CardType.WAR:
                    this.WarBuildings.Add((WarCard)building);
                    break;
                case CardType.CIVIL:
                    this.CivilBuildings.Add((CivilCard)building);
                    break;
                case CardType.COMMERCIAL:
                    this.CommercialBuildings.Add(building);
                    if (building is ResourceCard comResCard)
                        this.AddToResourceTree(comResCard.Resources, comResCard.IsOptional, comResCard.IsBuyable);
                    else if (building is BonusCard bc)
                        this.ApplyDirectCommercialBonus(bc);
                    else if (building is CommercialCard cc)
                        this.ApplyTradeReduction(cc);
                    break;
                case CardType.SCIENCE:
                    this.ScienceBuildings.Add((ScienceCard)building);
                    break;
                case CardType.GUILD:
                    this.Bonus.Add((BonusCard)building);
                    break;
            }
            this.Owner.Hand.Remove(building);

            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Get all city buildings of given types.
    /// </summary>
    /// <param name="cardTypes">The list of building types to retrieve.</param>
    /// <returns>The list of city buildings.</returns>
    public List<Card> GetAllBuildings(CardType[] cardTypes)
    {
        List<Card> allBuildings = new List<Card>();
        foreach (CardType type in cardTypes)
            switch (type)
            {
                case CardType.RESOURCE:
                    allBuildings.AddRange(this.Resources);
                    break;
                case CardType.WAR:
                    allBuildings.AddRange(this.WarBuildings);
                    break;
                case CardType.CIVIL:
                    allBuildings.AddRange(this.CivilBuildings);
                    break;
                case CardType.COMMERCIAL:
                    allBuildings.AddRange(this.CommercialBuildings);
                    break;
                case CardType.SCIENCE:
                    allBuildings.AddRange(this.ScienceBuildings);
                    break;
                case CardType.GUILD:
                    allBuildings.AddRange(this.Bonus);
                    break;
            }
        return allBuildings;
    }

    /// <summary>
    /// Get all city buildings names.
    /// </summary>
    /// <returns>Array of all buildings names.</returns>
    private string[] GetBuildingNames()
    {
        string[] resourceNames = this.Resources.Select(o => o.Name).ToArray();
        string[] warNames = this.WarBuildings.Select(o => o.Name).ToArray();
        string[] civilNames = this.CivilBuildings.Select(o => o.Name).ToArray();
        string[] commercialNames = this.CommercialBuildings.Select(o => o.Name).ToArray();
        string[] scienceNames = this.ScienceBuildings.Select(o => o.Name).ToArray();
        string[] bonusNames = this.Bonus.Select(o => o.Name).ToArray();

        return resourceNames
            .Concat(warNames)
            .Concat(civilNames)
            .Concat(commercialNames)
            .Concat(scienceNames)
            .Concat(bonusNames)
            .ToArray();
    }

    #endregion

    #region Build & resources management

    /// <summary>
    /// Get a list of resources that can be bought.
    /// </summary>
    /// <param name="ignoreLastResource">Ignore more recently played resource.</param>
    /// <returns>The list of all type of resources.</returns>
    public List<Dictionary<ResourceType, int>> GetBuyableResources(bool ignoreLastResource = false)
    {
        if (ignoreLastResource)
            return this.ResourceTreeLeaves.Select(o => o.Parent.BuyableResources).ToList();
        else
            return this.ResourceTreeLeaves.Select(o => o.BuyableResources).ToList();
    }

    /// <summary>
    /// Add new resources to the city tree of resources.
    /// </summary>
    /// <param name="resources">The resources to add.</param>
    /// <param name="optional">If the the player must choose between the given resources.</param>
    /// <param name="buyable">If those resources can be bought by other players.</param>
    public void AddToResourceTree(ResourceQuantity[] resources, bool optional, bool buyable)
    {
        List<ResourceTreeNode> newResources = null;

        if (optional) 
            newResources = new List<ResourceTreeNode>();

        foreach (ResourceQuantity rq in resources)
        {
            if (!optional)
                newResources = new List<ResourceTreeNode>();

            if (this.ResourceTreeLeaves.Count == 0)
                newResources.Add(new ResourceTreeNode(rq, buyable));
            else
                foreach (ResourceTreeNode node in this.ResourceTreeLeaves)
                    newResources.Add(node.Add(new ResourceTreeNode(rq, buyable), buyable));

            if (!optional)
                this.ResourceTreeLeaves = newResources;
        }
        if (optional)
            this.ResourceTreeLeaves = newResources;
    }

    /// <summary>
    /// Check if the city has enough resources or required chainings to build the building.
    /// </summary>
    /// <param name="building">The building to build.</param>
    /// <param name="isFreeBuild">If the building can be built for free.</param>
    /// <returns>True if buildable.</returns>
    public bool IsBuildable(Card building, bool isFreeBuild)
    {
        // Same building cannot be built twice
        string[] buildingNames = this.GetBuildingNames();
        if (buildingNames.Contains(building.Name))
            return false;

        if (isFreeBuild)
            return true;

        // Free to build
        if (building.CardBuildCondition.Resources.Length == 0)
            return true;

        // Only GOLD
        int coinsNeeded = IsMoneyRequired(building);
        if (building.CardBuildCondition.Resources.Length == 1  && coinsNeeded > 0)
            if (coinsNeeded <= this.Owner.Coins)
                return true;

        // Check chainings
        foreach (string buildingName in building.CardBuildCondition.ChainFrom)
            if (buildingNames.Contains(buildingName))
                return true;

        // Check city resources
        if (this.ResourceTreeLeaves.Count > 0)
        {
            foreach (ResourceTreeNode rtn in this.ResourceTreeLeaves)
                if (HasMatchingResources(rtn.Resources, building.CardBuildCondition.Resources))
                    return true;
        }
        else if (HasMatchingResources(new Dictionary<ResourceType, int>(), building.CardBuildCondition.Resources))
            return true;

        // Check wonder free build bonus
        if (this.Owner.WonderManager.HasFreeBuildBonus() && this.FreeBuildCount == 0)
        {
            this.FreeBuildCount++;
            return true;
        }

        return false;
    }

    /// <summary>
    /// If city has enough resources (bought or not) to build building, returns True.
    /// </summary>
    /// <param name="leafPath">The resources tree path to evaluate.</param>
    /// <param name="neededResources">The resources needed to build the building.</param>
    /// <returns>True if enough resources (bought or not).</returns>
    public bool HasMatchingResources(Dictionary<ResourceType, int> leafPath, ResourceQuantity[] neededResources)
    {
        return GetMissingResources(leafPath, neededResources).Length == 0;
    }

    /// <summary>
    /// Compare bought & produced resources with needed resources and return the difference.
    /// </summary>
    /// <param name="leafPath">The resources tree path to evaluate.</param>
    /// <param name="neededResources">The resources needed to build the building.</param>
    /// <param name="moneyUpdate">True if amount of coins should be counted as spent.</param>
    /// <returns>The difference of resources.</returns>
    public ResourceQuantity[] GetMissingResources(Dictionary<ResourceType, int> leafPath, ResourceQuantity[] neededResources, bool moneyUpdate=true)
    {
        List<ResourceQuantity> missingRes = new List<ResourceQuantity>();

        foreach (ResourceQuantity rq in neededResources)
        {
            if (rq.Type == ResourceType.GOLD)
            {
                if (this.Owner.Coins < rq.Quantity)
                    missingRes.Add(new ResourceQuantity { Type = ResourceType.GOLD, Quantity = rq.Quantity - this.Owner.Coins });
                else if (moneyUpdate)
                    this.Owner.Coins -= rq.Quantity;
            }
            else
            {
                int totalResQuantity = 0;
                totalResQuantity += (leafPath.ContainsKey(rq.Type)) ? leafPath[rq.Type] : 0;
                totalResQuantity += (this.TradeResources[TradeLocation.EAST].ContainsKey(rq.Type)) ? this.TradeResources[TradeLocation.EAST][rq.Type] : 0;
                totalResQuantity += (this.TradeResources[TradeLocation.WEST].ContainsKey(rq.Type)) ? this.TradeResources[TradeLocation.WEST][rq.Type] : 0;

                if (totalResQuantity < rq.Quantity)
                    missingRes.Add(new ResourceQuantity { Type = rq.Type, Quantity = rq.Quantity - totalResQuantity });
            }
        }

        return missingRes.ToArray();
    }

    /// <summary>
    /// Buy (sell) resources from neighbour.
    /// </summary>
    /// <param name="resources">The resources being traded.</param>
    public void BuyResources(Dictionary<ResourceType, int> resources, TradeLocation location)
    {
        const int RAW = 0, MANUFACTURED = 1;
        int[] totalResources = new int[] { 0, 0 };
        int typeRes;
        foreach (KeyValuePair<ResourceType, int> resource in resources)
        {
            typeRes = RAW_RESOURCES.Contains(resource.Key) ? RAW : MANUFACTURED;
            if (this.TradeResources[location].ContainsKey(resource.Key))
            {
                totalResources[typeRes] += (resource.Value - this.TradeResources[location][resource.Key]);
                this.TradeResources[location][resource.Key] = resource.Value;
            }
            else
            {
                this.TradeResources[location].Add(resource.Key, resource.Value);
                totalResources[typeRes] += resource.Value;
            }
        }

        int rawPrice;
        int manufacturedPrice;
        Player beneficiary;
        if (location == TradeLocation.WEST)
        {
            rawPrice = this.WestTradePrice[RAW];
            manufacturedPrice = this.WestTradePrice[MANUFACTURED];
            beneficiary = GameManager.Instance().GetLeftPlayer(this.Owner);
        }
        else
        {
            rawPrice = this.EastTradePrice[RAW];
            manufacturedPrice = this.EastTradePrice[MANUFACTURED];
            beneficiary = GameManager.Instance().GetRightPlayer(this.Owner);
        }

        int totalCost = (totalResources[RAW] * rawPrice)+ (totalResources[MANUFACTURED] * manufacturedPrice);
        this.Owner.Coins -= totalCost;
        beneficiary.Coins += totalCost;
    }

    #endregion

    #region War management

    /// <summary>
    /// Sum all war cards points for this city (and additional wonder war bonus if any).
    /// </summary>
    /// <returns>The total amount of war points.</returns>
    public int GetWarPoints()
    {
        return this.WarBuildings.Sum(warCard => warCard.WarPoints) + this.Owner.WonderManager.GetWarPoints();
    }

    #endregion

    #region Civil buildings management

    /// <summary>
    /// Sum all civil cards points for this city.
    /// </summary>
    /// <returns>The total amount of civil points.</returns>
    public int GetCivilPoints()
    {
        return this.CivilBuildings.Sum(civilCard => civilCard.VictoryPoints);
    }

    #endregion

    #region Science buildings management

    /// <summary>
    /// Compute science card points for the city.
    /// </summary>
    /// <returns>The total science points.</returns>
    public int GetSciencePoints()
    {
        const int TABLET = 0, GEAR = 1, COMPASS = 2;
        int[] scienceType = new int[3];

        foreach (ScienceCard sc in this.ScienceBuildings)
        {
            if ((int)sc.ScienceCardType == TABLET)
                scienceType[TABLET]++;
            else if ((int)sc.ScienceCardType == GEAR)
                scienceType[GEAR]++;
            else
                scienceType[COMPASS]++;
        }

        if (this.Bonus.Where(bonus => bonus.Bonus == BonusCard.BonusType.SCIENCE_BONUS).Count() > 0)
            scienceType = MaximizeSciencePoints(scienceType);

        if (this.Owner.WonderManager.HasScienceBonus())
            scienceType = MaximizeSciencePoints(scienceType);

        return this.ComputeScienceScore(scienceType);
    }

    /// <summary>
    /// Compute the total science score.
    /// </summary>
    /// <param name="scienceType">Science type sorted science card counts.</param>
    /// <returns>The total science score.</returns>
    private int ComputeScienceScore(int[] scienceType)
    {
        int total = 0;

        total += (int)Math.Pow((int)scienceType[(int)ScienceType.TABLET], 2);
        total += (int)Math.Pow((int)scienceType[(int)ScienceType.GEAR], 2);
        total += (int)Math.Pow((int)scienceType[(int)ScienceType.COMPASS], 2);

        int bonus = scienceType.Min();
        total += bonus * GameConsts.SCIENCE_BONUS;

        return total;
    }

    /// <summary>
    /// Maximize science points using science bonus.
    /// </summary>
    /// <param name="scienceType">Science type sorted science card counts.</param>
    /// <returns>Maximized science card counts including bonus.</returns>
    private int[] MaximizeSciencePoints(int[] scienceType)
    {
        int[] minOptimization = scienceType.ToArray();
        int[] maxOptimization = scienceType.ToArray();
        int min = scienceType.Min();
        int max = scienceType.Max();

        for (int i = 0; i < minOptimization.Length; i++)
            if (minOptimization[i] == min)
            {
                minOptimization[i]++;
                break;
            }

        for (int i = 0; i < maxOptimization.Length; i++)
            if (maxOptimization[i] == max)
            {
                maxOptimization[i]++;
                break;
            }

        int minTotal = this.ComputeScienceScore(minOptimization);
        int maxTotal = this.ComputeScienceScore(maxOptimization);

        return (minTotal > maxTotal) ? minOptimization : maxOptimization;
    }

    #endregion

    #region Commercial buildings management

    /// <summary>
    /// Get commercial instant bonus (if applicable).
    /// </summary>
    /// <param name="bc">The commercial bonus card granting the bonus.</param>
    public void ApplyDirectCommercialBonus(BonusCard bc)
    {
        if (bc.BonusCardType.Length == 0)
        {
            if (bc.Bonus != BonusCard.BonusType.WONDER_BONUS)
                this.Owner.Coins += bc.Reward[0].Quantity;
            else
            {
                List<Player> players = GameManager.Instance().Players;
                this.Owner.Coins += this.CalculateWonderBonus(players.ToArray(), players.IndexOf(this.Owner), true) * bc.Reward.Where(r => r.Reward == RewardType.GOLD).First().Quantity;
            }
        }
        else
        {
            CityManager leftCity = GameManager.Instance().GetLeftCity(this.Owner);
            CityManager rightCity = GameManager.Instance().GetRightCity(this.Owner);
            this.Owner.Coins += this.CalculateCardBonus(bc, leftCity, rightCity);
        }
    }

    /// <summary>
    /// Apply a reduction on trade with involved neighbour at the given price for given resources.
    /// </summary>
    /// <param name="cc">The commercial card granting the trade reduction.</param>
    public void ApplyTradeReduction(CommercialCard cc)
    {
        ResourceType[] resources = cc.Resources.Select(res => res.Type).ToArray();

        if (cc.LeftPlayer)
            if (resources.All(RAW_RESOURCES.Contains))
                this.WestTradePrice[(int)ResourceMetaType.RAW] = cc.Price;
            else
                this.WestTradePrice[(int)ResourceMetaType.MANUFACTURED] = cc.Price;

        if (cc.RightPlayer)
            if (resources.All(RAW_RESOURCES.Contains))
                this.EastTradePrice[(int)ResourceMetaType.RAW] = cc.Price;
            else
                this.EastTradePrice[(int)ResourceMetaType.MANUFACTURED] = cc.Price;
    }

    /// <summary>
    /// Apply bilateral trade reduction on given resources at given price.
    /// </summary>
    /// <param name="metaType">The meta type of the resources involved.</param>
    /// <param name="price">The price set for the transactions.</param>
    public void ApplyTradeReduction(ResourceMetaType metaType, int price)
    {
        this.WestTradePrice[(int)metaType] = price;
        this.EastTradePrice[(int)metaType] = price;
    }

    #endregion

    #region Bonus buildings management

    /// <summary>
    /// Calculate bonus points based on cards built on current/left/right cities.
    /// </summary>
    /// <param name="bonusCard">The card giving the bonus.</param>
    /// <param name="leftCity">The left player's city.</param>
    /// <param name="rightCity">The right player's city.</param>
    /// <returns>The amount of points earned.</returns>
    public int CalculateCardBonus(BonusCard bonusCard, CityManager leftCity, CityManager rightCity)
    {
        List<Card> cardsToCheck = new List<Card>();
        int bonusPoints = 0;

        if (bonusCard.CheckSelf)
            cardsToCheck.AddRange(this.GetAllBuildings(bonusCard.BonusCardType));
        if (bonusCard.CheckLeft)
            cardsToCheck.AddRange(leftCity.GetAllBuildings(bonusCard.BonusCardType));
        if (bonusCard.CheckRight)
            cardsToCheck.AddRange(rightCity.GetAllBuildings(bonusCard.BonusCardType));

        foreach (Card c in cardsToCheck)
            foreach (Card.CardType bonusType in bonusCard.BonusCardType)
                bonusPoints += GetBonusPoints(bonusType, c, bonusCard);

        return bonusPoints;
    }

    /// <summary>
    /// Get amount of bonus points according to bonus card type.
    /// </summary>
    /// <param name="bonusCardType">The bonus card type.</param>
    /// <param name="card">The current card being checked.</param>
    /// <param name="bonusCard">The bonus card giving the current bonus.</param>
    /// <returns>The amount of points earned.</returns>
    private int GetBonusPoints(Card.CardType bonusCardType, Card card, BonusCard bonusCard)
    {
        int points = 0;
        string[] manufacturedResourcesCardNames = new string[] { "Presse", "Metier a tisser", "Verrerie" };

        if (bonusCardType == Card.CardType.RESOURCE && card.Type == Card.CardType.RESOURCE)
        {
            if (bonusCard.ResourceKind == BonusCard.ResourceMetaType.MANUFACTURED)
            {
                if (manufacturedResourcesCardNames.Contains(card.Name))
                    points += 2;
            }
            else if (bonusCard.ResourceKind == BonusCard.ResourceMetaType.RAW)
            {
                if (!manufacturedResourcesCardNames.Contains(card.Name))
                    points++;
            }
            else  // "Guilde des armateurs"
                points++;
        }
        else
        {
            if (bonusCardType == card.Type)
                points++;
        }

        return points;
    }

    /// <summary>
    /// Calculate bonus points based on war opponents defeats.
    /// </summary>
    /// <param name="players">List of all players.</param>
    /// <param name="currentPlayerIdx">Position of current player in the list.</param>
    /// <returns>The amount of points earned.</returns>
    public int CalculateDefeatBonus(Player[] players, int currentPlayerIdx)
    {
        int bonusPoints = 0;
        int leftPlayer = currentPlayerIdx - 1 < 0 ? players.Length - 1 : currentPlayerIdx - 1;
        int rightPlayer = currentPlayerIdx + 1 == players.Length ? 0 : currentPlayerIdx + 1;

        bonusPoints += players[leftPlayer].EastDefeatWarTokens;
        bonusPoints += players[leftPlayer].WestDefeatWarTokens;
        bonusPoints += players[rightPlayer].EastDefeatWarTokens;
        bonusPoints += players[rightPlayer].WestDefeatWarTokens;

        return bonusPoints;
    }

    /// <summary>
    /// Calculate bonus points based on achieved wonder steps of current player and opponents.
    /// </summary>
    /// <param name="players">List of all players.</param>
    /// <param name="currentPlayerIdx">Position of the current player in the list.</param>
    /// <param name="selfOnly">Count current player's steps only.</param>
    /// <returns>The amount of points earned.</returns>
    public int CalculateWonderBonus(Player[] players, int currentPlayerIdx, bool selfOnly)
    {
        int bonusPoints = 0;
        if (!selfOnly)
        {
            int leftPlayer = currentPlayerIdx - 1 < 0 ? players.Length - 1 : currentPlayerIdx - 1;
            int rightPlayer = currentPlayerIdx + 1 == players.Length ? 0 : currentPlayerIdx + 1;
            bonusPoints += players[leftPlayer].WonderManager.AchievedSteps.Count;
            bonusPoints += players[rightPlayer].WonderManager.AchievedSteps.Count;
        }
        bonusPoints += players[currentPlayerIdx].WonderManager.AchievedSteps.Count;

        return bonusPoints;
    }

    #endregion
}
