using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardPile : MonoBehaviour
{
    private const int CARD_VALUE = 3;

    /// <summary>
    /// Deactivate a card and credit the player of CARD_VALUE.
    /// </summary>
    /// <param name="card">The card to discard.</param>
    public void Discard(GameObject card)
    {
        card.transform.position = this.transform.position;
        card.SetActive(false);

        Player player = GameObject.Find("player").GetComponent<Player>();
        player.updateCoinAmount(CARD_VALUE);
    }
}
