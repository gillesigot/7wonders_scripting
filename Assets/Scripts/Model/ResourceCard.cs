public class ResourceCard : Card
{
    // The types and quantities of resources a card will bring to a city.
    public ResourceQuantity[] Resources { get; set; }
    // Exclusive OR for the player to choose which resource & quantity he'll use.
    public bool IsOptional { get; set; }
    // If the ResourceQuantity can be bought by other players or not (cf. commercial resource).
    public bool IsBuyable { get; set; }

    public ResourceCard(
        Card card,
        ResourceQuantity[] resources,
        bool isOptional,
        bool isBuyable
        ) : base(card)
    {
        this.Resources = resources;
        this.IsOptional = isOptional;
        this.IsBuyable = isBuyable;
    }
}
