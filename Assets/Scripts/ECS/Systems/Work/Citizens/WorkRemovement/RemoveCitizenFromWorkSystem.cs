using UnityEngine;
using System.Collections;
using Unity.Entities;

public class RemoveCitizenFromWorkSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var CommandBuffer = bufferSystem.CreateCommandBuffer();

        Entities.WithAll<RemoveFromWorkTag>().ForEach((Entity entity, ref CitizenWork citizenWork) =>
        {
            if (citizenWork.WorkplaceEntity != Entity.Null && EntityManager.Exists(citizenWork.WorkplaceEntity) & EntityManager.HasComponent<WorkplaceWorkerData>(citizenWork.WorkplaceEntity))
            {
                var index = citizenWork.WorkplaceEntity.Index;
                var workerData = EntityManager.GetComponentData<WorkplaceWorkerData>(citizenWork.WorkplaceEntity);

                workerData.ActiveWorkers--;
                workerData.CurrentWorkers--;

                CommandBuffer.SetComponent(citizenWork.WorkplaceEntity, workerData);
            }

            CommandBuffer.RemoveComponent<IsWorkingTag>(entity);
            CommandBuffer.RemoveComponent<RemoveFromWorkTag>(entity);
            CommandBuffer.RemoveComponent<CitizenWork>(entity);

            CommandBuffer.AddComponent<IdleTag>(entity);
        }).WithoutBurst().Run();
    }
}
