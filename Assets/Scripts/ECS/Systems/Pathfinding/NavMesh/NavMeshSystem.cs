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
using UnityEngine.Experimental.AI;

[UpdateAfter(typeof(PlacementSystem))]
public class NavMeshSystem : ComponentSystem
{
    private Bounds bounds;
    private NavMeshData navMeshData;
    private NavMeshDataInstance navMeshDataInstance;

    /// Key is index of the entity
    NativeHashMap<int, NavMeshBuildSource> indexedSources;
    NativeList<int> expectedIds;
    bool updateMesh;

    List<NavMeshBuildSource> sources;
    protected override void OnCreate()
    {
        navMeshData = new NavMeshData();
        navMeshDataInstance = NavMesh.AddNavMeshData(navMeshData);
        bounds = new Bounds(Vector3.zero, new Vector3(5000, 256, 5000));

        indexedSources = new NativeHashMap<int, NavMeshBuildSource>(10, Allocator.Persistent);
        expectedIds = new NativeList<int>(Allocator.Persistent);

        updateMesh = true;
    }

    protected override void OnUpdate()
    {
        expectedIds.Clear();

        Entities.WithAllReadOnly<NavMeshObstacle, LocalToWorld>().ForEach((Entity entity, ref LocalToWorld localToWorld) =>
        {
            expectedIds.Add(entity.Index);

            if (!indexedSources.ContainsKey(entity.Index))
            {
                // Add new source
                var obstacleData = EntityManager.GetSharedComponentData<NavMeshObstacle>(entity);

                if (obstacleData.Size.Equals(float3.zero))
                {
                    Mesh mesh = EntityManager.GetSharedComponentData<RenderMesh>(entity).mesh;

                    obstacleData.Size = mesh.bounds.size;
                    EntityManager.SetSharedComponentData(entity, obstacleData);
                }

                NavMeshBuildSource source = new NavMeshBuildSource
                {
                    area = obstacleData.Area,
                    shape = NavMeshBuildSourceShape.Box,
                    size = obstacleData.Size,
                    transform = localToWorld.Value
                };

                indexedSources.Add(entity.Index, source);
            }
            else
            {
                // Update Position if needed
                NavMeshBuildSource source = indexedSources[entity.Index];
                if (source.transform != (Matrix4x4)localToWorld.Value)
                {
                    source.transform = localToWorld.Value;
                    indexedSources[entity.Index] = source;
                    updateMesh = true;
                }
            }
        });

        Entities.WithAllReadOnly<NavMeshSurface, LocalToWorld>().ForEach((Entity entity, ref LocalToWorld localToWorld) =>
        {
            expectedIds.Add(entity.Index);

            if (!indexedSources.ContainsKey(entity.Index))
            {
                // Add new source
                var surfaceData = EntityManager.GetComponentData<NavMeshSurface>(entity);

                //if (surfaceData.Size.Equals(float3.zero))
                var mesh = EntityManager.GetSharedComponentData<RenderMesh>(entity).mesh;

                NavMeshBuildSource source = new NavMeshBuildSource
                {
                    area = surfaceData.Area,
                    shape = NavMeshBuildSourceShape.Mesh,
                    sourceObject = mesh,
                    //size = surfaceData.Size,
                    transform = localToWorld.Value
                };
                indexedSources.Add(entity.Index, source);
            }
            else
            {
                // Update Position if needed
                NavMeshBuildSource source = indexedSources[entity.Index];
                if (source.transform != (Matrix4x4)localToWorld.Value)
                {
                    source.transform = localToWorld.Value;
                    indexedSources[entity.Index] = source;
                    updateMesh = true;
                }
            }
        });

        for (int i = 0; i < expectedIds.Length; i++)
        {
            int key = expectedIds[i];

            if (!indexedSources.ContainsKey(key))
            {
                indexedSources.Remove(key);
                updateMesh = true;
            }
        }

        if (updateMesh)
        {
            var temp = indexedSources.GetValueArray(Allocator.TempJob);
            sources = temp.ToList();
            NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);
            NavMeshBuilder.UpdateNavMeshData(
               navMeshData,
               buildSettings,
               sources,
               bounds);

            temp.Dispose();
            //NavMeshQuerySystem.instance.world = NavMeshWorld.GetDefaultWorld();
            updateMesh = false;
        }
    }

    protected override void OnDestroy()
    {
        indexedSources.Dispose();
        expectedIds.Dispose();
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
