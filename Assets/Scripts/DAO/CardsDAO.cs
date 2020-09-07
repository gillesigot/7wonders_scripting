using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

/// <summary>
/// This class represents cards information stored in data file.
/// </summary>
public class CardDAO
{
    private const int DEFAULT_INT_VALUE = -1;
    private const bool DEFAULT_BOOL_VALUE = false;

    public struct ResourceQuantity
    {
        public int Type { get; set; }
        public int Quantity { get; set; }
    }
    
    public struct BuildCondition
    {
        public ResourceQuantity[] Resources { get; set; }
        public string[] ChainFrom { get; set; }
    }

    public struct RewardQuantity
    {
        public int Reward { get; set; }
        public int Quantity { get; set; }
    }

    public string ID { get; set; }
    public string Name { get; set; }
    public int CardType { get; set; }
    public int Age { get; set; }
    public BuildCondition CardBuildCondition { get; set; }
    public string[] ChainTo { get; set; }
    public int NPlayersPlayable { get; set; }

    [DefaultValue(null)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public ResourceQuantity[] Resources { get; set; }

    [DefaultValue(DEFAULT_BOOL_VALUE)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool IsOptional { get; set; }

    [DefaultValue(DEFAULT_BOOL_VALUE)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool IsBuyable { get; set; }

    [DefaultValue(DEFAULT_INT_VALUE)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public int WarPoints { get; set; }

    [DefaultValue(DEFAULT_INT_VALUE)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public int VictoryPoints { get; set; }

    [DefaultValue(DEFAULT_INT_VALUE)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public int Price { get; set; }

    [DefaultValue(DEFAULT_BOOL_VALUE)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool LeftPlayer { get; set; }

    [DefaultValue(DEFAULT_BOOL_VALUE)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool RightPlayer { get; set; }

    [DefaultValue(DEFAULT_INT_VALUE)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public int ScienceCardType { get; set; }

    [DefaultValue(null)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public RewardQuantity[] Reward { get; set; }

    [DefaultValue(DEFAULT_BOOL_VALUE)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool CheckLeft { get; set; }

    [DefaultValue(DEFAULT_BOOL_VALUE)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool CheckRight { get; set; }

    [DefaultValue(DEFAULT_BOOL_VALUE)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public bool CheckSelf { get; set; }

    [DefaultValue(DEFAULT_INT_VALUE)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public int BonusType { get; set; }

    [DefaultValue(null)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public int[] BonusCardType { get; set; }
    [DefaultValue(DEFAULT_INT_VALUE)]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    public int ResourceKind { get; set; }
}

/// <summary>
/// Cards data access object, manipulate the cards information stored in data file.
/// </summary>
public static class CardsDAO
{
    /// <summary>
    /// Retrieve all stored cards and returns them in as card list.
    /// </summary>
    /// <param name="nbPlayers">Get corresponding cards according to the number of players.</param>
    /// <param name="age">Get the cards for a given age of the game.</param>
    /// <returns>All stored cards as list of Card objects.</returns>
    public static List<Card> GetCards(int nbPlayers = 0, int age = 0)
    {
        List<Card> cards = new List<Card>();
        if (!File.Exists(StorageConsts.CARD_FILE_LOCATION))
            return cards;

        string jsonValues = File.ReadAllText(StorageConsts.CARD_FILE_LOCATION);
        List<CardDAO> cardsDAO = JsonConvert.DeserializeObject<List<CardDAO>>(jsonValues);

        foreach (CardDAO cardDAO in cardsDAO)
        {
            if (nbPlayers != 0 && cardDAO.NPlayersPlayable > nbPlayers)
                continue;

            if (age != 0 && cardDAO.Age != age)
                continue;

            switch ((Card.CardType)cardDAO.CardType)
            {
                case Card.CardType.RESOURCE:
                    cards.Add(GetResourceCard(cardDAO));
                    break;
                case Card.CardType.WAR:
                    cards.Add(GetWarCard(cardDAO));
                    break;
                case Card.CardType.CIVIL:
                    cards.Add(GetCivilCard(cardDAO));
                    break;
                case Card.CardType.COMMERCIAL:
                    if (cardDAO.Price < 0)
                    {
                        if (cardDAO.Resources is null)
                            cards.Add(GetBonusCard(cardDAO)); // Commercial cards acting as bonus cards
                        else
                            cards.Add(GetResourceCard(cardDAO)); // Commercial cards acting as resources
                    }
                    else
                        cards.Add(GetCommercialCard(cardDAO));
                    break;
                case Card.CardType.SCIENCE:
                    cards.Add(GetScienceCard(cardDAO));
                    break;
                case Card.CardType.GUILD:
                    cards.Add(GetBonusCard(cardDAO));
                    break;
            }
        }
        return cards;
    }

    /// <summary>
    /// Retrieve basic fields to instanciate a Card.
    /// </summary>
    /// <param name="cardDAO">The object containing the card information.</param>
    /// <returns>A new Card</returns>
    private static Card GetCard(CardDAO cardDAO)
    {
        return new Card(
            cardDAO.ID,
            cardDAO.Name,
            (Card.CardType)cardDAO.CardType,
            cardDAO.Age,
            GetBuildCondition(cardDAO.CardBuildCondition),
            cardDAO.ChainTo,
            cardDAO.NPlayersPlayable
            );
    }

    /// <summary>
    /// Retrieve missing fields to instanciate a ResourceCard.
    /// </summary>
    /// <param name="cardDAO">The object containing the card information.</param>
    /// <returns>A new ResourceCard</returns>
    private static ResourceCard GetResourceCard(CardDAO cardDAO)
    {
        Card card = GetCard(cardDAO);
        return new ResourceCard(
            card, 
            GetResources(cardDAO.Resources.ToList()), 
            cardDAO.IsOptional, 
            cardDAO.IsBuyable
            );
    }

    /// <summary>
    /// Retrieve missing fields to instanciate a WarCard.
    /// </summary>
    /// <param name="cardDAO">The object containing the card information.</param>
    /// <returns>A new WarCard</returns>
    private static WarCard GetWarCard(CardDAO cardDAO)
    {
        Card card = GetCard(cardDAO);
        return new WarCard(card, cardDAO.WarPoints);
    }

    /// <summary>
    /// Retrieve missing fields to instanciate a CivilCard.
    /// </summary>
    /// <param name="cardDAO">The object containing the card information.</param>
    /// <returns>A new Civilcard.</returns>
    private static CivilCard GetCivilCard(CardDAO cardDAO)
    {
        Card card = GetCard(cardDAO);
        return new CivilCard(card, cardDAO.VictoryPoints);
    }

    /// <summary>
    /// Retrieve missing fields to instanciate a CommercialCard.
    /// </summary>
    /// <param name="cardDAO">The object containing the card information.</param>
    /// <returns>A new CommercialCard.</returns>
    private static CommercialCard GetCommercialCard(CardDAO cardDAO)
    {
        Card card = GetCard(cardDAO);
        return new CommercialCard(
            card, 
            GetResources(cardDAO.Resources.ToList()),
            cardDAO.Price,
            cardDAO.LeftPlayer,
            cardDAO.RightPlayer
            );
    }

    /// <summary>
    /// Retrieve missing fields to instanciate a ScienceCard.
    /// </summary>
    /// <param name="cardDAO">The object containing the card information.</param>
    /// <returns>A new ScienceCard.</returns>
    private static ScienceCard GetScienceCard(CardDAO cardDAO)
    {
        Card card = GetCard(cardDAO);
        return new ScienceCard(card, (Card.ScienceType)cardDAO.ScienceCardType);
    }

    /// <summary>
    /// Retrieve missing fields to instanciate a BonusCard.
    /// </summary>
    /// <param name="cardDAO">The object containing the card information.</param>
    /// <returns>A new BonusCard</returns>
    private static BonusCard GetBonusCard(CardDAO cardDAO)
    {
        Card card = GetCard(cardDAO);
        return new BonusCard(
            card,
            GetReward(cardDAO.Reward.ToList()),
            cardDAO.CheckLeft,
            cardDAO.CheckRight,
            cardDAO.CheckSelf,
            (BonusCard.BonusType)cardDAO.BonusType,
            GetBonusCardType(cardDAO.BonusCardType.ToList()),
            (BonusCard.ResourceMetaType)cardDAO.ResourceKind
            );
    }

    /// <summary>
    /// Cast the CardDAO build condition into the Card build condition.
    /// </summary>
    /// <param name="bcDAO">The CardDAO build condition.</param>
    /// <returns>The build condition as Card build condition.</returns>
    private static Card.BuildCondition GetBuildCondition(CardDAO.BuildCondition bcDAO)
    {
        Card.BuildCondition bc = new Card.BuildCondition
        {
            ChainFrom = bcDAO.ChainFrom,
            Resources = GetResources(bcDAO.Resources.ToList())
        };
        return bc;
    }

    /// <summary>
    /// Cast a CardDAO resources array into the Card applicable resouces array.
    /// </summary>
    /// <param name="rqsDAO">The CardDAO resources array.</param>
    /// <returns>The resources list as Card applicable resources list.</returns>
    public static Card.ResourceQuantity[] GetResources(List<CardDAO.ResourceQuantity> rqsDAO)
    {
        List<Card.ResourceQuantity> resources = new List<Card.ResourceQuantity>();
        foreach (CardDAO.ResourceQuantity rqDAO in rqsDAO)
        {
            Card.ResourceQuantity rq = new Card.ResourceQuantity
            {
                Quantity = rqDAO.Quantity,
                Type = (Card.ResourceType)rqDAO.Type
            };
            resources.Add(rq);
        }
        return resources.ToArray();
    }

    /// <summary>
    /// Cast a CardDAO rewards array into the Card applicable rewards array.
    /// </summary>
    /// <param name="rqsDAO">The CardDAO rewards array.</param>
    /// <returns>The rewards list as Card applicable rewards list.</returns>
    public static BonusCard.RewardQuantity[] GetReward(List<CardDAO.RewardQuantity> rqsDAO)
    {
        List<BonusCard.RewardQuantity> rewards = new List<BonusCard.RewardQuantity>();
        foreach (CardDAO.RewardQuantity rqDAO in rqsDAO)
        {
            BonusCard.RewardQuantity rq = new BonusCard.RewardQuantity
            {
                Quantity = rqDAO.Quantity,
                Reward = (BonusCard.RewardType)rqDAO.Reward
            };
            rewards.Add(rq);
        }
        return rewards.ToArray();
    }

    /// <summary>
    /// Cast a CardDAO card types array into the Card applicable card types array.
    /// </summary>
    /// <param name="btsDAO">The CardDAO card types array.</param>
    /// <returns>The card type list as Card applicable card type list.</returns>
    private static Card.CardType[] GetBonusCardType(List<int> btsDAO)
    {
        List<Card.CardType> types = new List<Card.CardType>();
        foreach (int bt in btsDAO)
        {
            types.Add((Card.CardType)bt);
        }
        return types.ToArray();
    }
} 