using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDragger : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField]
    private RectTransform transformToDrag;
    [SerializeField]
    private Canvas Canvas;

    // Start is called before the first frame update
    void Start()
    {
        if (transformToDrag == null)
            transformToDrag = transform.parent.GetComponent<RectTransform>();

        if (Canvas == null)
        {
            var tempCanvasTransform = transform.parent;
            while (tempCanvasTransform != null)
            {
                Canvas = tempCanvasTransform.GetComponent<Canvas>();
                if (Canvas != null)
                    break;

                tempCanvasTransform = tempCanvasTransform.parent;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transformToDrag.anchoredPosition += eventData.delta / Canvas.scaleFactor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transformToDrag.SetAsLastSibling();
    }
}
