using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class NavAgentMovementSystem : ComponentSystem
{
    EntityQuery movingNavAgents;

    protected override void OnCreate()
    {
        movingNavAgents = Entities.WithAll<NavAgent, MoveSpeed, Translation, AgentHasPath>().ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        Entities.With(movingNavAgents).ForEach((Entity entity, DynamicBuffer<Float3BufferElement> buffer, ref NavAgent agent, ref MoveSpeed speed, ref Translation translation) =>
        {

            if (agent.CurrentWaypointIndex >= buffer.Length)
            {
                agent.Status = AgentStatus.Idle;
                EntityManager.RemoveComponent<AgentHasPath>(entity);

                return;
            }

            float3 destination = buffer[agent.CurrentWaypointIndex];
            destination.y = 1.5f;

            if (math.distance(translation.Value, destination) > .4f)
            {
                float3 direction = math.normalize(destination - translation.Value);
                direction.y = 0;
                translation.Value += direction * speed.Value * Time.DeltaTime;
            }
            else
            {
                agent.CurrentWaypointIndex++;
            }
        });
    }
}
