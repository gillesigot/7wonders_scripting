using UnityEngine;

public class DefeatTokenBoard : MonoBehaviour
{
    // The prefab used to instantiate a defeat token representation.
    private GameObject TokenPrefab { get; set; }

    private void Awake()
    {
        TokenPrefab = Resources.Load("defeat_token") as GameObject;
    }

    /// <summary>
    /// Add a token to the board.
    /// </summary>
    public void AddToken()
    {
        Instantiate(TokenPrefab, this.transform);
    }

    /// <summary>
    /// Clean the token board from all tokens.
    /// </summary>
    public void CleanBoard()
    {
        foreach (Transform token in this.transform.GetComponentInChildren<Transform>())
            Destroy(token.gameObject);
    }
}
