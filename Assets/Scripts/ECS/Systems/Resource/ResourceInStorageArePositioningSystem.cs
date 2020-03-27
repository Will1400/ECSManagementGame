using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

public class ResourceInStorageArePositioningSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;
    EntityQuery resourcesInStorageQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        resourcesInStorageQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceInStorage), typeof(Translation)}
        });
    }

    protected override void OnUpdate()
    {
        var job = new PositionJob
        {
            EntityType = GetArchetypeChunkEntityType(),
            TranslationType = GetArchetypeChunkComponentType<Translation>(),
            ResourceInStorageType = GetArchetypeChunkComponentType<ResourceInStorage>(),
        }.Schedule(resourcesInStorageQuery);
        job.Complete();
    }

    [BurstCompile]
    struct PositionJob : IJobChunk
    {
        [ReadOnly]
        public ArchetypeChunkEntityType EntityType;
        public ArchetypeChunkComponentType<Translation> TranslationType;
        public ArchetypeChunkComponentType<ResourceInStorage> ResourceInStorageType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityType);

            NativeArray<Translation> translations = chunk.GetNativeArray(TranslationType);
            NativeArray<ResourceInStorage> resourceInStorages = chunk.GetNativeArray(ResourceInStorageType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var translation = translations[i];
                var resourceInStorage = resourceInStorages[i];

                float currentZ = 1 * (resourceInStorage.StorageEntityIndex % 4);
                float currentY = .25f * ((int)math.floor(resourceInStorage.StorageEntityIndex / 4) + 1);

                float3 positionOffset = new float3(0, 0, .75f) + new float3(0, currentY, currentZ);

                translation.Value = positionOffset + resourceInStorage.StorageAreaStartPosition;

                translations[i] = translation;
            }
        }
    }
}
