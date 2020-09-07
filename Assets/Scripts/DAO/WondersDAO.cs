using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using static CardDAO;
using static Step;

/// <summary>
/// This class represents wonders information stored in data file.
/// </summary>
public class WonderDAO
{
    private const int DEFAULT_INT_VALUE = -1;

    public struct StepDAO
    {
        public ResourceQuantity[] StepBuildCondition { get; set; }
        public int[] StepTypes { get; set; }

        [DefaultValue(null)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public RewardQuantity[] Reward { get; set; }

        [DefaultValue(DEFAULT_INT_VALUE)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int WarPoints { get; set; }

        [DefaultValue(DEFAULT_INT_VALUE)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int TradeType { get; set; }
        [DefaultValue(DEFAULT_INT_VALUE)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Resources { get; set; }

        [DefaultValue(DEFAULT_INT_VALUE)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int BuildType { get; set; }
    }

    public string ID { get; set; }
    public string Name { get; set; }
    public char Side { get; set; }
    public int BaseResource { get; set; }
    public StepDAO[] Steps { get; set; }
}

/// <summary>
/// Wonders data access object, manipulate the wonders information stored in data file.
/// </summary>
public static class WondersDAO
{
    /// <summary>
    /// Retrieve all stored wonders and returns them in as wonder list.
    /// </summary>
    /// <param name="side">Get corresponding wonders according to the side of it.</param>
    /// <returns>All stored wonders as list of Wonder objects.</returns>
    public static List<Wonder> GetWonders(char side = '\0')
    {
        List<Wonder> wonders = new List<Wonder>();
        if (!File.Exists(StorageConsts.CARD_FILE_LOCATION))
            return wonders;

        string jsonValues = File.ReadAllText(StorageConsts.WONDER_FILE_LOCATION);
        List<WonderDAO> wondersDAO = JsonConvert.DeserializeObject<List<WonderDAO>>(jsonValues);

        foreach (WonderDAO wonderDAO in wondersDAO)
        {
            if (side != '\0' && wonderDAO.Side != side)
                continue;

            wonders.Add(GetWonder(wonderDAO));
        }

        return wonders;
    }

    /// <summary>
    /// Retrieve fields to instanciate a Wonder.
    /// </summary>
    /// <param name="wonderDAO">The object containing the wonder information.</param>
    /// <returns>A new Wonder</returns>
    private static Wonder GetWonder(WonderDAO wonderDAO)
    {
        return new Wonder(
                wonderDAO.ID,
                wonderDAO.Name,
                wonderDAO.Side,
                (Card.ResourceType)wonderDAO.BaseResource,
                GetSteps(wonderDAO)
            );
    }

    /// <summary>
    /// Cast a WonderDAO steps array into the Wonder applicable steps array.
    /// </summary>
    /// <param name="wonderDAO">The object containing the steps information.</param>
    /// <returns>The list of wonder steps.</returns>
    private static List<Step> GetSteps(WonderDAO wonderDAO)
    {
        List<Step> steps = new List<Step>();

        foreach (WonderDAO.StepDAO stepDAO in wonderDAO.Steps)
        {
            steps.Add(new Step(
                CardsDAO.GetResources(stepDAO.StepBuildCondition.ToList()),
                stepDAO.StepTypes.Cast<StepType>().ToArray(),
                stepDAO.Reward != null ? CardsDAO.GetReward(stepDAO.Reward.ToList()): new BonusCard.RewardQuantity[0],
                stepDAO.WarPoints,
                (AcquisitionType)stepDAO.TradeType,
                (BonusCard.ResourceMetaType)stepDAO.Resources,
                (BuilderType)stepDAO.BuildType
                ));
        }

        return steps;
    }
}
