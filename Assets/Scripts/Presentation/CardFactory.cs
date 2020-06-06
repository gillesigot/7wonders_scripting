using UnityEngine;
using UnityEngine.UI;

public class CardFactory : MonoBehaviour
{
    // The prefab used to instantiate a card representation.
    private static GameObject CardPrefab { get; set; }
    // The panel displaying the player's hand.
    private static Transform Hand { get; set; }

    private void Awake()
    {
        Hand = GameObject.Find("hand").transform;
        CardPrefab = Resources.Load("card") as GameObject;
    }

    /// <summary>
    /// Generate a new card representation and add it to player's hand.
    /// </summary>
    /// <param name="card">The card object to represent.</param>
    public static void CreateCard(Card card)
    {
        Playable playable = CardPrefab.GetComponent<Playable>();
        playable.id = card.ID;
        playable.buildType = card.Type;

        Image image = CardPrefab.GetComponent<Image>();
        Sprite cardFront = Resources.Load<Sprite>("cards/" + card.ID);
        image.sprite = cardFront;

        Instantiate(CardPrefab, Hand);
    }
}
