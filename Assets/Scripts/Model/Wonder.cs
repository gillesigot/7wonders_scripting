using System.Collections.Generic;
using static BonusCard;
using static Card;

public class Wonder
{
    // Used to idenfity the wonder.
    public string ID { get; set; }
    // Used to store the pretty name of the wonder.
    public string Name { get; set; }
    // Used to tell on wich side is the wonder (A/B).
    public char Side { get; set; }
    // Used to define the base resource granted by the wonder.
    public ResourceType BaseResource { get; set; }
    // Used to define the wonder steps that can be built.
    public List<Step> Steps { get; set; }

    public Wonder(string id, string name, char side, ResourceType baseResource, List<Step> steps)
    {
        this.ID = id;
        this.Name = name;
        this.Side = side;
        this.BaseResource = baseResource;
        this.Steps = steps;
    }
}

public class Step
{
    // Used to define all different step types.
    public enum StepType
    {
        BONUS = 0,
        WAR = 1,
        SCIENCE = 2,
        COMMERCIAL = 3,
        GUILD = 4,
        BUILDER = 5
    }
    // Used to define the way of resource acquisition.
    public enum AcquisitionType
    {
        PRODUCTION = 0,
        TRADE = 1
    }
    // Used to define the type of build bonus.
    public enum BuilderType
    {
        FREE_BUILD = 0,
        EXTRA_BUILD = 1,
        GARBAGE_BUILD = 2
    }

    // Used to define the ressource needed to achieve this step.
    public ResourceQuantity[] BuildCondition { get; set; }
    // Used to tell the types of the step.
    public StepType[] Types { get; set; }

    // Used to tell the reward granted in case of BONUS.
    public RewardQuantity[] Reward { get; set; }

    // Used to tell the amount of additional war points in case of WAR.
    public int WarPoints { get; set; }

    // Used to tell how commercial resources are acquired in case of COMMERCIAL.
    public AcquisitionType CommercialType { get; set; }
    // Used to tell which meta type of resources are affected in case of COMMERCIAL.
    public ResourceMetaType ResourceMetaType { get; set; }

    // Used to tell which kind of build bonus is applied in case of BUILDER.
    public BuilderType Builder { get; set; }

    public Step(
        ResourceQuantity[] buildCondition, 
        StepType[] types, 
        RewardQuantity[] reward, 
        int warPoints,
        AcquisitionType commercialType,
        ResourceMetaType resourceMetaType,
        BuilderType builder
        )
    {
        this.BuildCondition = buildCondition;
        this.Types = types;
        this.Reward = reward;
        this.WarPoints = warPoints;
        this.CommercialType = commercialType;
        this.ResourceMetaType = resourceMetaType;
        this.Builder = builder;
    }
}
