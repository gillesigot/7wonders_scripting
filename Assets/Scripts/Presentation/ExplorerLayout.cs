using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExplorerLayout : MonoBehaviour
{
    public GameObject exploreBoard;
    private HorizontalLayoutGroup Layout { get; set; }
    private GameObject CardPrefab { get; set; }

    void Awake()
    {
        this.Layout = exploreBoard.GetComponent<HorizontalLayoutGroup>();
        this.CardPrefab = Resources.Load("card") as GameObject;
    }

    /// <summary>
    /// Hide/show explorer panel.
    /// </summary>
    /// <param name="show">True if it should be displayed.</param>
    public void ToggleExplorer(bool show)
    {
        exploreBoard.SetActive(show);
    }

    /// <summary>
    /// Load cards in the card explorer and set the layout accordingly.
    /// </summary>
    /// <param name="cards">The list of cards to load.</param>
    public void ExploreCards(List<Playable> cards)
    {
        for (int i = 0; i < exploreBoard.transform.childCount; i++)
            Destroy(exploreBoard.transform.GetChild(i).gameObject);

        foreach (Playable card in cards)
        {
            Playable playable = CardPrefab.GetComponent<Playable>();
            playable.id = card.id;
            playable.buildType = card.buildType;

            Draggable draggable = CardPrefab.GetComponent<Draggable>();
            draggable.clickable = true;

            Image image = CardPrefab.GetComponent<Image>();
            Sprite cardFront = Resources.Load<Sprite>("cards/" + card.id);
            image.sprite = cardFront;

            Instantiate(CardPrefab, exploreBoard.transform);
        }

        if (cards.Count <= 7)
            this.Layout.spacing = 9;
        else
            this.Layout.spacing = cards.Count * -4f;
    }
}
