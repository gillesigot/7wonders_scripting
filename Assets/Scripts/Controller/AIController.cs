using System.Collections.Generic;
using System.Linq;

public class AIController
{
    public VirtualPlayer PlayerBoard { get; set; }
    public Player Player { get; set; }

    public enum CardColor { 
        brown = 0,
        grey = 1,
        red = 2,
        blue = 3,
        yellow = 4,
        green = 5,
        purple = 6,
        white = 7,
    }

    public AIController(VirtualPlayer vp)
    {
        this.PlayerBoard = vp;
    }

    /// <summary>
    /// Initialize the board representation of a virtual player.
    /// </summary>
    /// <param name="player">The linked virtual player.</param>
    public void InitializeAIBoard(Player player)
    {
        this.Player = player;
        this.PlayerBoard.DisplayPlayerStat(this.Player);
    }

    /// <summary>
    /// Refresh the board representation of a virtual player.
    /// </summary>
    public void RefreshBoard()
    {
        this.PlayerBoard.RefreshBoard(this.Player);
    }

    /// <summary>
    /// Determine the color of a given card.
    /// </summary>
    /// <param name="card">The card to identify.</param>
    /// <returns>The card color.</returns>
    private CardColor GetCardColor(Card card)
    {
        CardColor cc = CardColor.white;

        if (card.Type == Card.CardType.RESOURCE)
            if (CityManager.RAW_RESOURCES.Where(rr => rr == ((ResourceCard)card).Resources.First().Type).Count() > 0)
                cc = CardColor.brown;
            else
                cc = CardColor.grey;
        else
            cc = ((CardColor)(int)card.Type + 1);

        return cc;
    }

    /// <summary>
    /// Get the list of symbols names present on given card.
    /// </summary>
    /// <param name="card">The card to analyze.</param>
    /// <returns>The list of symbols present on the card.</returns>
    private string[] GetSymbolNames(Card card)
    {
        List<string> symbolNames = new List<string>();
        
        switch (card.Type)
        {
            case Card.CardType.RESOURCE:
                ResourceCard rc = ((ResourceCard)card);
                foreach (Card.ResourceQuantity rq in rc.Resources) 
                    for (int i = 0; i < rq.Quantity; i++)
                        symbolNames.Add(rq.Type.ToString());
                break;
            case Card.CardType.WAR:
                for (int i = 0; i < ((WarCard)card).WarPoints; i++)
                    symbolNames.Add(card.Type.ToString());
                break;
            case Card.CardType.CIVIL:
                symbolNames.Add(card.Type.ToString());
                break;
            case Card.CardType.COMMERCIAL:
                switch (card)
                {
                    case ResourceCard rec:
                        symbolNames.AddRange(rec.Resources.Select(c => c.Type.ToString()));
                        break;
                    case BonusCard boc:
                        if (boc.Bonus == BonusCard.BonusType.WONDER_BONUS)
                            symbolNames.Add("MEDIUM_WONDER_BONUS");
                        else
                        {
                            if (!(boc.CheckLeft && boc.CheckRight))
                            {
                                if (boc.BonusCardType.Length == 0)
                                    symbolNames.Add("coin");
                                else if (boc.BonusCardType[0] == Card.CardType.COMMERCIAL)
                                    symbolNames.Add("MEDIUM_COMMERCIAL_BONUS");
                                else if (boc.BonusCardType[0] == Card.CardType.RESOURCE)
                                {
                                    if (boc.ResourceKind == BonusCard.ResourceMetaType.RAW)
                                        symbolNames.Add("MEDIUM_RAW_BONUS");
                                    else
                                        symbolNames.Add("MEDIUM_MANUFACTURED_BONUS");
                                }
                            }
                            else
                            {
                                if (boc.ResourceKind == BonusCard.ResourceMetaType.RAW)
                                    symbolNames.Add("BIG_RAW_BONUS_COINS");
                                else
                                    symbolNames.Add("BIG_MANUFACTURED_BONUS_COINS");
                            }
                        }
                        break;
                    case CommercialCard cc:
                        string comSymbol = "";
                        if (CityManager.RAW_RESOURCES.Where(rr => rr == cc.Resources.First().Type).Count() > 0)
                        {
                            comSymbol += "BIG_RAW";
                            if (cc.LeftPlayer)
                                comSymbol += "_LEFT";
                            else
                                comSymbol += "_RIGHT";
                        }
                        else
                            comSymbol += "BIG_MANUFACTURED";
                        symbolNames.Add(comSymbol);
                        break;
                }
                break;
            case Card.CardType.SCIENCE:
                symbolNames.Add(((ScienceCard)card).ScienceCardType.ToString());
                break;
            case Card.CardType.GUILD:
                BonusCard bc = (BonusCard)card;
                switch (bc.Bonus)
                {
                    case BonusCard.BonusType.CARD_BONUS:
                        if (bc.BonusCardType.Length > 1)
                            symbolNames.Add("BIG_GUI_3_CARDS");
                        else
                            if (bc.BonusCardType[0] == Card.CardType.RESOURCE)
                                if (bc.ResourceKind == BonusCard.ResourceMetaType.RAW)
                                    symbolNames.Add("BIG_GUI_RAW");
                                else
                                    symbolNames.Add("BIG_GUI_MANUFACTURED");
                            else
                                symbolNames.Add("BIG_GUI_" + bc.BonusCardType[0].ToString());
                        break;
                    case BonusCard.BonusType.DEFEAT_BONUS:
                        symbolNames.Add("BIG_GUI_DEFEAT");
                        break;
                    case BonusCard.BonusType.SCIENCE_BONUS:
                        foreach (Card.ScienceType science_symbol in System.Enum.GetValues(typeof(Card.ScienceType)))
                            symbolNames.Add(science_symbol.ToString());
                        break;
                    case BonusCard.BonusType.WONDER_BONUS:
                        symbolNames.Add("BIG_GUI_WONDER");
                        break;
                }
                break;
        }

        return symbolNames.ToArray();
    }

