using System.Collections.Generic;

public class GameController
{
    // The GameManager for the current game.
    private GameManager GameManager { get; set; }
    // The left virtual player board controller.
    private AIController LeftPlayer { get; set; }
    // The right virtual player board controller.
    private AIController RightPlayer { get; set; }

    public GameController(int nbPlayer)
    {
        GameManager.NbPlayers = nbPlayer;
        this.GameManager = GameManager.Instance();
    }

    /// <summary>
    /// Distribute cards to start a new age.
    /// </summary>
    /// <param name="age">The age to be started.</param>
    /// <param name="AILevel">The AI level of cleverness.</param>
    public void StartAge(int age, int AILevel=0)
    {
        if (age > 3)
        {
            this.RefreshAIBoards();
            this.CleanLastMove();
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
                Player humanPlayer = this.GameManager.GetHumanPlayer();
                Wonder humanWonder = humanPlayer.WonderManager.Wonder;
                PlayerBoardController.RefreshWonderBoard(humanWonder.ID, humanWonder.Steps.Count);

                this.GameManager.LoadAI(AILevel);
                this.LeftPlayer = PlayerBoardController.GetLeftAIBoard();
                this.RightPlayer = PlayerBoardController.GetRightAIBoard();
                Player leftPlayer = this.GameManager.GetLeftPlayer(humanPlayer);
                Player rightPlayer = this.GameManager.GetRightPlayer(humanPlayer);
                this.LeftPlayer.Player = leftPlayer;
                this.RightPlayer.Player = rightPlayer;
                this.LeftPlayer.InitializeAIBoard();
                this.RightPlayer.InitializeAIBoard();
                this.RefreshAIBoards();
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

    /// <summary>
    /// Refresh virtual players boards.
    /// </summary>
    public void RefreshAIBoards()
    {
        this.LeftPlayer.RefreshBoard();
        this.RightPlayer.RefreshBoard();
    }

    /// <summary>
    /// Update last player move on corresponding board.
    /// </summary>
    /// <param name="player">The player who did the move.</param>
    /// <param name="move">The last move.</param>
    public void SetLastMove(Player player, AIManager.Choice move)
    {
        if (player == this.LeftPlayer.Player)
            this.LeftPlayer.SetLastMove(move);
        else if (player == this.RightPlayer.Player)
            this.RightPlayer.SetLastMove(move);

        // TODO update other players board (if any)
    }

    /// <summary>
    /// Clean last move representation and add it to AI cards list.
    /// </summary>
    public void CleanLastMove()
    {
        this.LeftPlayer.CleanLastMove();
        this.RightPlayer.CleanLastMove();
    }
}
