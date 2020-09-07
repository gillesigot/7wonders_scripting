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
    }
}
