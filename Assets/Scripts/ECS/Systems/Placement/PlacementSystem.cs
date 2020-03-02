﻿using UnityEngine;
using System.Collections;
using Unity.Entities;
using System;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;

public class PlacementSystem : ComponentSystem
{
    public EntityArchetype entityPlacementArchetype;

    Entity currentEntity;
    string prefabName;

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        entityPlacementArchetype = EntityManager.CreateArchetype(typeof(BeingPlacedTag),
                                                                 typeof(FollowsMousePositionTag),
                                                                 typeof(Translation),
                                                                 typeof(Rotation),
                                                                 typeof(Scale),
                                                                 typeof(RenderMesh),
                                                                 typeof(RenderBounds),
                                                                 typeof(LocalToWorld));
    }
    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (currentEntity == Entity.Null)
            {
                Spawn(PrefabManager.Instance.GetBuilding(0));
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Cancel();
            GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Citizen"));
        }

        if (GameManager.Instance.CursorState == CursorState.Building)
        {
            if (Input.GetButton("Cancel") || Input.GetButton("Secondary Mouse"))
                Cancel();

            if (Input.GetMouseButtonDown(0))
                Place();
        }
    }

    private void Place()
    {
        var constructionEntity = EntityManager.CreateEntity(typeof(UnderConstruction),
                                                           typeof(BuildingWorkerData),
                                                           typeof(Translation));
        float3 position = EntityManager.GetComponentData<Translation>(currentEntity).Value;

        EntityManager.AddComponentData(constructionEntity, new Translation { Value = position });
        EntityManager.AddComponentData(constructionEntity, new BuildingWorkerData { MaxWorkers = 4, WorkPosition = position });
        EntityManager.AddComponentData(constructionEntity, new UnderConstruction { totalConstructionTime = 4, remainingConstructionTime = 4, finishedPrefabName = prefabName });

        Cancel();
    }

    void Spawn(GameObject prefab)
    {
        Cancel();
        currentEntity = EntityManager.CreateEntity(entityPlacementArchetype);
        GameManager.Instance.CursorState = CursorState.Building;
        prefabName = prefab.name;

        Mesh mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        Material material = prefab.GetComponent<Renderer>().sharedMaterial;

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

        EntityManager.DestroyEntity(currentEntity);
        currentEntity = Entity.Null;
    }
}
