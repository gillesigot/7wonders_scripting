using UnityEngine;

public class ToggleScores : MonoBehaviour
{
    public GameObject score_board;
    public void TogglePanel()
    {
        if (score_board.activeSelf)
            score_board.SetActive(false);
        else
            score_board.SetActive(true);
    }
}
