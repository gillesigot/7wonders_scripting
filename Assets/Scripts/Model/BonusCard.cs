public class BonusCard : Card 
{
    // Define the type of reward that can be granted.
    public enum RewardType
    {
        VICTORY_POINT = 0,
        GOLD = 1,
    }

    // Define the calculation mode used to evaluate bonus.
    public enum BonusType
    {
        CARD_BONUS = 0,
        WONDER_BONUS = 1,
        DEFEAT_BONUS = 2,
        SCIENCE_BONUS = 3,
        FREE_BONUS = 4
    }

    // Define the meta type of resource (for resources based bonus).
    public enum ResourceMetaType
    {
        RAW = 0,
        MANUFACTURED = 1
    }

    // Used to associate the reward type and the quantity of it.
    public struct RewardQuantity 
    {
        public RewardType Reward { get; set; }
        public int Quantity { get; set; }
    }

    // Used to tell if the left city will be involved in the points count.
    public bool CheckLeft { get; set; }
    // Used to tell if the right city will be involved in the points count.
    public bool CheckRight { get; set; }
    // Used to tell if the player's city will be involved in the points count.
    public bool CheckSelf { get; set; }
    // Used to tell which reward will be granted for all matching conditions.
    public RewardQuantity[] Reward { get; set; }
    // Used to tell which calculation mode will determine the bonus.
    public BonusType Bonus { get; set; }
    // Used to tell which type of card will grant you a bonus.
    public Card.CardType[] BonusCardType { get; set; }
    // Used to tell, in case of bonus on resources cards, which resources are counted.
    public ResourceMetaType ResourceKind { get; set; }

    public BonusCard(
        Card card,
        RewardQuantity[] reward,
        bool checkLeft,
        bool checkRight,
        bool checkSelf,
        BonusType bonus,
        CardType[] bonusCardType,
        ResourceMetaType resourceKind
        ) : base(card)
    {
        this.Reward = reward;
        this.CheckLeft = checkLeft;
        this.CheckRight = checkRight;
        this.CheckSelf = checkSelf;
        this.Bonus = bonus;
        this.BonusCardType = bonusCardType;
        this.ResourceKind = resourceKind;
    }
}