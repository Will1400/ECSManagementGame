using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

[UpdateInGroup(typeof(UIGroup))]
public class SelectionWindowUISystem : SystemBase
{
    GameObject buildingInfoPrefab;
    GameObject windowHolder;

    GameObject currentOpenWindow;

    protected override void OnCreate()
    {
        buildingInfoPrefab = Resources.Load<GameObject>("Prefabs/OtherPrefabs/UI/SelectionInfoPanel");
    }

    public void PinWindow(GameObject window)
    {
        if (currentOpenWindow == window)
            currentOpenWindow = null;
    }

    public void UnpinWindow(GameObject window)
    {
        if (currentOpenWindow != null)
        {
            window.GetComponent<SelectionInfoWindowController>().CloseWindow();
        }
    }

    protected override void OnUpdate()
    {
        if (windowHolder == null)
            windowHolder = GameObject.Find("Canvas").transform.Find("Windows").gameObject;

        if (!Input.GetButtonDown("Primary Mouse") || EventSystem.current.IsPointerOverGameObject() || GameManager.Instance.CursorState != CursorState.None)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hit = ECSRaycast.Raycast(ray.origin, ray.direction * 9999);

        if (hit.Entity != Entity.Null)
        {
            if (EntityManager.HasComponent<GridOccupation>(hit.Entity) || EntityManager.HasComponent<Citizen>(hit.Entity))
            {
                if (currentOpenWindow != null)
                {
                    currentOpenWindow.GetComponent<SelectionInfoWindowController>().CloseWindow();
                }

                GameObject infoPanel = GameObject.Instantiate(buildingInfoPrefab);
                infoPanel.transform.SetParent(windowHolder.transform);
                infoPanel.transform.localScale = Vector3.one;
                Vector3 offset = new Vector3(0, -(infoPanel.GetComponent<RectTransform>().rect.height / 4));
                infoPanel.transform.position = Input.mousePosition + offset;

                SelectionInfoWindowController controller = infoPanel.GetComponent<SelectionInfoWindowController>();
                controller.Initialize(hit.Entity);

                currentOpenWindow = infoPanel;
            }
        }
    }
}
