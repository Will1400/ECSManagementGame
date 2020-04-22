using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionUISystem : SystemBase
{
    GameObject buildingInfoPrefab;
    GameObject canvas;

    GameObject currentOpenWindow;

    protected override void OnCreate()
    {
        buildingInfoPrefab = Resources.Load<GameObject>("Prefabs/OtherPrefabs/UI/BuildingInfoPanel");
    }

    protected override void OnUpdate()
    {
        if (canvas == null)
            canvas = GameObject.Find("Canvas");

        if (!Input.GetButtonDown("Primary Mouse") || EventSystem.current.IsPointerOverGameObject() || GameManager.Instance.CursorState != CursorState.None)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hit = ECSRaycast.Raycast(ray.origin, ray.direction * 9999);

        if (hit.Entity != Entity.Null)
        {
            if (EntityManager.HasComponent<GridOccupation>(hit.Entity))
            {
                if (currentOpenWindow != null)
                {
                    currentOpenWindow.GetComponent<SelectionInfoWindowController>().CloseWindow();
                }

                GameObject infoPanel = GameObject.Instantiate(buildingInfoPrefab);
                infoPanel.transform.SetParent(canvas.transform);
                infoPanel.transform.localScale = Vector3.one;
                Vector3 offset = new Vector3(0, -(infoPanel.GetComponent<RectTransform>().rect.height / 4));
                Debug.Log(offset);
                infoPanel.transform.position = Input.mousePosition + offset;

                SelectionInfoWindowController controller = infoPanel.GetComponent<SelectionInfoWindowController>();
                controller.Initialize(hit.Entity);

                currentOpenWindow = infoPanel;
            }
            else
            {

            }
        }
    }
}
