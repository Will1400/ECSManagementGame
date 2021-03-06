﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(ProductionGroup))]
public class ResourceProductionSystem : SystemBase
{
    EntityQuery producingEntititesQuery;

    NativeQueue<ResourceCreationInfo> resourcesToCreate;

    protected override void OnCreate()
    {
        producingEntititesQuery = GetEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(ResourceProductionData), typeof(WorkplaceWorkerData) }
        });

        resourcesToCreate = new NativeQueue<ResourceCreationInfo>(Allocator.Persistent);
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
            ResourceProductionDataType = GetArchetypeChunkComponentType<ResourceProductionData>(),
            WorkplaceWorkerDataType = GetArchetypeChunkComponentType<WorkplaceWorkerData>(true),
            ResourceStorageType = GetArchetypeChunkComponentType<ResourceStorageData>(true),
        }.Schedule(producingEntititesQuery);

        job.Complete();

        while (resourcesToCreate.TryDequeue(out ResourceCreationInfo creationInfo))
        {
            Entity resourceEntity = EntityPrefabManager.Instance.SpawnEntityPrefab(creationInfo.ResourceType.ToString());

            EntityManager.SetComponentData(resourceEntity, new ResourceData { ResourceType = creationInfo.ResourceType, Amount = creationInfo.Amount });

            var position = EntityManager.GetComponentData<Translation>(resourceEntity).Value;

            if (math.all(creationInfo.PositionOffset != float3.zero))
                position += creationInfo.PositionOffset;

            if (math.all(creationInfo.PositionOffset == float3.zero) && EntityManager.HasComponent<GridOccupation>(creationInfo.StorageEntity))
            {
                var occupation = EntityManager.GetComponentData<GridOccupation>(creationInfo.StorageEntity);

                position.x = occupation.Start.x + ((occupation.End.x - occupation.Start.x) / 2);
                position.z = occupation.Start.y - 2;
            }
            else
            {
                position.xz = EntityManager.GetComponentData<Translation>(creationInfo.StorageEntity).Value.xz;
            }

            EntityManager.SetComponentData(resourceEntity, new Translation { Value = position });

            var entity =EntityManager.CreateEntity();
            EntityManager.AddComponent<AddResourceToStorageData>(entity);
            EntityManager.SetComponentData(entity, new AddResourceToStorageData { ResourceEntity = resourceEntity, StorageEntity = creationInfo.StorageEntity });
        }
    }

    protected override void OnDestroy()
    {
        resourcesToCreate.Dispose();
    }

    struct ProductionJob : IJobChunk
    {
        public NativeQueue<ResourceCreationInfo>.ParallelWriter ResourcesToCreate;

        [ReadOnly]
        public float DeltaTime;

        [ReadOnly]
        public ArchetypeChunkEntityType EntityType;
        [ReadOnly]
        public ArchetypeChunkComponentType<WorkplaceWorkerData> WorkplaceWorkerDataType;
        [ReadOnly]
        public ArchetypeChunkComponentType<ResourceStorageData> ResourceStorageType;

        public ArchetypeChunkComponentType<ResourceProductionData> ResourceProductionDataType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityType);

            NativeArray<ResourceProductionData> resourceProductionDatas = chunk.GetNativeArray(ResourceProductionDataType);
            NativeArray<WorkplaceWorkerData> workerDatas = chunk.GetNativeArray(WorkplaceWorkerDataType);
            NativeArray<ResourceStorageData> resourceStorages = chunk.GetNativeArray(ResourceStorageType);

            for (int i = 0; i < chunk.Count; i++)
            {
                ResourceProductionData productionData = resourceProductionDatas[i];
                WorkplaceWorkerData workerData = workerDatas[i];

                if (resourceStorages[i].UsedCapacity >= resourceStorages[i].MaxCapacity)
                    continue;

                if (workerData.ActiveWorkers >= 0)
                {
                    productionData.ProductionTimeRemaining -= DeltaTime * (workerData.ActiveWorkers / .5f);
                    resourceProductionDatas[i] = productionData;
                }

                if (productionData.ProductionTimeRemaining <= 0)
                {
                    ResourcesToCreate.Enqueue(new ResourceCreationInfo
                    {
                        ResourceType = productionData.ResourceType,
                        Amount = productionData.AmountPerProduction,
                        PositionOffset = productionData.SpawnPointOffset,
                        StorageEntity = entities[i]
                    });

                    productionData.ProductionTimeRemaining = productionData.ProductionTime;
                    resourceProductionDatas[i] = productionData;
                }
            }
        }
    }

    struct ResourceCreationInfo
    {
        public float3 PositionOffset;
        public ResourceType ResourceType;
        public int Amount;
        public Entity StorageEntity;
    }
}