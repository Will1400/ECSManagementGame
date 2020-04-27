using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ConstructionSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.Concurrent CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent();
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref ConstructionData construction, ref WorkplaceWorkerData workerData) =>
        {
            if (workerData.IsWorkable)
            {
                if (workerData.ActiveWorkers >= 0)
                {
                    construction.RemainingConstructionTime -= deltaTime * (workerData.ActiveWorkers / .5f);
                }

                if (construction.RemainingConstructionTime <= 0)
                {
                    CommandBuffer.AddComponent<ConstructionFinishedTag>(entityInQueryIndex, entity);
                    workerData.IsWorkable = false;
                }
            }
        }).Schedule(Dependency).Complete();
    }
}
