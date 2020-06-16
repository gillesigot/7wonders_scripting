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
        // TODO check if an age is ending -> count war points

        // TODO TEMP Display scores board
        if (age > 3)
            UnityEngine.Debug.Log("Game has ended.");
        else
        {
            GameManager.Age = age;
            this.GameManager.DistributeCards();
            PlayerBoardController.RefreshHand();
            PlayerBoardController.RefreshDiscardPiles(age);
        }
    }
}
