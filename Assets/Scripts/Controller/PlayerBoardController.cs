﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Card;

public class PlayerBoardController
{
    // Represent the trade location.
    public enum TradeLocation
    {
        EAST,
        WEST
    }
    // Used to represent the human player interacting with the board.
    public static Player Player { get; set; }
    // Used to store the UI component that display the coins amount.
    public static Text Coins { get; set; }
    // Used to store the UI component that display the war points.
    public static Text WarPoints { get; set; }
    // Used to store the player's cards representation.
    public static Transform Hand { get; set; }
    // Used to create the player's cards representation.
    public static CardFactory CardFactory { get; set; }
    // Used to store left discard pile representation.
    public static Image LeftDiscardPile { get; set; }
    // Used to store right discard pile representation.
    public static Image RightDiscardPile { get; set; }
    // Used to create east traded resources representation.
    public static TradeBoard RightTradeBoard { get; set; }
    // Used to create west defeat tokens representation.
    public static DefeatTokenBoard LeftDefeatTokenBoard { get; set; }
    // Used to create east defeat tokens representation.
    public static DefeatTokenBoard RightDefeatTokenBoard { get; set; }
    // Used to display end game scores.
    public static ScoreBoard ScoreBoard { get; set; }
    // Used to setup wonder layout.
    public static WonderLayout WonderLayout { get; set; }

    public PlayerBoardController()
    {
        Player = GameManager.Instance().GetHumanPlayer();
        Coins = GameObject.Find("coin_text").GetComponent<Text>();
        WarPoints = GameObject.Find("war_text").GetComponent<Text>();
        Hand = GameObject.Find("hand").GetComponent<Transform>();
        CardFactory = GameObject.Find("hand").GetComponent<CardFactory>();
        LeftDiscardPile = GameObject.Find("discard_left").GetComponent<Image>();
        RightDiscardPile = GameObject.Find("discard_right").GetComponent<Image>();
        RightTradeBoard = GameObject.Find("trade_board_right").GetComponent<TradeBoard>();
        LeftDefeatTokenBoard = GameObject.Find("defeat_left").GetComponent<DefeatTokenBoard>();
        RightDefeatTokenBoard = GameObject.Find("defeat_right").GetComponent<DefeatTokenBoard>();
        ScoreBoard = GameObject.Find("score_board").GetComponent<ScoreBoard>();
        WonderLayout = GameObject.Find("wonder_layout").GetComponent<WonderLayout>();
    }

    #region Player's display

    /// <summary>
    /// Refresh the coin amount on the player board.
    /// </summary>
    public static void RefreshCoinAmount()
    {
        Coins.text = Player.Coins.ToString();
    }

    /// <summary>
    /// Refresh the war points count on the player board.
    /// </summary>
    public static void RefreshWarPoints()
    {
        WarPoints.text = Player.VictoryWarPoints.ToString();

        // Manage defeat tokens
        LeftDefeatTokenBoard.CleanBoard();
        RightDefeatTokenBoard.CleanBoard();

        for (int i = 0; i < Player.WestDefeatWarTokens; i++)
            LeftDefeatTokenBoard.AddToken();
        for (int i = 0; i < Player.EastDefeatWarTokens; i++)
            RightDefeatTokenBoard.AddToken();
    }

    /// <summary>
    /// Display the discard piles according to the current age.
    /// </summary>
    /// <param name="age">The current age of the game.</param>
    public static void RefreshDiscardPiles(int age)
    {
        LeftDiscardPile.sprite = Resources.Load<Sprite>("discard_pile_" + age);
        RightDiscardPile.sprite = Resources.Load<Sprite>("discard_pile_" + age);
    }

    /// <summary>
    /// Display the board with all players scores.
    /// </summary>
    /// <param name="players">All current game players.</param>
    public static void DisplayScoreBoard(List<Player> players)
    {
        ScoreBoard.Display(players);
    }

    /// <summary>
    /// Display the wonder board according to the one attributed to the player.
    /// </summary>
    /// <param name="wonderID">The wonder's ID to display.</param>
    /// <param name="nbSteps">The number of wonder buildable steps.</param>
    public static void RefreshWonderBoard(string wonderID, int nbSteps)
    {
        Image wonderBoard = GameObject.Find("wonder_board").GetComponent<Image>();
        Sprite wonderImage = Resources.Load<Sprite>("wonders/" + wonderID);
        wonderBoard.sprite = wonderImage;
        WonderLayout.SetWonderLayout(nbSteps);
    }

    #endregion

    #region Hand management

    /// <summary>
    /// Refresh the cards display in player's hand.
    /// </summary>
    public static void RefreshHand()
    {
        CardFactory.CleanHand();
        foreach (Card card in Player.Hand)
            CardFactory.CreateCard(card);
    }

    /// <summary>
    /// Refresh the cards display in player's hand.
    /// </summary>
    public static void DiscardLastCard()
    {
        CardFactory.DiscardLastCard();
    }

    #endregion

    #region Trade management

    /// <summary>
    /// Activate the trade panel and set it up to trade with a given player.
    /// </summary>
    /// <param name="tradePanel">The related trade panel.</param>
    /// <param name="location">The player you want to trade with.</param>
    /// <param name="resLabels">All panel resource labels.</param>
    public static void InitTradePanel(GameObject tradePanel, TradeLocation location, List<Text> resLabels, Text totalLabel)
    {
        // Hack: TEMP use location to know which information to display
        const char SPLIT_CHAR = ':';
        if (Player.City.TradeResources.Count > 0)
        {
            int nbRes = 0;
            foreach (Text label in resLabels)
            {
                string[] splittedLabel = label.text.Split(SPLIT_CHAR);
                ResourceType resourceType = (ResourceType)Enum.Parse(typeof(ResourceType), splittedLabel[0]);
                if (Player.City.TradeResources.ContainsKey(resourceType))
                {
                    label.text = splittedLabel[0] + SPLIT_CHAR + " " + Player.City.TradeResources[resourceType];
                    nbRes += Player.City.TradeResources[resourceType];
                }
            }
        }
        tradePanel.SetActive(true);
    }

