using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;

[UpdateBefore(typeof(RemoveCitizenFromWorkSystem))]
public class FireAllWorkersFromWorkplaceSystem : SystemBase
{
    EntityQuery workplacesToFireQuery;
    EntityQuery workingCitizensQuery;
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        workplacesToFireQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(FireAllWorkersTag), typeof(WorkplaceWorkerData) }
        });
        workingCitizensQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Citizen), typeof(CitizenWork) },
            None = new ComponentType[] { typeof(RemoveFromWorkTag) }
        });
    }

    protected override void OnUpdate()
    {
        if (workplacesToFireQuery.CalculateChunkCount() <= 0)
            return;

        NativeArray<Entity> workingCitizens = workingCitizensQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<CitizenWork> workingCitizensWorkerData = workingCitizensQuery.ToComponentDataArray<CitizenWork>(Allocator.TempJob);

        var CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        var job = new RemoveWorkplaceJob
        {
            EntityType = GetArchetypeChunkEntityType(),
            WorkplaceWorkerDataType = GetArchetypeChunkComponentType<WorkplaceWorkerData>(),
            WorkingCitizens = workingCitizens,
            WorkingCitizensWorkerData = workingCitizensWorkerData,
            CommandBuffer = CommandBuffer.ToConcurrent()
        }.Schedule(workplacesToFireQuery);
        job.Complete();

        workingCitizens.Dispose();
        workingCitizensWorkerData.Dispose();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();
    }

    [BurstCompile]
    struct RemoveWorkplaceJob : IJobChunk
    {
        [ReadOnly]
        public ArchetypeChunkEntityType EntityType;
        public ArchetypeChunkComponentType<WorkplaceWorkerData> WorkplaceWorkerDataType;

        [ReadOnly]
        public NativeArray<Entity> WorkingCitizens;
        [ReadOnly]
        public NativeArray<CitizenWork> WorkingCitizensWorkerData;

        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var entities = chunk.GetNativeArray(EntityType);
            var workplaceWorkerDatas = chunk.GetNativeArray(WorkplaceWorkerDataType);

            for (int i = 0; i < chunk.Count; i++)
            {
                WorkplaceWorkerData workplaceWorkerData = workplaceWorkerDatas[i];

                for (int j = 0; j < WorkingCitizensWorkerData.Length; j++)
                {
                    if (workplaceWorkerData.CurrentWorkers <= 0)
                        break;

                    var workerData = WorkingCitizensWorkerData[j];
                    if (workerData.WorkplaceEntity == entities[i])
                    {
                        CommandBuffer.AddComponent<RemoveFromWorkTag>(chunkIndex, WorkingCitizens[j]);
                        workplaceWorkerData.CurrentWorkers--;
                    }
                }

                CommandBuffer.RemoveComponent<FireAllWorkersTag>(chunkIndex, entities[i]);
            }
        }
    }
}
