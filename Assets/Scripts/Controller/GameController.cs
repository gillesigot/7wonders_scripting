public class GameController
{
    // The GameManager for the current game.
    private GameManager GameManager { get; set; }

    public GameController(int nbPlayer)
    {
        GameManager.NbPlayers = nbPlayer;
        this.GameManager = GameManager.Instance();
    }
}
