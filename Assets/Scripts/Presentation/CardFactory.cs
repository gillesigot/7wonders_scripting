using UnityEngine;
using UnityEngine.UI;

public class CardFactory : MonoBehaviour
{
    // The prefab used to instantiate a card representation.
    private GameObject CardPrefab { get; set; }

    private void Awake()
    {
        CardPrefab = Resources.Load("card") as GameObject;
    }
    /// <summary>
    /// Generate a new card representation and add it to player's hand.
    /// </summary>
    /// <param name="card">The card object to represent.</param>
    public void CreateCard(Card card)
    {
        Playable playable = CardPrefab.GetComponent<Playable>();
        playable.id = card.ID;
        playable.buildType = card.Type;

        Image image = CardPrefab.GetComponent<Image>();
        Sprite cardFront = Resources.Load<Sprite>("cards/" + card.ID);
        image.sprite = cardFront;

        Instantiate(CardPrefab, this.transform);
    }

    /// <summary>
    /// Remove all remaining card in player's hand.
    /// </summary>
    public void CleanHand()
    {
        for (int i = 0; i < this.transform.childCount; i++)
            Destroy(this.transform.GetChild(i).gameObject);
    }

    /// <summary>
    /// Return the number of cards remaining in player's hand.
    /// </summary>
    public void DiscardLastCard()
    {
        Transform discardPile = GameObject.Find("discard_pile").transform;
        GameObject card = this.transform.GetChild(0).gameObject;
        card.transform.position = discardPile.position;
        card.SetActive(false);
    }
}
