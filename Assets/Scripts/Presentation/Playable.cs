using UnityEngine;

public class Playable : MonoBehaviour
{
    // Used to defint the current building type.
    public Card.CardType buildType;
    // Used to identify the business card represented by this object
    public string id;

    // Workaround to "instanciate" Playable, since "new" cannot be called on monobehaviour.
    public static Playable GetPlayable(string id, Card.CardType buildType)
    {
        GameObject go = new GameObject();
        Playable p = go.AddComponent<Playable>();
        p.id = id;
        p.buildType = buildType;
        return p;
    }
}
