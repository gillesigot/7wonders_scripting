public class GameController
{
    // The GameManager for the current game.
    private GameManager GameManager { get; set; }

    public GameController(int nbPlayer)
    {
        GameManager.NbPlayers = nbPlayer;
        this.GameManager = GameManager.Instance();
    }

    /// <summary>
    /// Distribute cards to start a new age.
    /// </summary>
    /// <param name="age">The age to be started.</param>
    public void StartAge(int age)
    {
        this.GameManager.Age = age;
        this.GameManager.DistributeCards();
        PlayerBoardController.RefreshHand();
    }
}
