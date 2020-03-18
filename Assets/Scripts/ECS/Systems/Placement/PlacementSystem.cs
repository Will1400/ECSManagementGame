using UnityEngine;
using System.Collections;
using Unity.Entities;
using System;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;

[UpdateAfter(typeof(GridValidationSystem))]
public class PlacementSystem : ComponentSystem
{
    Entity currentEntity;
    string prefabName;
    Material material;

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (currentEntity == Entity.Null)
            {
                Spawn(PrefabManager.Instance.GetPrefabByName("TallHouse"));
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Cancel();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float3 mouseWorldPosition = ECSRaycast.Raycast(ray.origin, ray.direction * 999, 1u << 9).Position;
            mouseWorldPosition.y = 1.5f;
            EntityCreationManager.Instance.GetSetupCitizenEntity(mouseWorldPosition);
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

        EntityManager.AddComponentData(constructionEntity, new GridOccupation { Start = new int2(occupation.x, occupation.y), End = new int2(occupation.z, occupation.w) });
        EntityManager.AddComponentData(constructionEntity, new Translation { Value = position });
        EntityManager.AddComponentData(constructionEntity, new WorkPlaceWorkerData { MaxWorkers = 4, WorkPosition = position + new float3(0, 0, -(position.z - occupation.y + 1)) });
        EntityManager.AddComponentData(constructionEntity, new UnderConstruction { totalConstructionTime = 4, remainingConstructionTime = 4, finishedPrefabName = prefabName });

        Cancel();
    }

    void Spawn(GameObject prefab)
    {
        Cancel();
        currentEntity = EntityManager.CreateEntity(ArcheTypeManager.Instance.GetArcheType(PredifinedArchetype.BeingPlaced));
        GameManager.Instance.CursorState = CursorState.Building;
        prefabName = prefab.name;

        Mesh mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        material = new Material(prefab.GetComponent<Renderer>().sharedMaterial);
        EntityManager.AddSharedComponentData(currentEntity, new RenderMesh { mesh = mesh, material = material });
        EntityManager.AddComponentData(currentEntity, new RenderBounds { Value = mesh.bounds.ToAABB() });
        EntityManager.AddComponentData(currentEntity, new Scale { Value = prefab.transform.localScale.x });
        EntityManager.AddComponentData(currentEntity, new Rotation { Value = prefab.transform.rotation });
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
