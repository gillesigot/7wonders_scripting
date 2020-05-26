public class CityManager
{
    // Used to represent the player owning the city.
    public Player Owner { get; set; }

    public CityManager(Player player)
    {
        this.Owner = player;
    }
    /// <summary>
    /// Notify the city that a card has been dicarded. It's considered as sold in aid of the city.
    /// </summary>
    public void Discard() 
    {
        this.Owner.Coins += GameConsts.DISCARDED_CARD_VALUE;
    }
}
