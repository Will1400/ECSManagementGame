using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;

public class ResourceStorageFullTaggingSystem : SystemBase
{
    EntityQuery resourceStorageQuery;

    protected override void OnCreate()
    {
        resourceStorageQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceStorageData) },
            None = new ComponentType[] {typeof(ResourceProductionData), typeof(FoodProductionData)}
        });
    }

    protected override void OnUpdate()
    {
        var buffer = new EntityCommandBuffer(Allocator.TempJob);
        var job = new TaggingJob
        {
            EntityType = GetArchetypeChunkEntityType(),
            ResourceStorageDataType = GetArchetypeChunkComponentType<ResourceStorageData>(true),
            ResourceStorageFullTagType = GetArchetypeChunkComponentType<ResourceStorageFullTag>(true),
            CommandBuffer = buffer.ToConcurrent()
        }.Schedule(resourceStorageQuery);
        job.Complete();

        buffer.Playback(EntityManager);
        buffer.Dispose();
    }

    [BurstCompile]
    struct TaggingJob : IJobChunk
    {
        [ReadOnly]
        public ArchetypeChunkEntityType EntityType;
        [ReadOnly]
        public ArchetypeChunkComponentType<ResourceStorageData> ResourceStorageDataType;
        [ReadOnly]
        public ArchetypeChunkComponentType<ResourceStorageFullTag> ResourceStorageFullTagType;

        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityType);

            NativeArray<ResourceStorageData> resourceStorageDatas = chunk.GetNativeArray(ResourceStorageDataType);

            if (chunk.Has<ResourceStorageFullTag>(ResourceStorageFullTagType))
            {
                for (int i = 0; i < chunk.Count; i++)
                {
                    if (resourceStorageDatas[i].UsedCapacity < resourceStorageDatas[i].MaxCapacity)
                    {
                        CommandBuffer.RemoveComponent<ResourceStorageFullTag>(chunkIndex, entities[i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < chunk.Count; i++)
                {
                    if (resourceStorageDatas[i].UsedCapacity >= resourceStorageDatas[i].MaxCapacity && resourceStorageDatas[i].MaxCapacity != -1)
                    {
                        CommandBuffer.AddComponent<ResourceStorageFullTag>(chunkIndex, entities[i]);
                    }
                }
            }
        }

    }
}
