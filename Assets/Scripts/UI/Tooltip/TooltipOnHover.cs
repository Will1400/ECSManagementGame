using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, TextArea]
    private string text;
    [SerializeField]
    private float timeToHover = 1;

    float timeRemaining;
    bool isHovering;

    private void Update()
    {
        if (isHovering)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                TooltipController.Instance.ShowTooltip(text);

                // No need to check anymore
                isHovering = false;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        timeRemaining = timeToHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        TooltipController.Instance.HideTooltip();
    }
}
