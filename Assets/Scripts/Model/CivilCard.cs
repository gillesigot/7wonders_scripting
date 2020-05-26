public class CivilCard : Card
{
    // Used to represent the victory points the card will add to a city.
    public int VictoryPoints { get; set; }

    public CivilCard(
        string id,
        string name,
        CardType type,
        BuildCondition buildCondition,
        string[] chainTo,
        int nPlayersPlayable,
        int victoryPoints
        ) : base(id, name, type, buildCondition, chainTo, nPlayersPlayable)
    {
        this.VictoryPoints = victoryPoints;
    }
}
