using System.Collections.Generic;
using System.Linq;
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
        ResourceTreeNode Parent { get; set; }
        // The node children (only several children if optional resources).
        List<ResourceTreeNode> Children { get; set; }

        public ResourceTreeNode(ResourceQuantity rq)
        {
            this.Resources = new Dictionary<ResourceType, int>() { [rq.Type] = rq.Quantity };
            this.BuyableResources = new Dictionary<ResourceType, int>() { [rq.Type] = rq.Quantity };
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
            if (buyable)
                node.BuyableResources = this.BuyableResources
                .Concat(node.BuyableResources)
                .GroupBy(o => o.Key)
                .ToDictionary(o => o.Key, o => o.Sum(v => v.Value));
            this.Children.Add(node);

            return node;
        }
    }

    // Represents the player owning the city.
    public Player Owner { get; set; }
    // Represents the city resources.
    public List<ResourceCard> Resources { get; set; }
    // Represents the city war buildings.
    public List<WarCard> WarBuildings { get; set; }
    // Represents the city civil buildings.
    public List<CivilCard> CivilBuildings { get; set; }
    // Represents the city commercial buildings.
    public List<CommercialCard> CommercialBuildings { get; set; }
    // Represents the city science buildings.
    public List<ScienceCard> ScienceBuildings { get; set; }
    // Represents the city bonuses (guilds, commercial bonuses, ...).
    public List<BonusCard> Bonus { get; set; }
    // Represents all resources of the city. As some are switchable, all possible values are gathered.
    public List<ResourceTreeNode> ResourceTreeLeaves { get; set; }

    public CityManager(Player player)
    {
        this.Owner = player;
        this.Resources = new List<ResourceCard>();
        this.WarBuildings = new List<WarCard>();
        this.CivilBuildings = new List<CivilCard>();
        this.CommercialBuildings = new List<CommercialCard>();
        this.ScienceBuildings = new List<ScienceCard>();
        this.Bonus = new List<BonusCard>();
        this.ResourceTreeLeaves = new List<ResourceTreeNode>();
    }
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
    /// Build the building within the city if building conditions are met.
    /// </summary>
    /// <param name="building_id">The id of the building to build.</param>
    /// <returns>True if built.</returns>
    public bool Build(string building_id)
    {
        Card building = this.Owner.Hand.Select(o => o).Where(o => o.ID == building_id).First();

        if (IsBuildable(building))
        {
            switch (building.Type)
            {
                case CardType.RESOURCE:
                    ResourceCard resourceBuilding = (ResourceCard)building;
                    this.Resources.Add(resourceBuilding);
                    this.AddToResourceTree(
                        resourceBuilding.Resources, 
                        resourceBuilding.IsOptional, 
                        resourceBuilding.IsBuyable
                        );
                    break;
                //case CardType.WAR: this.WarBuildings.Add((WarCard)building);
                //    break;
                //case CardType.CIVIL: this.CivilBuildings.Add((CivilCard)building);
                //    break;
                //case CardType.COMMERCIAL: this.CommercialBuildings.Add((CommercialCard)building);
                //    break;
                //case CardType.SCIENCE: this.ScienceBuildings.Add((ScienceCard)building);
                //    break;
                //case CardType.GUILD: this.Bonus.Add((BonusCard)building);
                //    break;
            }
            this.Owner.Hand.Remove(building);

            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Get a list of resources that can be bought.
    /// </summary>
    /// <returns>The list of all type of resources.</returns>
    private List<Dictionary<ResourceType, int>> GetBuyableResources()
    {
        return this.ResourceTreeLeaves.Select(o => o.BuyableResources).ToList();
    }

    /// <summary>
    /// Add new resources to the city tree of resources.
    /// </summary>
    /// <param name="resources">The resources to add.</param>
    /// <param name="optional">If the the player must choose between the given resources.</param>
    /// <param name="buyable">If those resources can be bought by other players.</param>
    private void AddToResourceTree(ResourceQuantity[] resources, bool optional, bool buyable)
    {
        List<ResourceTreeNode> newResources = null;
        if (optional) 
            newResources = new List<ResourceTreeNode>();
        foreach (ResourceQuantity rq in resources)
        {
            if (!optional)
                newResources = new List<ResourceTreeNode>();

            // TODO Should be initialized by resource on the wonder board
            if (this.ResourceTreeLeaves.Count == 0)
                newResources.Add(new ResourceTreeNode(rq));

            foreach (ResourceTreeNode node in this.ResourceTreeLeaves)
            {
                newResources.Add(node.Add(new ResourceTreeNode(rq), buyable));
            }
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
    /// <returns>True if buildable.</returns>
    private bool IsBuildable(Card building)
    {
        // Same building cannot be built twice
        string[] buildingNames = this.GetBuildingNames();
        if (buildingNames.Contains(building.Name))
            return false;

        // TODO ability to buy resources from neighbours

        // Free to build or only GOLD
        if (building.CardBuildCondition.Resources.Length == 0)
            return true;
        else if (building.CardBuildCondition.Resources.Length == 1)
        {
            ResourceQuantity firstRes = building.CardBuildCondition.Resources.ElementAt(0);
            if (firstRes.Type == ResourceType.GOLD && this.Owner.Coins >= firstRes.Quantity)
            {
                this.Owner.Coins -= firstRes.Quantity;
                return true;
            }
        }

        // Check chainings
        foreach (string buildingName in building.CardBuildCondition.ChainFrom)
            if (buildingNames.Contains(buildingName))
                return true;

        // Check city resources
        foreach (ResourceTreeNode rtn in this.ResourceTreeLeaves)
        {
            bool canBuild = true;
            foreach (ResourceQuantity rq in building.CardBuildCondition.Resources)
            {
                if (rq.Type == ResourceType.GOLD)
                {
                    if (this.Owner.Coins < rq.Quantity)
                        canBuild = false;
                    else
                        this.Owner.Coins -= rq.Quantity;
                }
                else
                {
                    if (rtn.Resources.Keys.Contains(rq.Type))
                    {
                        if (rtn.Resources[rq.Type] < rq.Quantity)
                            canBuild = false;
                    }
                    else
                        canBuild = false;
                }
            }
            if (canBuild)
                return true;
        }

        return false;
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
}
