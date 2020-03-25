using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public UnityEvent OnTabSelected;
    public UnityEvent OnTabDeselected;

    [SerializeField]
    private TabGroup tabGroup;

    [SerializeField]
    private Image background;
    [SerializeField]
    private TextMeshProUGUI text;

    private void Awake()
    {
        if (OnTabSelected == null)
            OnTabSelected = new UnityEvent();

        if (OnTabDeselected == null)
            OnTabDeselected = new UnityEvent();
    }

    void Start()
    {
        if (tabGroup != null)
            tabGroup.Subscribe(this);
    }

    public void SetTabGroup(TabGroup tabGroup)
    {
        this.tabGroup = tabGroup;
    }

    public void SetColor(Color backgroundColor, Color textColor)
    {
        background.color = backgroundColor;
        text.color = textColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup.OnTabEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup.OnTabExit(this);
    }

    public void Select()
    {
        OnTabSelected?.Invoke();
    }

    public void Deselect()
    {
        OnTabDeselected?.Invoke();
    }
}
