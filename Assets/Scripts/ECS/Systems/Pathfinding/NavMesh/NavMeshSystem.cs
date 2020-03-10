using UnityEngine;
using System.Collections;
using Unity.Entities;
using UnityEngine.AI;
using Unity.Collections;
using Unity.Transforms;
using Unity.Rendering;
using System.Collections.Generic;

public class NavMeshSystem : ComponentSystem
{
    private Bounds bounds;
    private NavMeshData navMeshData;
    private NavMeshDataInstance navMeshDataInstance;

    protected override void OnCreate()
    {
        navMeshData = new NavMeshData();
        navMeshDataInstance = NavMesh.AddNavMeshData(navMeshData);
        bounds = new Bounds(Vector3.zero, new Vector3(8192, 256, 8192));

    }
    protected override void OnUpdate()
    {
        NativeArray<Entity> obstacles = Entities.WithAll<GridOccupation, RenderMesh, LocalToWorld>().ToEntityQuery().ToEntityArray(Allocator.TempJob);
        NativeArray<Entity> surfaces = Entities.WithAll<NavMeshSurface, RenderMesh, LocalToWorld>().ToEntityQuery().ToEntityArray(Allocator.TempJob);

        List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();

        for (int i = 0; i < obstacles.Length; i++)
        {
            Entity entity = obstacles[i];

            RenderMesh renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(entity);
            NavMeshBuildSource buildingSource = new NavMeshBuildSource
            {
                area = 1,
                shape = NavMeshBuildSourceShape.Mesh,
                sourceObject = renderMesh.mesh,
                transform = EntityManager.GetComponentData<LocalToWorld>(entity).Value
            };
            sources.Add(buildingSource);
        }

        for (int i = 0; i < surfaces.Length; i++)
        {
            Entity entity = surfaces[i];

            RenderMesh renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(entity);
            NavMeshBuildSource buildingSource = new NavMeshBuildSource
            {
                area = 0,
                shape = NavMeshBuildSourceShape.Mesh,
                sourceObject = renderMesh.mesh,
                transform = EntityManager.GetComponentData<LocalToWorld>(entity).Value
            };
            sources.Add(buildingSource);
        }


        NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);
        NavMeshBuilder.UpdateNavMeshDataAsync(
           navMeshData,
           buildSettings,
           new List<NavMeshBuildSource>(),
           bounds);

        obstacles.Dispose();
        surfaces.Dispose();
    }

    struct MeshStash
    {
        public Matrix4x4 Transform;
        public int Area;
        public int MeshIndex;
    }
}
