using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class AgentManager : Agent
{
    public string AgentName;
    public List<Card> Hand = new List<Card>();
    public int VictoryPoints { get; set; }
    public int Coins { get; set; }

    public override void OnEpisodeBegin()
    {
        this.VictoryPoints = 0;
        this.Coins = 3;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // TODO
        // All cards
        // Player left, player right
        // What players have played
        // Players coins
    }
 
    public override void Heuristic(float[] actionsOut)
    {
        // TODO
    }

    // Reduce card options as they are played.
    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        List<int> actionIndices = new List<int>();
        for (int i = 0; i < 7; i++)
            if (i >= this.Hand.Count)
                actionIndices.Add(i);

        actionMasker.SetMask(1, actionIndices.ToArray());
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // TODO
        // Receives 2 integers (discrete actions): 1 for build/discard, second for which card to use
    }

    public void Play()
    {
        // TODO
        // Request decision for discarding or playing.
        // If building, request decision for which card to build.
        //     Add it to the player's tabletop
        // If discarding
        //     Remove the card from the hand and give 3 coins


        // This is asking for a new decision from the agent, then call the RequestAction, which will then trigger OnActionReceived.
        RequestDecision();
    }

    public void EndGame()
    {
        // TODO
        // Count score and give reward to winner, negative reward for losers
        EndEpisode();
    }
}
