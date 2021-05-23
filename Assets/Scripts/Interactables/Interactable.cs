using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Interactable : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler, IPointerExitHandler,
                                     IBeginDragHandler, IDragHandler, IEndDragHandler
{


    protected virtual void FingerClick(PointerEventData eventData) { }
    protected virtual void FingerDown(PointerEventData eventData) { }
    protected virtual void FingerEnter(PointerEventData eventData) { }
    protected virtual void FingerUp(PointerEventData eventData) { }
    protected virtual void FingerExit(PointerEventData eventData) { }
    protected virtual void FingerBeginDrag(PointerEventData eventData) { }
    protected virtual void FingerDrag(PointerEventData eventData) { }
    protected virtual void FingerEndDrag(PointerEventData eventData) { }

    public void OnPointerClick(PointerEventData eventData)
    {
        FingerClick(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        FingerDown(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        FingerEnter(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        FingerUp(eventData);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        FingerExit(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        FingerBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        FingerDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        FingerEndDrag(eventData);
    }
}