    /// <summary>
    /// Get the list of symbols names present on given wonder step.
    /// </summary>
    /// <param name="step">The step to analyze.</param>
    /// <returns>The list of symbols present on the step.</returns>
    private string[] GetSymbolNames(Step step)
    {
        List<string> symbolNames = new List<string>();

        foreach (Step.StepType type in step.Types)
        {
            switch (type)
            {
                case Step.StepType.BONUS:
                    foreach (BonusCard.RewardQuantity rq in step.Reward)
                        symbolNames.Add((rq.Reward == BonusCard.RewardType.GOLD) ? "coin" : "CIVIL");
                    break;
                case Step.StepType.WAR:
                    for (int i = 0; i < step.WarPoints; i++)
                        symbolNames.Add(Step.StepType.WAR.ToString());
                    break;
                case Step.StepType.SCIENCE:
                    foreach (Card.ScienceType science_symbol in System.Enum.GetValues(typeof(Card.ScienceType)))
                        symbolNames.Add(science_symbol.ToString());
                    break;
                case Step.StepType.COMMERCIAL:
                    if (step.CommercialType == Step.AcquisitionType.PRODUCTION)
                    {
                        if (step.ResourceMetaType == BonusCard.ResourceMetaType.MANUFACTURED)
                            symbolNames.AddRange(new string[] { "GLASS", "LOOM", "PAPYRUS" });
                        else
                            symbolNames.AddRange(new string[] { "WOOD", "STONE", "ORE", "CLAY" });
                    }
                    else
                        symbolNames.Add("BIG_RAW_BOTH");
                    break;
                case Step.StepType.GUILD:
                    symbolNames.Add("BIG_GUI_CHOICE");
                    break;
                case Step.StepType.BUILDER:
                    symbolNames.Add("MEDIUM_" + step.Builder.ToString());
                    break;
            }
        }
        return symbolNames.ToArray();
    }

    /// <summary>
    /// Get the list of values present on a given card.
    /// </summary>
    /// <param name="card">The card to analyze.</param>
    /// <returns>The list of values present on the card.</returns>
    private int[] GetSymbolsValue(Card card)
    {
        List<int> symbolsValue = new List<int>();

        if (card.Type == Card.CardType.CIVIL)
        {
            symbolsValue.Add(((CivilCard)card).VictoryPoints);
        }
        else if (card.Type == Card.CardType.COMMERCIAL && card is BonusCard)
        {
            BonusCard bc = (BonusCard)card;
            if (bc.Bonus == BonusCard.BonusType.FREE_BONUS)
                symbolsValue.Add(bc.Reward[0].Quantity);
        }

        return symbolsValue.ToArray();
    }

    /// <summary>
    /// Get the list of values present on a given wonder step.
    /// </summary>
    /// <param name="step">The step to analyze.</param>
    /// <returns>The list of values present on the step.</returns>
    private int[] GetSymbolsValue(Step step)
    {
        List<int> symbolsValue = new List<int>();

        foreach (Step.StepType type in step.Types)
            if (type == Step.StepType.BONUS)
                symbolsValue.AddRange(step.Reward.Select(r => r.Quantity));

        return symbolsValue.ToArray();
    }

    /// <summary>
    /// Display the last move played on the board.
    /// </summary>
    /// <param name="move">The last move.</param>
    public void SetLastMove(AIManager.Choice move)
    {
        string cardName = "Discard";
        CardColor cardColor = CardColor.white;
        string[] symbols = null;
        int[] symbolsValue = null;
        if (move.Action == AIManager.Action.BUILD_CITY)
        {
            cardName = move.CardToPlay.Name;
            cardColor = this.GetCardColor(move.CardToPlay);
            symbols = this.GetSymbolNames(move.CardToPlay);
            symbolsValue = this.GetSymbolsValue(move.CardToPlay);
        }
        else if (move.Action == AIManager.Action.BUILD_WONDER)
        {
            cardName = "Wonder";
            Step builtStep = this.Player.WonderManager.GetPreviousStep();
            if (builtStep != null)
            {
                symbols = this.GetSymbolNames(builtStep);
                symbolsValue = this.GetSymbolsValue(builtStep);
            }
        }
        this.PlayerBoard.SetLastMove(cardName, cardColor.ToString(), symbols, symbolsValue);
    }

    /// <summary>
    /// Clean last move representation and add it to AI cards list.
    /// </summary>
    public void CleanLastMove()
    {
        this.PlayerBoard.CleanLastMove();
    }
}
