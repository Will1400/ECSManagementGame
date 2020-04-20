using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SelectionInfoWindowController : MonoBehaviour
{

    private EntityManager EntityManager;
    private Entity selectedEntity;

    [SerializeField]
    private GameObject ContextPanelHolder;
    [SerializeField]
    private TabGroup TabGroup;

    private Dictionary<string, GameObject> applicablePanels = new Dictionary<string, GameObject>();

    public void Initialize(Entity entity)
    {
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (!EntityManager.Exists(entity))
        {
            Destroy(gameObject);
        }

        Dictionary<string, GameObject> availablePanels = new Dictionary<string, GameObject>();

        // Load all panels
        foreach (Transform item in ContextPanelHolder.transform)
        {
            availablePanels.Add(item.name.Replace("Panel", string.Empty), item.gameObject);
        }

        // Check entity for components that has a panel

        if (EntityManager.HasComponent<CitizenElement>(entity) && availablePanels.TryGetValue("Occupants", out GameObject panel))
        {
            applicablePanels.Add("Occupants", panel);
        }

        // Setup Applicable panels

        foreach (Transform item in TabGroup.transform)
        {
            if (!applicablePanels.TryGetValue(item.name, out panel))
            {
                Destroy(item.gameObject);
            }
        }
    }
}
