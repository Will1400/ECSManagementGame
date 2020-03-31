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
public class NavMeshSystem : JobComponentSystem
{
    private Bounds bounds;
    private NavMeshData navMeshData;
    private NavMeshDataInstance navMeshDataInstance;

    float updateCooldown = 5;
    float remainingTimeUntilUpdateAvailable;

    /// Key is index of the entity
    NativeHashMap<int, NavMeshBuildSource> indexedSources;
    NativeQueue<SourceStash> sourceQueue;
    bool updateMesh;
    List<NavMeshBuildSource> sources;

    AsyncOperation currentUpdateInfo;

    EntityQuery obstacleQuery;
    EntityQuery surfaceQuery;

    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        navMeshData = new NavMeshData();
        navMeshDataInstance = NavMesh.AddNavMeshData(navMeshData);
        bounds = new Bounds(Vector3.zero, new Vector3(5000, 256, 5000));

        indexedSources = new NativeHashMap<int, NavMeshBuildSource>(10, Allocator.Persistent);
        sourceQueue = new NativeQueue<SourceStash>(Allocator.Persistent);

        updateMesh = true;


        obstacleQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(NavMeshObstacle), typeof(LocalToWorld), typeof(NavMeshSourceHasSizeTag) }
        });
        surfaceQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(NavMeshSurface), typeof(LocalToWorld), typeof(NavMeshSourceHasSizeTag) }
        });
    }

    protected override JobHandle OnUpdate(JobHandle InputDeps)
    {
        remainingTimeUntilUpdateAvailable -= Time.DeltaTime;

        var commandBuffer = bufferSystem.CreateCommandBuffer();
        Entities.WithNone<NavMeshSourceHasSizeTag>().ForEach((Entity entity, ref LocalToWorld localToWorld, ref NavMeshObstacle obstacleData) =>
        {
            if (obstacleData.Size.Equals(float3.zero))
            {
                Mesh mesh = EntityManager.GetSharedComponentData<RenderMesh>(entity).mesh;
                obstacleData.Size = mesh.bounds.size;
            }

            commandBuffer.AddComponent<NavMeshSourceHasSizeTag>(entity);

        }).WithoutBurst().Run();

        Entities.WithNone<NavMeshSourceHasSizeTag>().ForEach((Entity entity, ref LocalToWorld localToWorld, ref NavMeshSurface surfaceData) =>
        {
            if (surfaceData.Size.Equals(float3.zero))
            {
                Mesh mesh = EntityManager.GetSharedComponentData<RenderMesh>(entity).mesh;
                surfaceData.Size = mesh.bounds.size;
            }

            commandBuffer.AddComponent<NavMeshSourceHasSizeTag>(entity);

        }).WithoutBurst().Run();

        if (sourceQueue.Count > 0)
            sourceQueue.Clear();

        JobHandle obstacleJob = new ObstacleJob
        {
            EntityType = GetArchetypeChunkEntityType(),
            LocalToWorldType = GetArchetypeChunkComponentType<LocalToWorld>(),
            NavMeshObstacleType = GetArchetypeChunkComponentType<NavMeshObstacle>(),
            Sources = sourceQueue.AsParallelWriter()
        }.Schedule(obstacleQuery, InputDeps);

        obstacleJob.Complete();

        JobHandle surfaceJob = new SurfaceJob
        {
            EntityType = GetArchetypeChunkEntityType(),
            LocalToWorldType = GetArchetypeChunkComponentType<LocalToWorld>(),
            NavMeshSurfaceType = GetArchetypeChunkComponentType<NavMeshSurface>(),
            Sources = sourceQueue.AsParallelWriter()
        }.Schedule(surfaceQuery, InputDeps);

        surfaceJob.Complete();

        while (sourceQueue.TryDequeue(out SourceStash source))
        {
            if (!indexedSources.ContainsKey(source.OwnersIndex))
            {
                indexedSources.Add(source.OwnersIndex, source.NavMeshBuildSource);
                updateMesh = true;
            }

            // Code for updating the position of the source  - disabled for performance

            //else
            //{
            //    var existingValue = indexedSources[source.OwnersIndex];
            //    if (existingValue.transform != source.NavMeshBuildSource.transform)
            //    {
            //        indexedSources[source.OwnersIndex] = source.NavMeshBuildSource;
            //        updateMesh = true;
            //    }
            //}
        }


        if (updateMesh && (currentUpdateInfo == null || currentUpdateInfo.isDone) && remainingTimeUntilUpdateAvailable <= 0)
        {
            var temp = indexedSources.GetValueArray(Allocator.TempJob);
            sources = temp.ToList();
            NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);
            currentUpdateInfo = NavMeshBuilder.UpdateNavMeshDataAsync(
               navMeshData,
               buildSettings,
               sources,
               bounds);

            temp.Dispose();

            if (NavMeshQuerySystem.instance.UseCache)
                NavMeshQuerySystem.instance.PurgeCache();

            remainingTimeUntilUpdateAvailable = updateCooldown;
            updateMesh = false;
        }

        return obstacleJob;
    }

    protected override void OnDestroy()
    {
        indexedSources.Dispose();
        sourceQueue.Dispose();
    }

    [BurstCompile]
    struct ObstacleJob : IJobChunk
    {
        [ReadOnly]
        public ArchetypeChunkEntityType EntityType;

        public ArchetypeChunkComponentType<NavMeshObstacle> NavMeshObstacleType;
        public ArchetypeChunkComponentType<LocalToWorld> LocalToWorldType;

        public NativeQueue<SourceStash>.ParallelWriter Sources;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityType);
            NativeArray<LocalToWorld> localToWorlds = chunk.GetNativeArray(LocalToWorldType);
            NativeArray<NavMeshObstacle> obstacles = chunk.GetNativeArray(NavMeshObstacleType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var entity = entities[i];
                var localToWorld = localToWorlds[i];
                var obstacleData = obstacles[i];
                // Add new source

                SourceStash source = new SourceStash
                {
                    NavMeshBuildSource = new NavMeshBuildSource
                    {
                        area = obstacleData.Area,
                        shape = NavMeshBuildSourceShape.Box,
                        size = obstacleData.Size,
                        transform = localToWorld.Value
                    },
                    OwnersIndex = entity.Index
                };

                Sources.Enqueue(source);
            }
        }
    }

    [BurstCompile]
    struct SurfaceJob : IJobChunk
    {
        [ReadOnly]
        public ArchetypeChunkEntityType EntityType;

        public ArchetypeChunkComponentType<NavMeshSurface> NavMeshSurfaceType;
        public ArchetypeChunkComponentType<LocalToWorld> LocalToWorldType;

        public NativeQueue<SourceStash>.ParallelWriter Sources;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityType);
            NativeArray<LocalToWorld> localToWorlds = chunk.GetNativeArray(LocalToWorldType);
            NativeArray<NavMeshSurface> surfaces = chunk.GetNativeArray(NavMeshSurfaceType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var entity = entities[i];
                var localToWorld = localToWorlds[i];
                var surfaceData = surfaces[i];

                // Add new source
                SourceStash source = new SourceStash
                {
                    NavMeshBuildSource = new NavMeshBuildSource
                    {
                        area = surfaceData.Area,
                        shape = NavMeshBuildSourceShape.Box,
                        size = surfaceData.Size,
                        transform = localToWorld.Value
                    },
                    OwnersIndex = entity.Index
                };

                Sources.Enqueue(source);
            }
        }
    }

    struct SourceStash
    {
        public int OwnersIndex;
        public NavMeshBuildSource NavMeshBuildSource;
    }
}
