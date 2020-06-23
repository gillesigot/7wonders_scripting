using UnityEngine;

// Hack: TEMP Class for mocking initial menu
public class GameStart : MonoBehaviour
{
    public int NumberOfPlayers;
    public int StartingAge;
    public GameController GameController { get; set; }
    private PlayerBoardController PlayerBoardController { get; set; }

    /// <summary>
    /// Initialize class attributes.
    /// </summary>
    private void Awake()
    {
        this.GameController = new GameController(this.NumberOfPlayers);
        this.PlayerBoardController = new PlayerBoardController();
    }

    /// <summary>
    /// Start the current game
    /// </summary>
    private void Start()
    {
        this.GameController.StartAge(this.StartingAge);
    }
}
