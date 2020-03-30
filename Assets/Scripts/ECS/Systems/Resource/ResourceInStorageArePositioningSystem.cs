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
            All = new ComponentType[] { typeof(ResourceInStorage), typeof(Translation) }
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
            //NativeArray<Entity> entities = chunk.GetNativeArray(EntityType);

            NativeArray<Translation> translations = chunk.GetNativeArray(TranslationType);
            NativeArray<ResourceInStorage> resourceInStorages = chunk.GetNativeArray(ResourceInStorageType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var translation = translations[i];
                var resourceInStorage = resourceInStorages[i];

                translations[i] = Place(resourceInStorage.StorageEntityIndex, translation, resourceInStorage.StorageAreaStartPosition, resourceInStorage.StorageAreaEndPosition);
            }
        }

        [BurstCompile]
        Translation Place(int index, Translation translation, float3 startPosition, float3 endPosition)
        {
            int columns = (int)math.floor((endPosition.x - startPosition.x) / 1.5f);
            int rows = (int)(endPosition.z - startPosition.z);

            float currentX = 1.5f * (index % columns);
            float currentZ = 1 * (((int)math.floor(index / columns)) % rows);
            float currentY = .25f * (((int)math.floor(index / columns)) / rows);

            float3 positionOffset = new float3(1, 0, 1.5f) + new float3(currentX, currentY, currentZ);

            translation.Value = positionOffset + startPosition;
            return translation;
        }
    }
}
