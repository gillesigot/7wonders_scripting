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
        // Hack: TEMP Display scores board
        if (age > 3)
        {
            List<int> civilRes = this.GameManager.GetCivilResults();
            string civilLabel = "";
            for (int i = 0; i < civilRes.Count; i++)
                civilLabel += "Joueur " + (i + 1) + ": " + civilRes[i] + " points \n";

            UnityEngine.Debug.Log(
                    "Game has ended. \n" +
                    "Civil victory points : \n" +
                    civilLabel
                    );
        }
        else
        {
            GameManager.Age = age;
            this.GameManager.DistributeCards();
            PlayerBoardController.RefreshHand();
            PlayerBoardController.RefreshDiscardPiles(age);
        }
    }

    /// <summary>
    /// Resolve war conflicts between each cities.
    /// </summary>
    /// <param name="age">The current achieving age.</param>
    public void ResolveConflicts(int age)
    {
        List<int[]> victoryMatrix = this.GameManager.CaculateWarResults();

        // Hack: TEMP apply war points to IA
        int[] humanResults = victoryMatrix[0];
        EvaluateResult(humanResults[0], "WEST");
        EvaluateResult(humanResults[1], "EAST");

        PlayerBoardController.RefreshWarPoints();

        // Update war victory points, defeat points and defeat tokens.
        void EvaluateResult(int result, string side)
        {
            if (result > 0)
                this.GameManager.GetHumanPlayer().VictoryWarPoints += GameConsts.WAR_VICTORY_POINTS[age - 1];
            else if (result < 0)
            {
                this.GameManager.GetHumanPlayer().VictoryWarPoints += GameConsts.WAR_DEFEAT_POINTS;
                if (side.Contains("WEST"))
                    this.GameManager.GetHumanPlayer().WestDefeatWarTokens += 1;
                else
                    this.GameManager.GetHumanPlayer().EastDefeatWarTokens += 1;
            }
                    
        }
    }
}
