using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

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
        var productionJob = new ProductionJob
        {
            DeltaTime = Time.DeltaTime,
            ResourcesToCreate = resourcesToCreate.AsParallelWriter(),
            EntityType = GetArchetypeChunkEntityType(),
            ResourceProductionDataType = GetArchetypeChunkComponentType<ResourceProductionData>(),
            WorkPlaceWorkerDataType = GetArchetypeChunkComponentType<WorkPlaceWorkerData>(),
        }.Schedule(producingEntititesQuery);

        productionJob.Complete();


        while (resourcesToCreate.TryDequeue(out ResourceCreationInfo creationInfo))
        {
            var resourceEntity = EntityCreationManager.Instance.GetSetupResourceEntity(creationInfo.ResourceType, creationInfo.Amount);

            var position = EntityManager.GetComponentData<Translation>(resourceEntity).Value;

            if (EntityManager.HasComponent<GridOccupation>(creationInfo.CreatedBy))
            {
                var occupation = EntityManager.GetComponentData<GridOccupation>(creationInfo.CreatedBy);

                position.x = occupation.Start.x + ((occupation.End.x - occupation.Start.x) / 2);
                position.z = occupation.Start.y + ((occupation.End.y - occupation.Start.y) / 2);
            }

            EntityManager.SetComponentData(resourceEntity, new Translation { Value = position });
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
        public ArchetypeChunkComponentType<ResourceProductionData> ResourceProductionDataType;
        public ArchetypeChunkComponentType<WorkPlaceWorkerData> WorkPlaceWorkerDataType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityType);

            var resourceProductionDatas = chunk.GetNativeArray(ResourceProductionDataType);
            var workerDatas = chunk.GetNativeArray(WorkPlaceWorkerDataType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var productionData = resourceProductionDatas[i];
                var workerData = workerDatas[i];

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