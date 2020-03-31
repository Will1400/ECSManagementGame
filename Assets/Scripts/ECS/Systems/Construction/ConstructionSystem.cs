﻿using System.Collections;
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

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref UnderConstruction construction, ref WorkPlaceWorkerData workerData) =>
        {
            if (workerData.ActiveWorkers >= 0)
            {
                construction.remainingConstructionTime -= deltaTime * (workerData.ActiveWorkers / .5f);
            }

            if (construction.remainingConstructionTime <= 0)
            {
                CommandBuffer.AddComponent<ConstructionFinishedTag>(entityInQueryIndex, entity);
            }
        }).Schedule();
    }
}
