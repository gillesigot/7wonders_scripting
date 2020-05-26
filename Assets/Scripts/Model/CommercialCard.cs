public class CommercialCard : Card
{
    // Used to tell which resources will be involved for the given price.
    public ResourceQuantity[] Resources { get; set; }
    // Used to tell the price of the related resources.
    public int Price { get; set; }
    // Used to tell if the deal involved the player on the left.
    public bool LeftPlayer { get; set; }
    // Used to tell if the deal involved the player on the right.
    public bool RightPlayer { get; set; }

    public CommercialCard(
        string id,
        string name,
        CardType type,
        BuildCondition buildCondition,
        string[] chainTo,
        int nPlayersPlayable,
        ResourceQuantity[] resources,
        int price,
        bool leftPlayer,
        bool rightPlayer
        ) : base(id, name, type, buildCondition, chainTo, nPlayersPlayable)
    {
        this.Resources = resources;
        this.Price = price;
        this.LeftPlayer = leftPlayer;
        this.RightPlayer = rightPlayer;
    }
}
