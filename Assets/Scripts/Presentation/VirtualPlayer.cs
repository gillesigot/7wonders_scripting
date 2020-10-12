using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static AIController;

public class VirtualPlayer : MonoBehaviour
{
    public Text NameLabel { get; set; }
    public Text WonderLabel { get; set; }
    public Text CoinLabel { get; set; }
    public Text DefeatLabel { get; set; }
    public Transform JustPlayed { get; set; }
    public GameObject Thumbnail { get; set; }
    public GameObject ThumbnailTitle { get; set; }
    public GameObject ThumbnailIcon { get; set; }
    public GameObject IconValue { get; set; }
    public GameObject LeftCardColumn { get; set; }
    public GameObject RightCardColumn { get; set; }
    public int[] CardsCount { get; set; }

    // Define appropriate icon size within thumbnail.
    private enum IconSize { 
        SMALL = 25,
        REGULAR = 40,
        MEDIUM = 60,
        BIG = 101,
    }

    private void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// Initialize get graphical components references and load needed resources.
    /// </summary>
    public void Initialize()
    {
        foreach (Text child in this.GetComponentsInChildren<Text>())
        {
            switch (child.name)
            {
                case "player_title":
                    this.NameLabel = child;
                    break;
                case "wonder_title":
                    this.WonderLabel = child;
                    break;
                case "coin_text":
                    this.CoinLabel = child;
                    break;
                case "defeat_text":
                    this.DefeatLabel = child;
                    break;
            }
        }

        foreach (Transform child in this.GetComponentInChildren<Transform>())
        {
            switch (child.name)
            {
                case "just_played":
                    this.JustPlayed = child;
                    break;
                case "cards":
                    Transform[] grandChildren = child.GetComponentsInChildren<Transform>();
                    this.LeftCardColumn = grandChildren.Where(c => c.name == "col1").First().gameObject;
                    this.RightCardColumn = grandChildren.Where(c => c.name == "col2").First().gameObject;
                    break;
            }
        }

        this.Thumbnail = Resources.Load("thumbnail") as GameObject;
        this.ThumbnailTitle = Resources.Load("thumbnail_title") as GameObject;
        this.ThumbnailIcon = Resources.Load("thumbnail_icon") as GameObject;
        this.IconValue = Resources.Load("icon_value") as GameObject;

        this.CardsCount = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
    }

    /// <summary>
    /// Display basic player's information.
    /// </summary>
    /// <param name="player">The player information to display.</param>
    public void DisplayPlayerStat(Player player)
    {
        this.NameLabel.text = player.Name;
        Wonder playerWonder = player.WonderManager.Wonder;
        this.WonderLabel.text = playerWonder.Name + " - " + playerWonder.Side;
    }

    /// <summary>
    /// Actualize player's information.
    /// </summary>
    public void RefreshBoard(Player player)
    {
        this.CoinLabel.text = player.Coins.ToString();
        this.DefeatLabel.text = ((player.EastDefeatWarTokens + player.WestDefeatWarTokens) * -1).ToString();
    }

    /// <summary>
    /// Remove previous last move thumbnails.
    /// </summary>
    public void CleanLastMove()
    {
        if (this.JustPlayed.childCount > 0)
        {
            Transform thumbnail = this.JustPlayed.GetComponentsInChildren<Transform>()[1];
            string noRegularBuild = CardColor.white.ToString();
            string trimmedName = thumbnail.name.Split('(')[0];
            string thumbnailTitle = thumbnail.GetChild(0).GetComponent<Text>().text;

            if (noRegularBuild.Contains(trimmedName))
            {
                if (thumbnailTitle.Contains("Discard"))
                    Destroy(thumbnail.gameObject);
                else
                {
                    thumbnail.SetParent(this.RightCardColumn.transform);
                    this.ResizePanel(thumbnail.gameObject, IconSize.BIG, IconSize.REGULAR);
                }
            }
            else
            {
                this.AddToPlayedCards(thumbnail, trimmedName);
                Destroy(thumbnail.GetChild(0).gameObject); // remove thumbnail title
                this.ResizePanel(thumbnail.gameObject, IconSize.BIG, IconSize.REGULAR);
            }
        }
    }

