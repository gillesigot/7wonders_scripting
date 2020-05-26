using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// This class represents cards information store in data files.
/// </summary>
public class CardDAO
{
    public string Name { get; set; }
}

public static class CardsDAO
{
    // The file path where the cards are stored
    private const string FILE_PATH = @"..\7Wonders\Assets\Data\cards.json";

    /// <summary>
    /// Retrieve all stored cards and returns them in as card list.
    /// </summary>
    /// <returns>All stored cards as list of Card objects.</returns>
    public static List<Card> GetValues()
    {
        List<Card> cards = new List<Card>();
        if (!File.Exists(FILE_PATH))
            return cards;

        string jsonValues = File.ReadAllText(FILE_PATH);
        List<CardDAO> cardsDAO = JsonConvert.DeserializeObject<List<CardDAO>>(jsonValues);

        foreach (CardDAO cardDAO in cardsDAO)
        {
            Card card = new CivilCard
            (
                "ID",
                cardDAO.Name,
                Card.CardType.CIVIL,
                new Card.BuildCondition(),
                new string[] { },
                3,
                1
            );
            cards.Add(card);
        }
        return cards;
    }
}
