using UnityEngine;
using UnityEngine.UI;

public class TradeBoard : MonoBehaviour
{
    // The prefab used to instantiate a traded resource representation.
    private GameObject ResPrefab { get; set; }

    private void Awake()
    {
        ResPrefab = Resources.Load("res_icon") as GameObject;
    }

    /// <summary>
    /// Add a resource to the trade board.
    /// </summary>
    /// <param name="resourceType">The type of resource to add.</param>
    public void AddResource(Card.ResourceType resourceType) 
    {
        Image image = ResPrefab.GetComponent<Image>();
        Sprite resIcon = Resources.Load<Sprite>("icons/" + resourceType.ToString());
        image.sprite = resIcon;

        Instantiate(ResPrefab, this.transform);
    }

    /// <summary>
    /// Clean the trade board from all resources.
    /// </summary>
    public void CleanBoard()
    {
        foreach (Transform resource in this.transform.GetComponentInChildren<Transform>())
            Destroy(resource.gameObject);
    }
}
