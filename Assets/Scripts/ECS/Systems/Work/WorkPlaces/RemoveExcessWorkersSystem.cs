using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

[UpdateInGroup(typeof(WorkRemovementGroup))]
public class RemoveExcessWorkersSystem : SystemBase
{
    EntityQuery workingCitizensQuery;

    protected override void OnCreate()
    {
        workingCitizensQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(CitizenWork) },
            None = new ComponentType[] { typeof(RemoveFromWorkTag) }
        });
    }

    protected override void OnUpdate()
    {
        // Used to only run when there is excess workers
        bool isWorkerCountValid = true;
        Entities.ForEach((Entity entity, ref WorkplaceWorkerData workerData) =>
        {
            if (workerData.CurrentWorkers > workerData.MaxWorkers)
            {
                isWorkerCountValid = false;
                return;
            }
        }).Run();

        if (isWorkerCountValid)
            return;

        var buffer = new EntityCommandBuffer(Allocator.TempJob);
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
                        // Reset citizenWork, since the worker gets removed from the workplace here instead
                        CommandBuffer.SetComponent(entityInQueryIndex, citizens[i], new CitizenWork { });
                        workerData.ActiveWorkers--;
                        workerData.CurrentWorkers--;
                    }
                }
            }
        }).Schedule(Dependency).Complete();

        buffer.Playback(EntityManager);
        buffer.Dispose();

        citizens.Dispose();
        citizenWorks.Dispose();
    }
}
