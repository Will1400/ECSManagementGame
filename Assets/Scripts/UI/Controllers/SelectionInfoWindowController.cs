using System.Collections;
using System.Collections.Generic;
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
    private WindowDragger windowDragger;
    [SerializeField]
    private GameObject contextPanelHolder;
    [SerializeField]
    private TabGroup tabGroup;

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

        titleText.text = EntityManager.GetName(entity);

        Dictionary<string, GameObject> availablePanels = new Dictionary<string, GameObject>();

        // Load all panels
        foreach (Transform item in contextPanelHolder.transform)
        {
            availablePanels.Add(item.name.Replace("Panel", string.Empty), item.gameObject);
        }

        // Check entity for components that has a panel

        if (EntityManager.HasComponent<CitizenElement>(entity) && availablePanels.TryGetValue("Occupants", out GameObject panel))
        {
            applicablePanels.Add("Occupants", panel);
        }

        if (EntityManager.HasComponent<ResourceProductionData>(entity) && availablePanels.TryGetValue("Production", out panel))
        {
            applicablePanels.Add("Production", panel);
        }

        // Setup Applicable panels

        foreach (Transform item in tabGroup.transform)
        {
            if (!applicablePanels.TryGetValue(item.name, out panel))
            {
                item.gameObject.SetActive(false);
            }
            else
            {
                item.gameObject.SetActive(true);
            }
        }
    }

    public void SwitchToPanel(string panelName)
    {
        if (applicablePanels.TryGetValue(panelName, out GameObject panel))
        {
            panel.SetActive(true);
            var filler = panel.GetComponent<PanelFiller>();
            if (filler != null)
            {
                filler.Fill(selectedEntity);
            }
        }
    }

    public void CloseWindow()
    {
        Destroy(gameObject);
    }
}
