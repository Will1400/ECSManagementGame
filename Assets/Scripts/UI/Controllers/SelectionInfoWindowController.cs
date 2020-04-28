using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Entities;
using UnityEngine;

public class SelectionInfoWindowController : MonoBehaviour
{
    private EntityManager EntityManager;
    private Entity selectedEntity;

    [SerializeField]
    private TextMeshProUGUI titleText;
    [SerializeField]
    private GameObject workerControlPanel;
    [SerializeField]
    private GameObject citizenPanel;
    [SerializeField]
    private WindowDragger windowDragger;
    [SerializeField]
    private GameObject contextPanelHolder;
    [SerializeField]
    private TabGroup tabGroup;

    bool isPinned;
    GameObject currentPanel;

    private Dictionary<string, GameObject> applicablePanels = new Dictionary<string, GameObject>();

    public void Initialize(Entity entity)
    {
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (!EntityManager.Exists(entity))
        {
            Destroy(gameObject);
        }

        selectedEntity = entity;

        windowDragger.Setup();

        if (EntityManager.HasComponent<DisplayData>(entity))
            titleText.text = EntityManager.GetComponentData<DisplayData>(entity).Name.ToString();
        else
            titleText.text = "";

        if (EntityManager.HasComponent<WorkplaceWorkerData>(entity))
        {
            workerControlPanel.SetActive(true);
            workerControlPanel.GetComponent<WorkerController>().Initialize(entity);
        }
        else
        {
            workerControlPanel.SetActive(false);
        }

        if (EntityManager.HasComponent<Citizen>(entity))
        {
            citizenPanel.SetActive(true);
        }
        else
        {
            citizenPanel.SetActive(false);
        }

        Dictionary<string, GameObject> availablePanels = new Dictionary<string, GameObject>();

        // Load all panels
        foreach (Transform item in contextPanelHolder.transform)
        {
            availablePanels.Add(item.name.Replace("Panel", string.Empty), item.gameObject);
            item.gameObject.SetActive(false);
        }

        AddApplicablePanels(availablePanels);

        // Setup Applicable panels

        foreach (Transform item in tabGroup.transform)
        {
            if (!applicablePanels.TryGetValue(item.name, out _))
            {
                item.gameObject.SetActive(false);
                tabGroup.Unbscribe(item.GetComponent<TabButton>());
            }
            else
            {
                item.gameObject.SetActive(true);
            }
        }

        // Activate first panel

        if (applicablePanels.Count > 0)
        {
            SwitchToPanel(applicablePanels.First().Key);
            tabGroup.OnTabSelected(tabGroup.tabButtons[0]);
        }
    }

    void AddApplicablePanels(Dictionary<string, GameObject> availablePanels)
    {
        if (EntityManager.HasComponent<Citizen>(selectedEntity) && availablePanels.TryGetValue("CitizenDetails", out GameObject panel))
        {
            applicablePanels.Add("CitizenDetails", panel);
        }
        if (EntityManager.HasComponent<CitizenElement>(selectedEntity) && availablePanels.TryGetValue("Occupants", out panel))
        {
            applicablePanels.Add("Occupants", panel);
        }
        if (EntityManager.HasComponent<ResourceProductionData>(selectedEntity) && availablePanels.TryGetValue("Production", out panel))
        {
            applicablePanels.Add("Production", panel);
        }
        if (EntityManager.HasComponent<ResourceDataElement>(selectedEntity) && availablePanels.TryGetValue("Inventory", out panel))
        {
            applicablePanels.Add("Inventory", panel);
        }
    }

    public void SwitchToPanel(string panelName)
    {
        if (applicablePanels.TryGetValue(panelName, out GameObject panel))
        {
            if (panel != currentPanel)
            {
                if (currentPanel != null)
                    currentPanel.SetActive(false);

                panel.SetActive(true);
                var filler = panel.GetComponent<PanelFillerBase>();
                if (filler != null)
                {
                    filler.Fill(selectedEntity);
                }

                currentPanel = panel;
            }
        }
    }

    public void TogglePin()
    {
        var windowSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SelectionUISystem>();

        isPinned = !isPinned;
        if (isPinned)
        {
            windowSystem.PinWindow(gameObject);
        }
        else
        {
            windowSystem.UnpinWindow(gameObject);
        }

    }

    public void CloseWindow()
    {
        Destroy(gameObject);
    }
}
