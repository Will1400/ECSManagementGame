using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(ProductionGroup))]
public class FoodProductionSystem : SystemBase
{
    EntityQuery producingEntititesQuery;

    NativeQueue<CreationInfo> resourcesToCreate;

    protected override void OnCreate()
    {
        producingEntititesQuery = GetEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(FoodProductionData) }
        });

        resourcesToCreate = new NativeQueue<CreationInfo>(Allocator.Persistent);
    }

    protected override void OnUpdate()
    {
        if (resourcesToCreate.Count > 0)
            resourcesToCreate.Clear();

        var job = new ProductionJob
        {
            DeltaTime = Time.DeltaTime,
            ResourcesToCreate = resourcesToCreate.AsParallelWriter(),
            EntityType = GetArchetypeChunkEntityType(),
            FoodProductionDataType = GetArchetypeChunkComponentType<FoodProductionData>(),
            WorkplaceWorkerDataType = GetArchetypeChunkComponentType<WorkplaceWorkerData>(true),
            ResourceStorageType = GetArchetypeChunkComponentType<ResourceStorageData>(true),
        }.Schedule(producingEntititesQuery);

        job.Complete();

        while (resourcesToCreate.TryDequeue(out CreationInfo creationInfo))
        {
            Entity foodEntity = EntityPrefabManager.Instance.SpawnEntityPrefab(creationInfo.ResourceType.ToString());

            EntityManager.SetComponentData(foodEntity, new ResourceData { ResourceType = ResourceType.Food, Amount = creationInfo.Amount });

            var position = EntityManager.GetComponentData<Translation>(foodEntity).Value;

            if (math.all(creationInfo.PositionOffset == float3.zero) && EntityManager.HasComponent<GridOccupation>(creationInfo.StorageEntity))
            {
                var occupation = EntityManager.GetComponentData<GridOccupation>(creationInfo.StorageEntity);

                position.x = occupation.Start.x + ((occupation.End.x - occupation.Start.x) / 2);
                position.z = occupation.Start.y - 1;
            }
            else
            {
                position.xz = EntityManager.GetComponentData<Translation>(creationInfo.StorageEntity).Value.xz;
            }

            if (math.all(creationInfo.PositionOffset != float3.zero))
                position += creationInfo.PositionOffset;

            EntityManager.SetComponentData(foodEntity, new Translation { Value = position });

            if (creationInfo.MakeFoodAvailable)
                EntityManager.AddComponent<ResourceIsAvailableTag>(foodEntity);

            // Add to storage
            var entity = EntityManager.CreateEntity();
            EntityManager.AddComponent<AddResourceToStorageData>(entity);
            EntityManager.SetComponentData(entity, new AddResourceToStorageData { ResourceEntity = foodEntity, StorageEntity = creationInfo.StorageEntity });
        }
    }

    protected override void OnDestroy()
    {
        resourcesToCreate.Dispose();
    }

    struct ProductionJob : IJobChunk
    {
        public NativeQueue<CreationInfo>.ParallelWriter ResourcesToCreate;

        [ReadOnly]
        public float DeltaTime;

        [ReadOnly]
        public ArchetypeChunkEntityType EntityType;
        [ReadOnly]
        public ArchetypeChunkComponentType<WorkplaceWorkerData> WorkplaceWorkerDataType;
        [ReadOnly]
        public ArchetypeChunkComponentType<ResourceStorageData> ResourceStorageType;

        public ArchetypeChunkComponentType<FoodProductionData> FoodProductionDataType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityType);

            NativeArray<FoodProductionData> productionDatas = chunk.GetNativeArray(FoodProductionDataType);
            NativeArray<ResourceStorageData> resourceStorages = chunk.GetNativeArray(ResourceStorageType);

            bool usesWorkers = false;

            if (chunk.Has<WorkplaceWorkerData>(WorkplaceWorkerDataType))
            {
                NativeArray<WorkplaceWorkerData> workerDatas = chunk.GetNativeArray(WorkplaceWorkerDataType);
                usesWorkers = true;

                for (int i = 0; i < chunk.Count; i++)
                {
                    FoodProductionData productionData = productionDatas[i];

                    productionData.ProductionTimeRemaining -= DeltaTime * (workerDatas[i].ActiveWorkers / .5f);
                    productionDatas[i] = productionData;
                }
            }

            for (int i = 0; i < chunk.Count; i++)
            {
                if (resourceStorages[i].UsedCapacity >= resourceStorages[i].MaxCapacity)
                    continue;

                FoodProductionData productionData = productionDatas[i];

                if (!usesWorkers)
                {
                    productionData.ProductionTimeRemaining -= DeltaTime;
                    productionDatas[i] = productionData;
                }

                if (productionDatas[i].ProductionTimeRemaining <= 0)
                {
                    ResourcesToCreate.Enqueue(new CreationInfo
                    {
                        ResourceType = productionData.FoodType,
                        Amount = productionData.AmountPerProduction,
                        PositionOffset = productionData.SpawnPointOffset,
                        StorageEntity = entities[i],
                        MakeFoodAvailable = productionData.IsProducedFoodAvailable
                    });

                    productionData.ProductionTimeRemaining = productionData.ProductionTime;
                    productionDatas[i] = productionData;
                }
            }
        }
    }

    struct CreationInfo
    {
        public float3 PositionOffset;
        public FoodType ResourceType;
        public int Amount;
        public Entity StorageEntity;
        public bool MakeFoodAvailable;
    }
}
