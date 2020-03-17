using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public class NavAgentMovementSystem : JobComponentSystem
{

    EndSimulationEntityCommandBufferSystem bufferSystem;


    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new MoveJob
        {
            DeltaTime = Time.DeltaTime,
            CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, inputDeps);

        job.Complete();
        return job;
    }

    [RequireComponentTag(typeof(AgentHasPath))]
    struct MoveJob : IJobForEachWithEntity_EBCCC<Float3BufferElement, Translation, MoveSpeed, NavAgent>
    {
        public float DeltaTime;

        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, DynamicBuffer<Float3BufferElement> buffer, ref Translation translation, [ReadOnly]ref MoveSpeed speed, ref NavAgent agent)
        {
            if (agent.CurrentWaypointIndex >= buffer.Length)
            {
                agent.Status = AgentStatus.Idle;
                CommandBuffer.RemoveComponent(index, entity, typeof(AgentHasPath));
                return;
            }
            float3 destination = buffer[agent.CurrentWaypointIndex];
            destination.y = 1.5f;

            if (math.distance(translation.Value, destination) > .4f)
            {
                float3 direction = math.normalize(destination - translation.Value);
                direction.y = 0;
                translation.Value += direction * speed.Value * DeltaTime;
            }
            else
            {
                agent.CurrentWaypointIndex++;
            }
        }
    }
}
