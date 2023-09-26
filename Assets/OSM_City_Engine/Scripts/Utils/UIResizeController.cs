using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIResizeController : UIBehaviour
{
    public UnityEvent windowResizeEvent = new UnityEvent();
    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        windowResizeEvent?.Invoke();
    }
}
