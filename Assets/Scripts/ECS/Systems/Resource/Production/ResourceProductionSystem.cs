using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(CitizenResourcePickupSystem))]
public class ResourceProductionSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;
    EntityQuery producingEntititesQuery;

    NativeQueue<ResourceCreationInfo> resourcesToCreate;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        producingEntititesQuery = GetEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(ResourceProductionData), typeof(WorkPlaceWorkerData) }
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
            WorkPlaceWorkerDataType = GetArchetypeChunkComponentType<WorkPlaceWorkerData>(true),
            ResourceStorageType = GetArchetypeChunkComponentType<ResourceStorageData>(true),
        }.Schedule(producingEntititesQuery);

        job.Complete();

        while (resourcesToCreate.TryDequeue(out ResourceCreationInfo creationInfo))
        {
            Entity resourceEntity = EntityPrefabManager.Instance.SpawnEntityPrefab(creationInfo.ResourceType.ToString());

            EntityManager.SetComponentData(resourceEntity, new ResourceData { ResourceType = creationInfo.ResourceType, Amount = creationInfo.Amount });

            var position = EntityManager.GetComponentData<Translation>(resourceEntity).Value;

            if (EntityManager.HasComponent<GridOccupation>(creationInfo.CreatedBy))
            {
                var occupation = EntityManager.GetComponentData<GridOccupation>(creationInfo.CreatedBy);

                position.x = occupation.Start.x + ((occupation.End.x - occupation.Start.x) / 2);
                position.z = occupation.Start.y - 1;
            }
            else
            {
                position.xz = EntityManager.GetComponentData<Translation>(creationInfo.CreatedBy).Value.xz;
            }

            EntityManager.SetComponentData(resourceEntity, new Translation { Value = position });

            // Set storage capacity
            var resourceStorage = EntityManager.GetComponentData<ResourceStorageData>(creationInfo.CreatedBy);
            resourceStorage.UsedCapacity++;
            EntityManager.SetComponentData(creationInfo.CreatedBy, resourceStorage);

            // Add to storage

            var resourceBuffer = EntityManager.GetBuffer<ResourceDataElement>(creationInfo.CreatedBy);
            resourceBuffer.Add(new ResourceData { ResourceType = creationInfo.ResourceType, Amount = creationInfo.Amount });

            EntityManager.AddComponent<ResourceInStorageData>(resourceEntity);
            EntityManager.SetComponentData(resourceEntity, new ResourceInStorageData
            {
                StorageEntity = creationInfo.CreatedBy,
                ResourceData = new ResourceData { ResourceType = creationInfo.ResourceType, Amount = creationInfo.Amount },
                StorageAreaStartPosition = position,
                StorageAreaEndPosition = position + new float3(1, 1, 1),
            });
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
        public ArchetypeChunkComponentType<WorkPlaceWorkerData> WorkPlaceWorkerDataType;
        [ReadOnly]
        public ArchetypeChunkComponentType<ResourceStorageData> ResourceStorageType;

        public ArchetypeChunkComponentType<ResourceProductionData> ResourceProductionDataType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityType);

            NativeArray<ResourceProductionData> resourceProductionDatas = chunk.GetNativeArray(ResourceProductionDataType);
            NativeArray<WorkPlaceWorkerData> workerDatas = chunk.GetNativeArray(WorkPlaceWorkerDataType);
            NativeArray<ResourceStorageData> resourceStorages = chunk.GetNativeArray(ResourceStorageType);

            for (int i = 0; i < chunk.Count; i++)
            {
                ResourceProductionData productionData = resourceProductionDatas[i];
                WorkPlaceWorkerData workerData = workerDatas[i];

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
                        Position = float3.zero,
                        CreatedBy = entities[i]
                    });

                    productionData.ProductionTimeRemaining = productionData.ProductionTime;
                    resourceProductionDatas[i] = productionData;
                }
            }
        }
    }

    struct ResourceCreationInfo
    {
        public float3 Position;
        public ResourceType ResourceType;
        public int Amount;
        public Entity CreatedBy;
    }
}