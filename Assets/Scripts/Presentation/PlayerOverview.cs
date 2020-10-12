using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerOverview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // The overview panel linked to toggle when pointer enter/exit.
    public GameObject Overview { get; set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.Overview.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.Overview.SetActive(false);
    }
}
