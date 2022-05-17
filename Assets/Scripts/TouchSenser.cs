using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchSenser : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public void OnBeginDrag(PointerEventData data)
    {
        GameManager.Instance.StartDrag(data);
    }
    public void OnEndDrag(PointerEventData data)
    {
        GameManager.Instance.EndDrag(data);
    }
    public void OnDrag(PointerEventData data)
    {
        GameManager.Instance.UpdateDrag(data);
    }
}
