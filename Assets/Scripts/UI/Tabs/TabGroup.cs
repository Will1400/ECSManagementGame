using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TabGroup : MonoBehaviour
{
    public List<TabButton> tabButtons;

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

    [SerializeField]
    TabButton selectedTab;

    private void Awake()
    {
        if (tabButtons == null)
            tabButtons = new List<TabButton>();
        else
        {
            foreach (TabButton tabButton in tabButtons)
            {
                tabButton.SetTabGroup(this);
                tabButton.SetColor(tabIdle, tabSecondaryIdle);
            }

            if (tabButtons.Count > 0)
                OnTabSelected(tabButtons[0]);
        }
    }

    public void SelectLeft()
    {
        int targetIndex = tabButtons.IndexOf(selectedTab) - 1;
        if (targetIndex < 0)
            targetIndex = tabButtons.Count - 1;
        OnTabSelected(tabButtons[targetIndex % tabButtons.Count]);
    }

    public void SelectRight()
    {
        OnTabSelected(tabButtons[(tabButtons.IndexOf(selectedTab) + 1) % tabButtons.Count]);

    }

    public void Subscribe(TabButton tabButton)
    {
        if (!tabButtons.Contains(tabButton))
        {
            tabButtons.Add(tabButton);
            tabButton.SetColor(tabIdle, tabSecondaryIdle);
        }
    }

    public void Unbscribe(TabButton tabButton)
    {
        if (tabButtons.Contains(tabButton))
        {
            tabButtons.Remove(tabButton);
        }
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
