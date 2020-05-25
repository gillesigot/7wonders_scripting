using UnityEngine;
using UnityEngine.UI;

// TODO Decouple from event/gameobject logic (remove MonoBehavior)
public class Player : MonoBehaviour
{
    // Amount of coins owned by the player
    private int coins = 3;
    // public int war = 0;

    /// <summary>
    /// Update the player's coin amount.
    /// </summary>
    /// <param name="amount">The amount of coins to add (set negative to remove).</param>
    public void updateCoinAmount(int amount)
    {
        this.coins += amount;
        Text coin_text = GameObject.Find("coin_text").GetComponent<Text>();
        coin_text.text = this.coins.ToString();
    }
}
