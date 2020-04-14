using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class CitizenArrivedAtWork : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var CommandBuffer = bufferSystem.CreateCommandBuffer();

        Entities.WithAll<HasArrivedAtDestinationTag>().ForEach((Entity entity, ref CitizenWork citizenWork) =>
        {
            if (EntityManager.HasComponent<WorkPlaceWorkerData>(citizenWork.WorkPlaceEntity))
            {
                CommandBuffer.RemoveComponent<GoingToWorkTag>(entity);
                CommandBuffer.AddComponent<IsWorkingTag>(entity);

                var workerData = EntityManager.GetComponentData<WorkPlaceWorkerData>(citizenWork.WorkPlaceEntity);
                workerData.ActiveWorkers++;
                CommandBuffer.SetComponent(citizenWork.WorkPlaceEntity, workerData);
                CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(entity);
            }
            else
            {
                CommandBuffer.AddComponent<RemoveFromWorkTag>(entity);
            }

        }).WithoutBurst().Run();
    }
}
