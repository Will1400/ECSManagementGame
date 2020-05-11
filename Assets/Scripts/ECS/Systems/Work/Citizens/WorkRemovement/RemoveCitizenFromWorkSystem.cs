using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

[UpdateAfter(typeof(CitizenArrivedAtWork))]
public class RemoveCitizenFromWorkSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        Entities.WithAll<RemoveFromWorkTag>().ForEach((Entity entity, ref CitizenWork citizenWork) =>
        {
            if (citizenWork.WorkplaceEntity != Entity.Null && EntityManager.Exists(citizenWork.WorkplaceEntity) & EntityManager.HasComponent<WorkplaceWorkerData>(citizenWork.WorkplaceEntity))
            {
                var index = citizenWork.WorkplaceEntity.Index;
                var workerData = EntityManager.GetComponentData<WorkplaceWorkerData>(citizenWork.WorkplaceEntity);

                if (citizenWork.IsWorking)
                    workerData.ActiveWorkers--;

                workerData.CurrentWorkers--;

                EntityManager.SetComponentData(citizenWork.WorkplaceEntity, workerData);
            }

            CommandBuffer.RemoveComponent<IsWorkingTag>(entity);
            CommandBuffer.RemoveComponent<RemoveFromWorkTag>(entity);
            CommandBuffer.RemoveComponent<CitizenWork>(entity);

            CommandBuffer.AddComponent<IdleTag>(entity);
        }).WithoutBurst().Run();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();
    }
}
