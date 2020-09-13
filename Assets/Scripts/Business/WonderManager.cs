using System.Linq;
using static CityManager;

public class WonderManager
{
    // Used to represent the player owning the wonder.
    public Player Owner { get; set; }
    // The wonder representation.
    public Wonder Wonder { get; set; }
    // Used to tell the last wonder step achieved.
    public int AchievedSteps { get; set; }

    public WonderManager(Player player)
    {
        this.Owner = player;
        this.AchievedSteps = 0;
    }

    /// <summary>
    /// Tell if the wonder is fully built or not.
    /// </summary>
    /// <returns>True if wonder is done.</returns>
    public bool IsWonderBuilt()
    {
        return this.AchievedSteps == this.Wonder.Steps.Count;
    }

    /// <summary>
    /// Build a wonder step and apply effect, if building conditions are met.
    /// </summary>
    /// <param name="building_id">The building discarded in order to build a wonder step.</param>
    /// <returns></returns>
    public bool BuildWonder(string building_id)
    {
        Step nextStep = this.Wonder.Steps[AchievedSteps];
        bool canBuild = false;

        foreach (ResourceTreeNode rtn in this.Owner.City.ResourceTreeLeaves)
            if (this.Owner.City.HasMatchingResources(rtn.Resources, nextStep.BuildCondition))
            {
                canBuild = true;
                break;
            }
        if (!canBuild)
            return false;

        Card building = this.Owner.Hand.Select(o => o).Where(o => o.ID == building_id).First();
        this.Owner.Hand.Remove(building);

        this.AddWonderStep(nextStep);
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="step"></param>
    private void AddWonderStep(Step step)
    {
        // TODO Apply effect on game
        this.AchievedSteps++;
    }
}
