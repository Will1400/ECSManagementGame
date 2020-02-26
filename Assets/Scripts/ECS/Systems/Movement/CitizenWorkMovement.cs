﻿using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class CitizenWorkMovement : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var moveJob = new MoveJob
        {
            deltatime = Time.DeltaTime
        };

        return moveJob.Schedule(this, inputDeps);
    }

    private struct MoveJob : IJobForEachWithEntity<GoingToWorkTag, CitizenWork, MoveSpeed, Translation, Rotation>
    {
        [ReadOnly]
        public float deltatime;

        public void Execute(Entity entity, int index, ref GoingToWorkTag goingToWork, ref CitizenWork work, ref MoveSpeed moveSpeed, ref Translation translation, ref Rotation rotation)
        {
            if (math.distance(translation.Value, work.WorkPosition) >= .2f)
            {
                float3 direction = math.normalize(work.WorkPosition- translation.Value);
                direction.y = 0;
                translation.Value += direction * moveSpeed.Speed * deltatime;
            }
            else
            {
            }
        }
    }
}
