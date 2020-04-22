using UnityEngine;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using System;

public class TooltipController : MonoBehaviour
{
    public static TooltipController Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        //gameObject.SetActive(false);
    }

    [SerializeField]
    private TextMeshProUGUI tooltipText;
    [SerializeField]
    private RectTransform backgroundRectTransform;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private RectTransform canvasRectTransform;


    private void Update()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, Input.mousePosition, canvas.worldCamera, out Vector2 point);
        transform.localPosition = point;
    }

    public void ShowTooltip(string text)
    {
        gameObject.SetActive(true);

        tooltipText.text = text;

        float2 backgroundSize = new float2(tooltipText.preferredWidth, tooltipText.preferredHeight);
        backgroundRectTransform.sizeDelta = backgroundSize;
        backgroundRectTransform.anchoredPosition = backgroundSize / 2;
        //transform.position = Input.mousePosition;
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
