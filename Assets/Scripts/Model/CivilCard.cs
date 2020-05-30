public class CivilCard : Card
{
    // Used to represent the victory points the card will add to a city.
    public int VictoryPoints { get; set; }

    public CivilCard(Card card, int victoryPoints) : base(card)
    {
        this.VictoryPoints = victoryPoints;
    }
}
