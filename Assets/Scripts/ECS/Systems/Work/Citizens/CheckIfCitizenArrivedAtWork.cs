using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;

public class CheckIfCitizenArrivedAtWork : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new CheckIfArrivedAtWorkJob
        {
            //CommandBuffer = commandBuffer.ToConcurrent()
            CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, inputDeps);

        job.Complete();
        return job;
    }

    [RequireComponentTag(typeof(GoingToWorkTag))]
    struct CheckIfArrivedAtWorkJob : IJobForEachWithEntity<Citizen, CitizenWork, Translation>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, ref Citizen citizen, ref CitizenWork citizenWork, ref Translation translation)
        {
            float3 correctedTranslation = translation.Value;
            correctedTranslation.y = citizenWork.WorkPosition.y;
            float distance = math.distance(correctedTranslation, citizenWork.WorkPosition);

            if (distance <= .5f)
            {
                CommandBuffer.AddComponent<HasArrivedAtWorkTag>(index, entity);
            }
        }
    }
}
