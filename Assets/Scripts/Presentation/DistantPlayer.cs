using UnityEngine;
using UnityEngine.UI;

public class DistantPlayer : MonoBehaviour
{
    /// <summary>
    /// Add distant player to main GUI.
    /// </summary>
    /// <param name="player">The distant player to be added.</param>
    /// <returns>The particular distant player's GUI.</returns>
    public VirtualPlayer AddPlayer(Player player)
    {
        GameObject playerGUI = Resources.Load("distant_player") as GameObject;
        GameObject playerOverview = Instantiate(playerGUI, this.transform.root);

        GameObject playerIcon = Resources.Load("player") as GameObject;
        Text playerLabel = playerIcon.GetComponentInChildren<Text>();
        playerLabel.text = player.Name;

        GameObject distantPlayerIcon = Instantiate(playerIcon, this.transform);
        PlayerOverview po = distantPlayerIcon.GetComponent<PlayerOverview>();
        po.Overview = playerOverview;

        VirtualPlayer virtualPlayer = playerOverview.GetComponentInChildren<VirtualPlayer>();
        virtualPlayer.Initialize();
        return virtualPlayer;
    }
}
