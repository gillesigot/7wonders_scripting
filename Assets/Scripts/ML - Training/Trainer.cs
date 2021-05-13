using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Trainer : MonoBehaviour
{
    public List<Card> Cards = new List<Card>();
    public Dictionary<string, List<Card>> TableTop = new Dictionary<string, List<Card>>();
    public AgentManager[] AllPlayers { get; set; }
    public List<AgentManager> GameSchedule { get; set; }

    // Initialize all vars and start a new game.
    public void StartGame()
    {
        // Get all cards needed for this training.
        if (this.Cards.Count == 0)
        {
            // Get all CIVIL cards from all ages.
            List<Card> AllCards = CardsDAO.GetCards(3);
            foreach (Card card in AllCards)
                if (card.Type == Card.CardType.CIVIL)
                    this.Cards.Add(card);
        }

        // Get all players and shuffle play order.
        if (this.AllPlayers == null)
        {
            this.AllPlayers = this.GetComponentsInChildren<AgentManager>();
            this.GameSchedule = new List<AgentManager>(this.AllPlayers.ToList());
            ListManagement.Shuffle(this.GameSchedule);
        }

        // Clear all 3 players tabletop.
        foreach (AgentManager player in this.AllPlayers)
            this.TableTop.Add(player.AgentName, new List<Card>());

        // Shuffle & give cards.
        ListManagement.Shuffle(this.Cards);
        List<Card> remainingCards = this.Cards.ToList();
        // Distribute all cards amongst players (fill with the first card if uneven amount).
        while (remainingCards.Count > 0)
        {
            foreach (AgentManager player in this.AllPlayers)
            {
                if (remainingCards.Count > 0)
                {
                    player.Hand.Add(remainingCards.First());
                    remainingCards.RemoveAt(0);
                }
                else
                    player.Hand.Add(this.Cards.First());
            }
        }

        Debug.Log("Start of the game.");
        this.TurnScheduler();
    }

    // End the current game and prepare for next one.
    public void EndGame()
    {
        // Game is over.
        Debug.Log("End of the game.");
        // Starting a new one.
        // this.StartGame();
    }

    // Perform each turn until the end of the game.
    public void TurnScheduler()
    {
        // TODO
        this.EndGame();
    }
}

// Short utility class for list management.
public static class ListManagement
{
    // Shuffle a list of any type.
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
