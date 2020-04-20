using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionUISystem : SystemBase
{
    GameObject buildingInfoPrefab;

    protected override void OnCreate()
    {
        buildingInfoPrefab = Resources.Load<GameObject>("Prefabs/OtherPrefabs/UI/BuildingInfoPanel");
    }

    protected override void OnUpdate()
    {
        if (!Input.GetButtonDown("Primary Mouse"))
            return;

        bool isOverObject = !EventSystem.current.IsPointerOverGameObject();


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hit =ECSRaycast.Raycast(ray.origin, ray.direction * 9999);

        if (hit.Entity != Entity.Null)
        {
            if (EntityManager.HasComponent<GridOccupation>(hit.Entity))
            {
                var infoPanel =GameObject.Instantiate(buildingInfoPrefab);
                var controller = infoPanel.GetComponent<SelectionInfoWindowController>();
                controller.Initialize(hit.Entity);
            }
            else
            {

            }
        }
    }
}
