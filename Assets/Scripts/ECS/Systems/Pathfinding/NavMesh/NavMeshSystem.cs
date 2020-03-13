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
using Unity.Jobs;
using System.Linq;

public class NavMeshSystem : ComponentSystem
{
    private Bounds bounds;
    private NavMeshData navMeshData;
    private NavMeshDataInstance navMeshDataInstance;

    NativeList<NavMeshBuildSource> obstacles;
    NativeList<NavMeshBuildSource> surfaces;

    List<NavMeshBuildSource> sources;
    protected override void OnCreate()
    {
        navMeshData = new NavMeshData();
        navMeshDataInstance = NavMesh.AddNavMeshData(navMeshData);
        bounds = new Bounds(Vector3.zero, new Vector3(200, 256, 200));
        navMeshData.position = new Vector3(bounds.extents.x, 0, bounds.extents.z);
        sources = new List<NavMeshBuildSource>();

        obstacles = new NativeList<NavMeshBuildSource>(Allocator.Persistent);
        surfaces = new NativeList<NavMeshBuildSource>(Allocator.Persistent);
    }

    protected override void OnUpdate()
    {

        Entities.WithAllReadOnly<NavMeshObstacle, LocalToWorld>().ForEach((Entity obstacleEntity, ref LocalToWorld localToWorld) =>
        {
            obstacles.Clear();
            var obstacle = EntityManager.GetSharedComponentData<NavMeshObstacle>(obstacleEntity);

            if (obstacle.Size.Equals(float3.zero))
                obstacle.Size = EntityManager.GetSharedComponentData<RenderMesh>(obstacleEntity).mesh.bounds.size;

            NavMeshBuildSource buildingSource = new NavMeshBuildSource
            {
                area = obstacle.Area,
                shape = NavMeshBuildSourceShape.Box,
                size = obstacle.Size,
                transform = localToWorld.Value
            };
            obstacles.Add(buildingSource);
        });

        Entities.WithAllReadOnly<NavMeshSurface, LocalToWorld>().ForEach((Entity surfaceEntity, ref LocalToWorld localToWorld) =>
        {
            surfaces.Clear();
            var obstacle = EntityManager.GetSharedComponentData<NavMeshSurface>(surfaceEntity);

            if (obstacle.Size.Equals(float3.zero))
                obstacle.Size = EntityManager.GetSharedComponentData<RenderMesh>(surfaceEntity).mesh.bounds.size;

            NavMeshBuildSource buildingSource = new NavMeshBuildSource
            {
                area = obstacle.Area,
                shape = NavMeshBuildSourceShape.Box,
                size = obstacle.Size,
                transform = localToWorld.Value
            };
            surfaces.Add(buildingSource);
        });


        var combined = obstacles.ToArray().Concat(surfaces.ToArray()).ToList();
        if (combined.Count > 0)
        {
            sources = combined;
            NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);
            NavMeshBuilder.UpdateNavMeshData(
               navMeshData,
               buildSettings,
               sources,
               bounds);
        }

    }

    protected override void OnDestroy()
    {
        obstacles.Dispose();
        surfaces.Dispose();
    }

    //struct ObstacleJob : IJobChunk
    //{
    //    public ArchetypeChunkSharedComponentType<NavMeshObstacle> NavMeshObstacleType;
    //    public ArchetypeChunkSharedComponentType<RenderMesh> RenderMeshType;
    //    public ArchetypeChunkComponentType<LocalToWorld> LocalToWorldType;

    //    public NativeQueue<MeshStash> MeshSources;
    //    public NativeList<NavMeshBuildSource> Sources;

    //    public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
    //    {
    //        int obstacleIndex = chunk.GetSharedComponentIndex(NavMeshObstacleType);
    //        int meshIndex = chunk.GetSharedComponentIndex(RenderMeshType);
    //        var localToWorlds = chunk.GetNativeArray(LocalToWorldType);

    //        for (int i = 0; i < chunk.Count; i++)
    //        {
    //            var transform = localToWorlds[i].Value;

    //            MeshSources.Enqueue(new MeshStash
    //            {
    //                Transform = transform,
    //                MeshIndex = meshIndex,
    //                ObstacleIndex = obstacleIndex
    //            });
    //        }
    //    }
    //}

    //public struct MeshStash
    //{
    //    public Matrix4x4 Transform;
    //    public int MeshIndex;
    //    public int ObstacleIndex;
    //}
}
