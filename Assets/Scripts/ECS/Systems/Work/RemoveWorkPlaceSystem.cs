using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateBefore(typeof(RemoveCitizenFromWorkSystem))]
public class RemoveWorkPlaceSystem : SystemBase
{
    EntityQuery workPlacesToRemoveQuery;
    EntityQuery workingCitizensQuery;
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();


        workPlacesToRemoveQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(RemoveWorkPlaceTag) }
        });
        workingCitizensQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(CitizenWork) },
            None = new ComponentType[] { typeof(RemoveFromWorkTag) }
        });
    }

    protected override void OnUpdate()
    {
        if (workPlacesToRemoveQuery.CalculateChunkCount() > 0)
        {
            NativeArray<Entity> workingCitizens = workingCitizensQuery.ToEntityArray(Allocator.TempJob);
            NativeArray<CitizenWork> workingCitizensWorkerData = workingCitizensQuery.ToComponentDataArray<CitizenWork>(Allocator.TempJob);

            var job = new RemoveWorkPlaceJob
            {
                EntityType = GetArchetypeChunkEntityType(),
                WorkPlaceWorkerDataType = GetArchetypeChunkComponentType<WorkPlaceWorkerData>(),
                WorkingCitizens = workingCitizens,
                WorkingCitizensWorkerData = workingCitizensWorkerData,
                CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(workPlacesToRemoveQuery);
            job.Complete();

            workingCitizens.Dispose();
            workingCitizensWorkerData.Dispose();
        }
    }

    [BurstCompile]
    struct RemoveWorkPlaceJob : IJobChunk
    {
        [ReadOnly]
        public ArchetypeChunkEntityType EntityType;
        public ArchetypeChunkComponentType<WorkPlaceWorkerData> WorkPlaceWorkerDataType;

        [ReadOnly]
        public NativeArray<Entity> WorkingCitizens;
        [ReadOnly]
        public NativeArray<CitizenWork> WorkingCitizensWorkerData;

        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var entities = chunk.GetNativeArray(EntityType);
            var workPlaceWorkerDatas = chunk.GetNativeArray(WorkPlaceWorkerDataType);

            for (int i = 0; i < chunk.Count; i++)
            {
                WorkPlaceWorkerData workPlaceWorkerData = workPlaceWorkerDatas[i];

                for (int j = 0; j < WorkingCitizensWorkerData.Length; j++)
                {
                    if (workPlaceWorkerData.CurrentWorkers <= 0)
                        break;

                    var workerData = WorkingCitizensWorkerData[j];
                    if (workerData.WorkPlaceEntity.Index == entities[i].Index)
                    {
                        CommandBuffer.AddComponent<RemoveFromWorkTag>(chunkIndex, WorkingCitizens[j]);
                        workPlaceWorkerData.CurrentWorkers--;
                    }
                }

                if (workPlaceWorkerData.CurrentWorkers <= 0)
                    CommandBuffer.DestroyEntity(chunkIndex, entities[i]);
            }
        }
    }
}
