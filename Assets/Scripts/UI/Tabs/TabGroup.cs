using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    [SerializeField]
    private List<TabButton> tabButtons;

    [SerializeField]
    private Color tabIdle;
    [SerializeField]
    private Color tabSecondaryIdle;
    [SerializeField]
    private Color tabHover;
    [SerializeField]
    private Color tabSecondaryHover;
    [SerializeField]
    private Color tabSelected;
    [SerializeField]
    private Color tabSecondarySelected;

    TabButton selectedTab;

    private void Awake()
    {
        tabButtons = new List<TabButton>();
    }

    public void Subscribe(TabButton tabButton)
    {
        tabButtons.Add(tabButton);
        tabButton.SetColor(tabIdle, tabSecondaryIdle);
    }

    public void OnTabEnter(TabButton tabButton)
    {
        ResetTabs();

        if (tabButton != selectedTab)
            tabButton.SetColor(tabHover, tabSecondaryHover);
    }

    public void OnTabExit(TabButton tabButton)
    {
        ResetTabs();
    }

    public void OnTabSelected(TabButton tabButton)
    {
        if (selectedTab != null)
            selectedTab.Deselect();

        selectedTab = tabButton;
        selectedTab.Select();

        ResetTabs();
        tabButton.SetColor(tabSelected, tabSecondarySelected);
    }

    public void ResetTabs()
    {
        foreach (TabButton tabButton in tabButtons)
        {
            if (tabButton != selectedTab)
                tabButton.SetColor(tabIdle, tabSecondaryIdle);
        }
    }
}
