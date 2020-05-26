public class ResourceCard : Card
{
    // The types and quantities of resources a card will bring to a city.
    public ResourceQuantity[] Resources { get; set; }
    // Exclusive OR for the player to choose which resource & quantity he'll use.
    public bool IsOptional { get; set; }
    // If the ResourceQuantity can be bought by other players or not (cf. commercial resource).
    public bool IsBuyable { get; set; }

    public ResourceCard(
        string id,
        string name,
        CardType type,
        BuildCondition buildCondition,
        string[] chainTo,
        int nPlayersPlayable,
        ResourceQuantity[] resources,
        bool isOptional,
        bool isBuyable
        ) : base(id, name, type, buildCondition, chainTo, nPlayersPlayable)
    {
        this.Resources = resources;
        this.IsOptional = isOptional;
        this.IsBuyable = isBuyable;
    }
}
