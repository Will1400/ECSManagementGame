using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;

public class MoveTowardsSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var moveJob = new MoveJob
        {
            deltatime = Time.DeltaTime
        };

        return moveJob.Schedule(this, inputDeps);
    }

    private struct MoveJob : IJobForEachWithEntity<MoveTowards, Translation, MoveSpeed>
    {
        public float deltatime;

        public void Execute(Entity entity, int index, ref MoveTowards moveTowardsComponent, ref Translation translation, ref MoveSpeed moveSpeedComponent)
        {
            if (math.distance(translation.Value, moveTowardsComponent.TargetPosition) > .05f)
            {
                float3 direction = math.normalize(moveTowardsComponent.TargetPosition - translation.Value);
                direction.y = 0;
                translation.Value += direction * moveSpeedComponent.Speed * deltatime;
            }
        }
    }
}
