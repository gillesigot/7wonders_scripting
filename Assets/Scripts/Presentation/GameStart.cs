using UnityEngine;

// TODO TEMP Class for mocking initial menu
public class GameStart : MonoBehaviour
{
    public int NumberOfPlayers;
    private GameController GameController { get; set; }
    private PlayerBoardController PlayerBoardController { get; set; }

    /// <summary>
    /// Initialize class attributes.
    /// </summary>
    void Awake()
    {
        this.GameController = new GameController(this.NumberOfPlayers);
        this.PlayerBoardController = new PlayerBoardController();
    }
}