    /// <summary>
    /// Add thumnail to corresponding slot in AI played cards.
    /// </summary>
    /// <param name="thumbnail">The thumbnail to add.</param>
    /// <param name="name">The name of the thumbnail.</param>
    private void AddToPlayedCards(Transform thumbnail, string name)
    {
        int thumbnailType = (int)Enum.Parse(typeof(CardColor), name, true);
        
        if (thumbnailType < 4)
            thumbnail.SetParent(this.LeftCardColumn.transform);
        else
            thumbnail.SetParent(this.RightCardColumn.transform);

        int cardIndex = 0;
        int startingIndex = (thumbnailType < 4) ? 0 : 4;

        for (int i = startingIndex; i <= thumbnailType; i++)
            cardIndex += this.CardsCount[i];

        thumbnail.SetSiblingIndex(cardIndex);
        this.CardsCount[thumbnailType]++;
    }

    /// <summary>
    /// Display last move performed by the player.
    /// </summary>
    /// <param name="cardName">Name of the last card played.</param>
    /// <param name="cardColor">Last card color.</param>
    /// <param name="cardSymbols">Last card symbols.</param>
    /// <param name="symbolsValue">The value attached to last card symbols.</param>
    public void SetLastMove(string cardName, string cardColor, string[] cardSymbols, int[] symbolsValue)
    {
        this.CleanLastMove();

        int MAX_SIZE_LABEL = 15;
        cardName = (cardName.Length <= MAX_SIZE_LABEL) ? cardName : cardName.Substring(0, MAX_SIZE_LABEL) + "...";

        this.AddThumbnail(this.JustPlayed, cardColor, cardName, cardSymbols, symbolsValue);
    }

    /// <summary>
    /// Create and add a thumbnail representing a played card.
    /// </summary>
    /// <param name="parentPanel">The panel where the thumbnail should be added.</param>
    /// <param name="cardColor">The background color of the card.</param>
    /// <param name="freeText">Free text to display in thumbnail.</param>
    /// <param name="symbols">The card symbols that should be added.</param>
    /// <param name="symbolsValue">The value attached to symbols to display.</param>
    public void AddThumbnail(
        Transform parentPanel, 
        string cardColor, 
        string freeText = null, 
        string[] symbols = null, 
        int[] symbolsValue = null
        )
    {
        Sprite lastMoveBackground = Resources.Load<Sprite>("background/" + cardColor);
        Image thumbnailBackgound = this.Thumbnail.GetComponent<Image>();
        thumbnailBackgound.sprite = lastMoveBackground;
        this.Thumbnail.name = cardColor;  // save card type

        GameObject newThumbnail = Instantiate(this.Thumbnail, parentPanel);

        if (freeText != null)
        {
            Text t_title = this.ThumbnailTitle.GetComponent<Text>();
            t_title.text = freeText;

            Instantiate(this.ThumbnailTitle, newThumbnail.transform);
        }

        if (symbols != null)
        {
            for (int i = 0; i < symbols.Length; i ++)
            {
                if (symbols[i].StartsWith("BIG"))
                    this.ResizePanel(this.ThumbnailIcon, IconSize.BIG, IconSize.SMALL);
                else if (symbols[i].StartsWith("MEDIUM"))
                    this.ResizePanel(this.ThumbnailIcon, IconSize.MEDIUM, IconSize.SMALL);
                else
                    this.ResizePanel(this.ThumbnailIcon, IconSize.SMALL, IconSize.SMALL);

                Sprite symbolRepresentation = Resources.Load<Sprite>("icons/" + symbols[i]);
                Image iconBackground = this.ThumbnailIcon.GetComponent<Image>();
                iconBackground.sprite = symbolRepresentation;

                GameObject newIcon = Instantiate(this.ThumbnailIcon, newThumbnail.transform);

                if (symbolsValue != null && i < symbolsValue.Length)
                {
                    Text t_title = this.IconValue.GetComponent<Text>();
                    t_title.text = symbolsValue[i].ToString();

                    Instantiate(this.IconValue, newIcon.transform);
                }
            }
        }
    }

    /// <summary>
    /// Resize a given gameobject to predefined dimensions.
    /// </summary>
    /// <param name="panel">The panel to resize.</param>
    /// <param name="x">The width to apply.</param>
    /// <param name="y">The height to apply.</param>
    private void ResizePanel(GameObject panel, IconSize x, IconSize y)
    {
        RectTransform panel_rt = panel.GetComponent<RectTransform>();
        panel_rt.sizeDelta = new Vector2((int)x, (int)y);
    }
}
