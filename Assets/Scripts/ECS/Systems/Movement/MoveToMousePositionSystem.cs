using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

public class MoveToMousePositionSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hit = ECSRaycast.Raycast(ray.origin, ray.direction * 999, 1u << 9);
        float3 mouseWorldPosition = hit.Position;

        Job job = new Job { position = mouseWorldPosition };
        return job.Schedule(this, inputDeps);
    }

    [RequireComponentTag(typeof(FollowsMousePositionTag))]
    public struct Job : IJobForEachWithEntity<Translation>
    {
        public float3 position;

        public void Execute(Entity entity, int index, ref Translation translation)
        {
            translation.Value = position;
        }
    }
}
