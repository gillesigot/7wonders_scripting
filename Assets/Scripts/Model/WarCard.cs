public class WarCard : Card
{
    // Used to represent the war points the card will add to a city.
    public int WarPoints { get; set; }

    public WarCard(
        Card card,
        int warPoints
        ) : base(card)
    {
        this.WarPoints = warPoints;
    }
}
