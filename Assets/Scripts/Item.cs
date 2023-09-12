using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class Item : MonoBehaviour, ISelectHandler, IPointerEnterHandler
{
    public Text label;
    public UnityEvent onSelected = new();

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        onSelected.Invoke();
    }

    public void OnSelect(BaseEventData eventData)
    {
        onSelected.Invoke();
    }

    public void Setup(string text)
    {
        label.text = text;
    }
}
