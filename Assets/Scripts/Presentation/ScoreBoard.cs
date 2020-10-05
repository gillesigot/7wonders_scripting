using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    public GameObject scorePanel;
    public GameObject scoreTable;
    public GameObject toggleScoreBoard;

    private GameManager GameManager { get; set; }
    private Text[] scores;

    private void Awake()
    {
        scores = scoreTable.GetComponentsInChildren<Text>();
        this.GameManager = GameManager.Instance();
    }

    /// <summary>
    /// Display all players scores on the score board.
    /// </summary>
    /// <param name="players">The list of all players.</param>
    public void Display(List<Player> players)
    {
        this.scorePanel.SetActive(true);
        this.toggleScoreBoard.SetActive(true);

        for (int i = 0; i < players.Count; i++)
        {
            // Header
            Text header = scores.Single(s => s.name == "r0" + i);
            header.text = (i == 0) ? "You" : "Player " + (i + 1);

            // War results
            Text warPoints = scores.Single(s => s.name == "r1" + i);
            warPoints.text = players[i].VictoryWarPoints.ToString();

            // Treasure results
            Text treasurePoints = scores.Single(s => s.name == "r2" + i);
            List<int> treasureScores = this.GameManager.GetTreasureResults();
            treasurePoints.text = treasureScores[i].ToString();

            // Wonder results
            Text wonderPoints = scores.Single(s => s.name == "r3" + i);
            List<int> wonderScores = this.GameManager.GetWonderResults();
            wonderPoints.text = wonderScores[i].ToString();

            // Civil results
            Text civilPoints = scores.Single(s => s.name == "r4" + i);
            List<int> civilScores = this.GameManager.GetCivilResults();
            civilPoints.text = civilScores[i].ToString();

            // Commercial results
            Text commercialPoints = scores.Single(s => s.name == "r5" + i);
            List<int> commercialScores = this.GameManager.GetCommercialBonusResults();
            commercialPoints.text = commercialScores[i].ToString();

            // Guild results
            Text guildPoints = scores.Single(s => s.name == "r6" + i);
            List<int> guildScores = this.GameManager.GetBonusResults();
            guildPoints.text = guildScores[i].ToString();

            // Science results
            Text sciencePoints = scores.Single(s => s.name == "r7" + i);
            List<int> scienceScores = this.GameManager.GetScienceResults();
            sciencePoints.text = scienceScores[i].ToString();

            // TOTAL
            Text totalPoints = scores.Single(s => s.name == "r8" + i);
            totalPoints.text = (
                players[i].VictoryWarPoints + 
                treasureScores[i] + 
                wonderScores[i] +
                civilScores[i] +
                commercialScores[i] +
                guildScores[i] +
                scienceScores[i]
                ).ToString();
        }
    }
}
