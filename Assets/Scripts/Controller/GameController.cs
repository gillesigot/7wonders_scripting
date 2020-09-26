using System.Collections.Generic;

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
        if (age > 3)
        {
            PlayerBoardController.DisplayScoreBoard(this.GameManager.Players);
        }
        else
        {
            GameManager.Age = age;
            this.GameManager.DistributeCards();
            this.GameManager.ResetFreeBuildCount();
            PlayerBoardController.RefreshHand();
            PlayerBoardController.RefreshDiscardPiles(age);

            if (age == 1)
            {
                this.GameManager.LoadWonders();
                PlayerBoardController.RefreshWonderBoard(
                    this.GameManager.GetHumanPlayer().WonderManager.Wonder.ID,
                    this.GameManager.GetHumanPlayer().WonderManager.Wonder.Steps.Count
                    );
            }
        }
    }

    /// <summary>
    /// Resolve war conflicts between each cities.
    /// </summary>
    /// <param name="age">The current achieving age.</param>
    public void ResolveConflicts(int age)
    {
        const int WEST = 0, EAST = 1;
        List<int[]> victoryMatrix = this.GameManager.CaculateWarResults();

        for (int i = 0; i < victoryMatrix.Count; i++)
        {
            EvaluateResult(victoryMatrix[i][WEST], "WEST", this.GameManager.Players[i]);
            EvaluateResult(victoryMatrix[i][EAST], "EAST", this.GameManager.Players[i]);
        }

        PlayerBoardController.RefreshWarPoints();

        // Update war victory points, defeat points and defeat tokens.
        void EvaluateResult(int result, string side, Player p)
        {
            if (result > 0)
                p.VictoryWarPoints += GameConsts.WAR_VICTORY_POINTS[age - 1];
            else if (result < 0)
            {
                p.VictoryWarPoints += GameConsts.WAR_DEFEAT_POINTS;
                if (side.Contains("WEST"))
                    p.WestDefeatWarTokens += 1;
                else
                    p.EastDefeatWarTokens += 1;
            }
                    
        }
    }

    /// <summary>
    /// Retrieve neighbours buildings from given types.
    /// </summary>
    /// <param name="currentPlayer">The player to retrieve neighbours from.</param>
    /// <param name="cardTypes">The types of buildings to retrieve.</param>
    /// <returns>The list of neighbours buildings from given types.</returns>
    public List<Card> GetNeighboursBuildings(Player currentPlayer, Card.CardType[] cardTypes)
    {
        List<Card> neighboursBuildings = new List<Card>();

        neighboursBuildings.AddRange(this.GameManager.GetLeftCity(currentPlayer).GetAllBuildings(cardTypes));
        neighboursBuildings.AddRange(this.GameManager.GetRightCity(currentPlayer).GetAllBuildings(cardTypes));

        return neighboursBuildings;
    }
}
