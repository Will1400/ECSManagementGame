using UnityEngine;
using System.Collections;
using Unity.Entities;
using UnityEngine.AI;
using Unity.Collections;
using Unity.Transforms;
using Unity.Rendering;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;

public class NavMeshSystem : ComponentSystem
{
    private Bounds bounds;
    private NavMeshData navMeshData;
    private NavMeshDataInstance navMeshDataInstance;

    List<NavMeshBuildSource> sources;
    protected override void OnCreate()
    {
        navMeshData = new NavMeshData();
        navMeshDataInstance = NavMesh.AddNavMeshData(navMeshData);
        bounds = new Bounds(Vector3.zero, new Vector3(200, 256, 200));
        navMeshData.position = new Vector3(bounds.extents.x, 0, bounds.extents.z);
        sources = new List<NavMeshBuildSource>();

    }

    protected override void OnUpdate()
    {
        NativeArray<Entity> obstacles = Entities.WithAll<GridOccupation, RenderMesh, LocalToWorld, IsInCache>().ToEntityQuery().ToEntityArray(Allocator.TempJob);
        NativeArray<Entity> surfaces = Entities.WithAll<NavMeshSurface, RenderMesh, LocalToWorld>().ToEntityQuery().ToEntityArray(Allocator.TempJob);

        bool needsUpdate = false;
        for (int i = 0; i < obstacles.Length; i++)
        {
            Entity entity = obstacles[i];

            RenderMesh renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(entity);
            NavMeshBuildSource buildingSource = new NavMeshBuildSource
            {
                area = 1,
                shape = NavMeshBuildSourceShape.Box,
                size = renderMesh.mesh.bounds.size,
                transform = EntityManager.GetComponentData<LocalToWorld>(entity).Value
            };

            if (!sources.Contains(buildingSource))
            {
                sources.Add(buildingSource);
                needsUpdate = true;
            }
        }

        for (int i = 0; i < surfaces.Length; i++)
        {
            Entity entity = surfaces[i];

            RenderMesh renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(entity);
            NavMeshBuildSource buildingSource = new NavMeshBuildSource
            {
                area = 0,
                shape = NavMeshBuildSourceShape.Box,
                size = renderMesh.mesh.bounds.size,
                transform = EntityManager.GetComponentData<LocalToWorld>(entity).Value
            };

            if (!sources.Contains(buildingSource))
            {
                sources.Add(buildingSource);
                needsUpdate = true;
            }
        }

        if (needsUpdate)
        {
            NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);
            NavMeshBuilder.UpdateNavMeshData(
               navMeshData,
               buildSettings,
               sources,
               bounds);
        }

        obstacles.Dispose();
        surfaces.Dispose();
    }

    void OnDrawGizmosSelected()
    {
        if (navMeshData)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(navMeshData.sourceBounds.center, navMeshData.sourceBounds.size);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new float3(0, 0, 0), bounds.size);
    }

    struct MeshStash
    {
        public Matrix4x4 Transform;
        public int Area;
        public int MeshIndex;
    }
}
