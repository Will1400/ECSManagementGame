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
        movingNavAgents = Entities.WithAll<NavAgent, MoveSpeed, Translation>().ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        Entities.With(movingNavAgents).ForEach((Entity entity, ref NavAgent agent, ref MoveSpeed speed, ref Translation translation) =>
        {
            if (agent.Status == AgentStatus.Moving)
            {
                var buffer = EntityManager.GetBuffer<Float3BufferElement>(entity);

                if (agent.CurrentWaypointIndex >= buffer.Length)
                {
                    agent.Status = AgentStatus.Idle;
                    buffer.Clear();
                    return;
                }

                float3 destination = buffer[agent.CurrentWaypointIndex];
                destination.y = 1;

                if (math.distance(translation.Value, destination) > .4f)
                {
                    float3 direction = math.normalize(destination - translation.Value);
                    direction.y = 0;
                    translation.Value += direction * speed.Value * Time.DeltaTime;
                    //translation.Value = math.lerp(translation.Value, destination, speed.Value * Time.DeltaTime);
                }
                else
                {
                    agent.CurrentWaypointIndex++;
                }
            }
        });
    }
}
