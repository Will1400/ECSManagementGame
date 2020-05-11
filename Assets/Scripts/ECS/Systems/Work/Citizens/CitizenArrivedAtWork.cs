﻿using UnityEngine;
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
        var CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        Entities.WithAll<HasArrivedAtDestinationTag>().ForEach((Entity entity, ref CitizenWork citizenWork) =>
        {
            if (EntityManager.HasComponent<WorkplaceWorkerData>(citizenWork.WorkplaceEntity))
            {
                CommandBuffer.AddComponent<IsWorkingTag>(entity);

                citizenWork.IsWorking = true;

                var workerData = EntityManager.GetComponentData<WorkplaceWorkerData>(citizenWork.WorkplaceEntity);
                workerData.ActiveWorkers++;
                CommandBuffer.SetComponent(citizenWork.WorkplaceEntity, workerData);
                CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(entity);
            }
            else
            {
                CommandBuffer.AddComponent<RemoveFromWorkTag>(entity);
            }

        }).WithoutBurst().Run();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();
    }
}
