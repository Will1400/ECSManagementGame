using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class UnitControlSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        JobHandle job = new JobHandle();
        if (Input.GetMouseButtonDown(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float3 targetPosition = ECSRaycast.Raycast(ray.origin, ray.direction * 100).Position;

            job = new SetPositionJob
            {
                targetPosition = targetPosition
            }.Schedule(this, inputDeps);
        }

        return job;
    }

    struct SetPositionJob : IJobForEachWithEntity<MoveTowards>
    {
        public float3 targetPosition;

        public void Execute(Entity entity, int index, ref MoveTowards moveTowards)
        {
            moveTowards.TargetPosition = targetPosition;
        }
    }
}
