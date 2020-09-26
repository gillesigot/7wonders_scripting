using UnityEngine;

// Hack: TEMP made for debug purpose
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