    /// <summary>
    /// Deactivate the trade panel and apply linked actions whether there was a deal or not.
    /// </summary>
    /// <param name="tradePanel">The related trade panel.</param>
    /// <param name="deal">If a deal was concluded or not.</param>
    /// <param name="resLabels">All resource labels from trade panel.</param>
    public static void CloseTradePanel(GameObject tradePanel, bool deal, List<Text> resLabels)
    {
        if (deal)
        {
            Dictionary<ResourceType, int> resWanted = GetAllResourcesToTrade(resLabels);
            Player.City.BuyResources(resWanted);
            PlayerBoardController.RefreshCoinAmount();
            tradePanel.SetActive(false);

            // Hack: TEMP update right bar only
            RightTradeBoard.CleanBoard();
            foreach (KeyValuePair<ResourceType, int> resource in resWanted)
                for(int i = 0; i < resource.Value; i++)
                    RightTradeBoard.AddResource(resource.Key);
        }
        else
            tradePanel.SetActive(false);
    }

    /// <summary>
    /// Clean all trade boards (i.e. at the end of a turn).
    /// </summary>
    public static void CleanTradeBoards()
    {
        // LeftTradBoard.CleanBoard();
        RightTradeBoard.CleanBoard();
    }

    /// <summary>
    /// Update the amount wanted for a given resource.
    /// </summary>
    /// <param name="label">The label displaying the amount.</param>
    /// <param name="value">The value of the increment.</param>
    /// <param name="totalLabel">The label where the total cost will be displayed.</param>
    /// <param name="resLabels">List of all labels displaying the amount.</param>
    public static void UpdateTradeResourceAmount(Text label, int value, Text totalLabel, List<Text> resLabels)
    {
        const char SPLIT_CHAR = ':';
        string[] splittedLabel = label.text.Split(SPLIT_CHAR);
        ResourceType resourceType = (ResourceType)Enum.Parse(typeof(ResourceType), splittedLabel[0]);
        int resourceQuantity = Int32.Parse(splittedLabel[1]) + value;

        string[] splittedCost = totalLabel.text.Split(SPLIT_CHAR);
        int totalCost = Int32.Parse(splittedCost[1]);

        if (IsAvailableForTrade(resourceType, resourceQuantity, resLabels) && resourceQuantity >= 0)  
        {
            // Hack: TEMP only for east trading
            const int RAW = 0, MANUFACTURED = 1;
            if (CityManager.RAW_RESOURCES.Contains(resourceType))
                totalCost += (value * Player.City.EastTradePrice[RAW]);
            else
                totalCost += (value * Player.City.EastTradePrice[MANUFACTURED]);

            if (Player.Coins >= totalCost)
            {
                label.text = splittedLabel[0] + SPLIT_CHAR + " " + resourceQuantity;
                totalLabel.text = splittedCost[0] + SPLIT_CHAR + " " + totalCost;
            }
        }
    }

    /// <summary>
    /// Tell whether a resource is available to buy or not.
    /// </summary>
    /// <param name="resource">The resource type wanted.</param>
    /// <param name="quantity">The resource quantity wanted.</param>
    /// <param name="resLabels">List of all labels displaying the amount.</param>
    /// <returns>The resource availability.</returns>
    private static bool IsAvailableForTrade(ResourceType type, int quantity, List<Text> resLabels)
    {
        // Hack: TEMP GetBuyableResources (EAST or WEST)
        List<Dictionary<ResourceType, int>> resAvailable = new List<Dictionary<ResourceType, int>>(
            new Dictionary<ResourceType, int>[] {
                new Dictionary<ResourceType, int>{
                    { ResourceType.CLAY, 1},
                    { ResourceType.ORE, 1},
                    { ResourceType.STONE, 1},
                    { ResourceType.WOOD, 1},
                    { ResourceType.GLASS, 1},
                    { ResourceType.LOOM, 1},
                    { ResourceType.PAPYRUS, 1}
                },
            });

        Dictionary<ResourceType, int> resWanted = GetAllResourcesToTrade(resLabels);
        if (resWanted.ContainsKey(type))
            resWanted[type] = quantity;
        else
            resWanted.Add(type, quantity);

        foreach (Dictionary<ResourceType, int> resDict in resAvailable)
            if (resWanted.All(x => resDict.ContainsKey(x.Key) && resDict[x.Key] >= x.Value))
                return true;
        return false;
    }

    /// <summary>
    /// Gather all resource types and quantity associated in the trade panel.
    /// </summary>
    /// <param name="resLabels">The list of trad panel labels.</param>
    /// <returns>Dict of resource types and quantity associated.</returns>
    private static Dictionary<ResourceType, int> GetAllResourcesToTrade(List<Text> resLabels)
    {
        const char SPLIT_CHAR = ':';
        Dictionary<ResourceType, int> resWanted = new Dictionary<ResourceType, int>();
        foreach (Text label in resLabels)
        {
            string[] resInfo = label.text.Split(SPLIT_CHAR);
            ResourceType wantedType = (ResourceType)Enum.Parse(typeof(ResourceType), resInfo[0]);
            int wantedQuantity = Int32.Parse(resInfo[1]);
            resWanted.Add(wantedType, wantedQuantity);
        }
        return resWanted;
    }

    #endregion
}
