public class BonusCard : Card 
{
    // Define the type of reward that can be granted.
    public enum RewardType
    {
        VICTORY_POINT,
        GOLD,
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

    public BonusCard(
        string id,
        string name,
        CardType type,
        BuildCondition buildCondition,
        string[] chainTo,
        int nPlayersPlayable,
        RewardQuantity[] reward,
        bool checkLeft,
        bool checkRight,
        bool checkSelf
        ) : base(id, name, type, buildCondition, chainTo, nPlayersPlayable)
    {
        this.Reward = reward;
        this.CheckLeft = checkLeft;
        this.CheckRight = checkRight;
        this.CheckSelf = checkSelf;
    }
}

public class ResourceBonusCard : BonusCard
{
    // Used to tell which type of card will grand you a bonus.
    public Card.CardType[] BonusCardType { get; set; }  // 3 cards for Shipowners guild

    public ResourceBonusCard(
        string id,
        string name,
        CardType type,
        BuildCondition buildCondition,
        string[] chainTo,
        int nPlayersPlayable,
        RewardQuantity[] reward,
        bool checkLeft,
        bool checkRight,
        bool checkSelf,
        CardType[] bonusCardType
        ) : base(id, name, type, buildCondition, chainTo, nPlayersPlayable, reward, checkLeft, checkRight, checkSelf)
    {
        this.BonusCardType = bonusCardType;
    }
}

public class WonderBonusCard : BonusCard
{
    // Check wonder stages
    public WonderBonusCard(
        string id,
        string name,
        CardType type,
        BuildCondition buildCondition,
        string[] chainTo,
        int nPlayersPlayable,
        RewardQuantity[] reward,
        bool checkLeft,
        bool checkRight,
        bool checkSelf
        ) : base(id, name, type, buildCondition, chainTo, nPlayersPlayable, reward, checkLeft, checkRight, checkSelf)
    {}
}

public class DefeatTokenBonusCard : BonusCard
{
    // Count defeat tokens
    public DefeatTokenBonusCard(
        string id,
        string name,
        CardType type,
        BuildCondition buildCondition,
        string[] chainTo,
        int nPlayersPlayable,
        RewardQuantity[] reward,
        bool checkLeft,
        bool checkRight,
        bool checkSelf
        ) : base(id, name, type, buildCondition, chainTo, nPlayersPlayable, reward, checkLeft, checkRight, checkSelf)
    { }
}