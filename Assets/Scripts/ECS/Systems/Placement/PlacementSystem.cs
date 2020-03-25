using UnityEngine;
using System.Collections;
using Unity.Entities;
using System;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;

[UpdateAfter(typeof(GridValidationSystem))]
public class PlacementSystem : SystemBase
{
    public static PlacementSystem Instance;

    Entity currentEntity;
    string prefabName;
    Material material;

    bool openBuildingMenuWhenDone;

    protected override void OnCreate()
    {
        if (Instance == null)
            Instance = this;
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (currentEntity == Entity.Null)
            {
                Spawn("TallHouse");
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Cancel();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float3 mouseWorldPosition = ECSRaycast.Raycast(ray.origin, ray.direction * 999, 1u << 9).Position;
            var entity = EntityPrefabManager.Instance.SpawnEntityPrefab("Citizen");

            var position = EntityManager.GetComponentData<Translation>(entity);
            position.Value.x = mouseWorldPosition.x;
            position.Value.z = mouseWorldPosition.z;
            EntityManager.SetComponentData(entity, position);
        }

        if (GameManager.Instance.CursorState == CursorState.Building)
        {
            if (EntityManager.HasComponent<GridOccupationIsValidTag>(currentEntity))
            {
                material.SetColor("_BaseColor", Color.green);

                if (Input.GetMouseButtonDown(0))
                    Place();
            }
            else
            {
                material.SetColor("_BaseColor", Color.red);
            }

            if (Input.GetButton("Cancel") || Input.GetButton("Secondary Mouse"))
                Cancel();
        }
    }

    private void Place()
    {
        var constructionEntity = EntityManager.CreateEntity(ArcheTypeManager.Instance.GetArcheType(PredifinedArchetype.ConstructionSite));

        float3 position = EntityManager.GetComponentData<Translation>(currentEntity).Value;
        var occupation = GridHelper.CalculateGridOccupationFromBounds(EntityManager.GetComponentData<WorldRenderBounds>(currentEntity).Value);

        DynamicBuffer<ResourceCostElement> buildCostBuffer = EntityManager.GetBuffer<ResourceCostElement>(currentEntity);
        if (buildCostBuffer.IsCreated && buildCostBuffer.Length > 0)
        {
            DynamicBuffer<ResourceCostElement> resourceCostBuffer = EntityManager.AddBuffer<ResourceCostElement>(constructionEntity);

            resourceCostBuffer.CopyFrom(EntityManager.GetBuffer<ResourceCostElement>(currentEntity));
        }

        EntityManager.AddComponentData(constructionEntity, new GridOccupation { Start = new int2(occupation.x, occupation.y), End = new int2(occupation.z, occupation.w) });
        EntityManager.AddComponentData(constructionEntity, new Translation { Value = position });
        EntityManager.AddComponentData(constructionEntity, new WorkPlaceWorkerData { MaxWorkers = 4, WorkPosition = position + new float3(0, 0, -(position.z - occupation.y + 1)) });
        EntityManager.AddComponentData(constructionEntity, new UnderConstruction { totalConstructionTime = 4, remainingConstructionTime = 4, finishedPrefabName = prefabName });

        if (!Input.GetButton("Shift"))
        {
            Cancel();

            if (openBuildingMenuWhenDone)
                BuildUIManager.Instance.OpenPanel();
        }
    }

    public void Spawn(string name)
    {
        Cancel();

        currentEntity = EntityPrefabManager.Instance.SpawnEntityPrefab(name);
        EntityManager.AddComponent<BeingPlacedTag>(currentEntity);

        prefabName = name;
        var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(currentEntity);

        material = new Material(renderMesh.material);
        renderMesh.material = material;
        EntityManager.SetSharedComponentData(currentEntity, renderMesh);

        GameManager.Instance.CursorState = CursorState.Building;
    }

    public void Spawn(string name, bool openBuildingMenuWhenDone)
    {
        Spawn(name);
        this.openBuildingMenuWhenDone = openBuildingMenuWhenDone;
    }

    void Cancel()
    {
        GameManager.Instance.CursorState = CursorState.None;
        if (currentEntity == Entity.Null)
            return;

        prefabName = string.Empty;
        EntityManager.DestroyEntity(currentEntity);
        currentEntity = Entity.Null;
    }
}
