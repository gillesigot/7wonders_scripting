public class WarCard : Card
{
    // Used to represent the war points the card will add to a city.
    public int WarPoints { get; set; }

    public WarCard(
        string id,
        string name,
        CardType type,
        BuildCondition buildCondition,
        string[] chainTo,
        int nPlayersPlayable,
        int warPoints
        ) : base(id, name, type, buildCondition, chainTo, nPlayersPlayable)
    {
        this.WarPoints = warPoints;
    }
}
