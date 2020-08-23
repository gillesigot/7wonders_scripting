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

            List<int> scienceRes = this.GameManager.GetScienceResults();
            string scienceLabel = "";
            for (int i = 0; i < scienceRes.Count; i++)
                scienceLabel += "Joueur " + (i + 1) + ": " + scienceRes[i] + " points \n";

            List<int> guildRes = this.GameManager.GetBonusResults();
            string guildLabel = "";
            for (int i = 0; i < guildRes.Count; i++)
                guildLabel += "Joueur " + (i + 1) + ": " + guildRes[i] + " points \n";

            List<int> goldRes = this.GameManager.GetTreasureResults();
            string goldLabel = "";
            for (int i = 0; i < goldRes.Count; i++)
                goldLabel += "Joueur " + (i + 1) + ": " + goldRes[i] + " points \n";

            UnityEngine.Debug.Log(
                    "Game has ended. \n\n" +
                    "Civil victory points: \n" +
                    civilLabel + "\n" +
                    "Science victory points: \n" +
                    scienceLabel + "\n" +
                    "Guild victory points: \n" +
                    guildLabel + "\n" +
                    "Treasure victory points: \n" +
                    goldLabel
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
}
