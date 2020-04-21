using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDragger : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField]
    private RectTransform transformToDrag;
    [SerializeField]
    private Canvas canvas;

    public bool setup;

    public void Setup()
    {
        if (transformToDrag == null)
            transformToDrag = transform.parent.GetComponent<RectTransform>();

        if (canvas == null)
        {
            var tempCanvasTransform = transform.parent;
            while (tempCanvasTransform != null)
            {
                canvas = tempCanvasTransform.GetComponent<Canvas>();
                if (canvas != null)
                    break;

                tempCanvasTransform = tempCanvasTransform.parent;
            }
        }
        setup = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (setup)
            transformToDrag.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transformToDrag.SetAsLastSibling();
    }
}
