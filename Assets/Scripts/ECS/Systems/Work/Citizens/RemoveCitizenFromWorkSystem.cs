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
            if (citizenWork.WorkPlaceEntity != Entity.Null && EntityManager.Exists(citizenWork.WorkPlaceEntity) & EntityManager.HasComponent<WorkPlaceWorkerData>(citizenWork.WorkPlaceEntity))
            {
                var index = citizenWork.WorkPlaceEntity.Index;
                var workerData = EntityManager.GetComponentData<WorkPlaceWorkerData>(citizenWork.WorkPlaceEntity);

                workerData.ActiveWorkers--;
                workerData.CurrentWorkers--;

                CommandBuffer.SetComponent(citizenWork.WorkPlaceEntity, workerData);
            }

            CommandBuffer.RemoveComponent<IsWorkingTag>(entity);
            CommandBuffer.RemoveComponent<RemoveFromWorkTag>(entity);
            CommandBuffer.RemoveComponent<CitizenWork>(entity);

            if (EntityManager.HasComponent<GoingToWorkTag>(entity))
                CommandBuffer.RemoveComponent<GoingToWorkTag>(entity);

            CommandBuffer.AddComponent<IdleTag>(entity);
        }).WithoutBurst().Run();
    }
}
