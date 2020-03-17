using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ConstructionSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new ConstructionJob
        {
            DeltaTime = Time.DeltaTime,
            CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent()
            
        }.Schedule(this, inputDeps);

        job.Complete();
        return job;
    }

    struct ConstructionJob : IJobForEachWithEntity<UnderConstruction, WorkPlaceWorkerData>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public float DeltaTime;

        public void Execute(Entity entity, int index, ref UnderConstruction construction, ref WorkPlaceWorkerData workerData)
        {
            if (workerData.ActiveWorkers >= 0)
            {
                construction.remainingConstructionTime -= DeltaTime * (workerData.ActiveWorkers / .5f);
            }

            if (construction.remainingConstructionTime <= 0)
            {
                CommandBuffer.AddComponent<ConstructionFinishedTag>(index, entity);
            }
        }
    }
}
