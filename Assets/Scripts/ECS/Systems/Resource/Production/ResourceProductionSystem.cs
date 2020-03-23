using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class ResourceProductionSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {

    }


    struct ProductionJob : IJobChunk
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

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
            }
        }
    }
}