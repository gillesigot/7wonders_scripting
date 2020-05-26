public class WonderManager
{
    // Used to represent the player owning the city.
    public Player Owner { get; set; }

    public WonderManager(Player player)
    {
        this.Owner = player;
    }
}
