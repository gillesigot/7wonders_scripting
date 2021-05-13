using UnityEngine;

// Hack: TEMP Class for mocking initial menu
public class GameStart : MonoBehaviour
{
    public int NumberOfPlayers;
    public int StartingAge;
    public int AILevel;
    public GameObject ExtraPlayers;
    public GameController GameController { get; set; }
    private PlayerBoardController PlayerBoardController { get; set; }
    public bool TrainAI;
    public GameObject TrainingZone;
    /// <summary>
    /// Initialize class attributes.
    /// </summary>
    private void Awake()
    {
        this.GameController = new GameController(this.NumberOfPlayers);
        this.PlayerBoardController = new PlayerBoardController();

        if (this.NumberOfPlayers > 3)
            this.ExtraPlayers.SetActive(true);
        else
            this.ExtraPlayers.SetActive(false);
    }

    /// <summary>
    /// Start the current game (or current training if TraningAI enabled).
    /// </summary>
    private void Start()
    {
        if (!TrainAI)
            this.GameController.StartAge(this.StartingAge, AILevel);
        else
        {
            Debug.Log("Starting training...");
            foreach (Trainer trainer in this.TrainingZone.GetComponentsInChildren<Trainer>())
                trainer.StartGame();
        }
    }
}
