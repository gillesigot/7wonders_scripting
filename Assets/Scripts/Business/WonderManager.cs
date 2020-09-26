using System.Collections.Generic;
using System.Linq;
using static BonusCard;
using static Card;
using static CityManager;

public class WonderManager
{
    // Used to represent the player owning the wonder.
    public Player Owner { get; set; }
    // The wonder representation.
    public Wonder Wonder { get; set; }
    // List of all built wonder steps.
    public List<Step> AchievedSteps { get; set; }

    public WonderManager(Player player)
    {
        this.Owner = player;
        this.AchievedSteps = new List<Step>();
    }

    /// <summary>
    /// Tell if the wonder is fully built or not.
    /// </summary>
    /// <returns>True if wonder is done.</returns>
    public bool IsWonderBuilt()
    {
        return this.AchievedSteps.Count == this.Wonder.Steps.Count;
    }

    /// <summary>
    /// Build a wonder step and apply effect, if building conditions are met.
    /// </summary>
    /// <param name="building_id">The building discarded in order to build a wonder step.</param>
    /// <returns>The type of action to be performed (-1 if nothing).</returns>
    public int BuildWonder(string building_id)
    {
        Step nextStep = this.Wonder.Steps[this.AchievedSteps.Count];
        bool canBuild = false;

        foreach (ResourceTreeNode rtn in this.Owner.City.ResourceTreeLeaves)
            if (this.Owner.City.HasMatchingResources(rtn.Resources, nextStep.BuildCondition))
            {
                canBuild = true;
                break;
            }
        if (!canBuild)
            return -1;

        Card building = this.Owner.Hand.Select(o => o).Where(o => o.ID == building_id).First();
        this.Owner.Hand.Remove(building);

        int actionToPerform = this.AddWonderStep(nextStep);
        return actionToPerform;
    }

    /// <summary>
    /// Add the given step to the wonder and apply direct effects.
    /// </summary>
    /// <param name="step">The step to add to the wonder.</param>
    private int AddWonderStep(Step step)
    {
        // Action to perform:
        // 0 - step is build, do nothing
        // 1 - Refresh coins
        // 2 - Select guild to copy
        // 3 - Build card for discard pile

        int actionToPerform = 0;
        foreach (Step.StepType type in step.Types)
        {
            switch (type)
            {
                case Step.StepType.BONUS:
                    this.Owner.Coins += step.Reward
                        .Where(o => o.Reward == RewardType.GOLD)
                        .Select(o => o.Quantity)
                        .FirstOrDefault();
                    actionToPerform = 1;
                    break;
                case Step.StepType.COMMERCIAL:
                    if (step.CommercialType == Step.AcquisitionType.PRODUCTION)
                    {
                        List<ResourceQuantity> freeResources = new List<ResourceQuantity>();
                        if (step.ResourceMetaType == ResourceMetaType.RAW)
                        {
                            freeResources.Add(new ResourceQuantity { Type = ResourceType.CLAY, Quantity = 1 });
                            freeResources.Add(new ResourceQuantity { Type = ResourceType.ORE, Quantity = 1 });
                            freeResources.Add(new ResourceQuantity { Type = ResourceType.STONE, Quantity = 1 });
                            freeResources.Add(new ResourceQuantity { Type = ResourceType.WOOD, Quantity = 1 });
                        }
                        else
                        {
                            freeResources.Add(new ResourceQuantity { Type = ResourceType.GLASS, Quantity = 1 });
                            freeResources.Add(new ResourceQuantity { Type = ResourceType.LOOM, Quantity = 1 });
                            freeResources.Add(new ResourceQuantity { Type = ResourceType.PAPYRUS, Quantity = 1 });
                        }
                        this.Owner.City.AddToResourceTree(freeResources.ToArray(), true, false);
                    }
                    else
                        this.Owner.City.ApplyTradeReduction(step.ResourceMetaType, GameConsts.DEFAULT_TRADE_REDUCTION_PRICE);
                    break;
                case Step.StepType.GUILD:
                    actionToPerform = 2;
                    break;
                case Step.StepType.BUILDER:
                    if (step.Builder == Step.BuilderType.GARBAGE_BUILD)
                        actionToPerform = 3;
                    break;
            }
        }

        this.AchievedSteps.Add(step);
        return actionToPerform;
    }

    /// <summary>
    /// Sum all wonder steps points.
    /// </summary>
    /// <returns>The total amount of wonder points.</returns>
    public int GetWonderPoints()
    {
        int wonderPoints = 0;

        // Checking if steps with victory point bonuses and add them to total
        foreach (Step step in this.AchievedSteps)
            wonderPoints += step.Reward
                .Where(o => o.Reward == RewardType.VICTORY_POINT)
                .Select(o => o.Quantity)
                .FirstOrDefault();

        return wonderPoints;
    }

    /// <summary>
    /// Sum all wonder war steps war points.
    /// </summary>
    /// <returns>The sum of the war points.</returns>
    public int GetWarPoints()
    {
        return this.AchievedSteps
            .Where(step => step.Types.Any(stepType => stepType == Step.StepType.WAR))
            .Sum(warStep => warStep.WarPoints);
    }

    /// <summary>
    /// Tell if a science bonus is granted by the wonder.
    /// </summary>
    /// <returns>True if bonus applicable (related step has been built).</returns>
    public bool HasScienceBonus()
    {
        return this.AchievedSteps
            .Where(step => step.Types.Any(stepType => stepType == Step.StepType.SCIENCE))
            .Count() > 0;
    }

    /// <summary>
    /// Tell if an extra build is granted by the wonder.
    /// </summary>
    /// <returns>True if bonus applicable (related step has been built).</returns>
    public bool HasExtraBuildBonus()
    {
        return this.AchievedSteps
            .Where(step => step.Builder == Step.BuilderType.EXTRA_BUILD)
            .Count() > 0;
    }
}
