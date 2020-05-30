public class ScienceCard : Card
{
    // Used to tell the science type of the card (used in the count of science points).
    public ScienceType ScienceCardType { get; set; }

    public ScienceCard(
        Card card,
        ScienceType scienceType
        ) : base(card)
    {
        this.ScienceCardType = scienceType;
    }
}
