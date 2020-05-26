public class ScienceCard : Card
{
    // Used to tell the science type of the card (used in the count of science points).
    public ScienceType ScienceCardType { get; set; }

    public ScienceCard(
        string id,
        string name,
        CardType type,
        BuildCondition buildCondition,
        string[] chainTo,
        int nPlayersPlayable,
        ScienceType scienceType
        ) : base(id, name, type, buildCondition, chainTo, nPlayersPlayable)
    {
        this.ScienceCardType = scienceType;
    }
}
