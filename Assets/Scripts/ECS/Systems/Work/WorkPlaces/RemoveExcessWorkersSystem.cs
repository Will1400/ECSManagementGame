using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

public class RemoveExcessWorkersSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery workplaceQuery;
    EntityQuery workingCitizensQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();


        workplaceQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(WorkplaceWorkerData) }
        });
        workingCitizensQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(CitizenWork) },
            None = new ComponentType[] { typeof(RemoveFromWorkTag) }
        });
    }

    protected override void OnUpdate()
    {
        var buffer = bufferSystem.CreateCommandBuffer();
        var CommandBuffer = buffer.ToConcurrent();

        NativeArray<Entity> citizens = workingCitizensQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<CitizenWork> citizenWorks = workingCitizensQuery.ToComponentDataArray<CitizenWork>(Allocator.TempJob);

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref WorkplaceWorkerData workerData) =>
        {
            if (workerData.CurrentWorkers > workerData.MaxWorkers)
            {
                for (int i = 0; i < citizenWorks.Length; i++)
                {
                    if (workerData.CurrentWorkers <= workerData.MaxWorkers)
                        break;

                    if (citizenWorks[i].WorkplaceEntity == entity)
                    {
                        CommandBuffer.AddComponent<RemoveFromWorkTag>(entityInQueryIndex, citizens[i]);
                        CommandBuffer.SetComponent(entityInQueryIndex, citizens[i], new CitizenWork { });
                        workerData.ActiveWorkers--;
                        workerData.CurrentWorkers--;
                    }
                }
            }
        }).Schedule(Dependency).Complete();
        //}).Run();

        buffer.Playback(EntityManager);
        buffer.ShouldPlayback = false;

        citizens.Dispose();
        citizenWorks.Dispose();
    }
}
