using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BringWindowToFrontOnClick : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private RectTransform window;

    private void Start()
    {
        if (window == null)
            window = gameObject.GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        window.SetAsLastSibling();
    }
}
