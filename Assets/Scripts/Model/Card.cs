public abstract class Card
{
    // Used to define all different card types.
    public enum CardType
    {
        RESOURCE = 0,
        WAR = 1,
        CIVIL = 2,
        COMMERCIAL = 3,
        SCIENCE = 4,
        GUILD = 5,
    }

    // Used to define all different resource types.
    public enum ResourceType
    {
        CLAY,
        ORE,
        STONE,
        WOOD,
        GLASS,
        LOOM,
        PAPYRUS,
        GOLD,
    }

    // Used to define all diferent science symbols.
    public enum ScienceType
    {
        TABLET,
        GEAR,
        COMPASS,
    }

    // Used to associate a resource type and the related quantity.
    public struct ResourceQuantity
    {
        public ResourceType Type { get; set; }
        public int Quantity { get; set; }
    }

    // Used to define the build condition either by chaining or by using the required resources.
    public struct BuildCondition
    {
        public ResourceQuantity[] Resources { get; set; }
        public string[] ChainFrom { get; set; }
    }

    // Used to idenfity the card.
    public string ID { get; set; }
    // Used to store the pretty name of the card.
    public string Name { get; set; }
    // Defines the type of the card.
    public CardType Type { get; set; }
 
    // Used to tell which condition fulfil to build the card.
    public BuildCondition CardBuildCondition { get; set; }
    // Used to tell opportunities of building other cards for free.
    public string[] ChainTo { get; set; }

    // Used to define in which kind of game (number of players) the card will be used.
    public int NPlayersPlayable { get; set; }

    public Card(
        string id, 
        string name, 
        CardType type, 
        BuildCondition buildCondition, 
        string[] chainTo, 
        int nPlayersPlayable
        ) 
    {
        this.ID = id;
        this.Name = name;
        this.Type = type;
        this.CardBuildCondition = buildCondition;
        this.ChainTo = chainTo;
        this.NPlayersPlayable = nPlayersPlayable;
    }
}
