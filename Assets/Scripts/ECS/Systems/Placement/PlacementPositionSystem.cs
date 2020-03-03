using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Rendering;

public class PlacementPositionSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float3 mouseWorldPosition = ECSRaycast.Raycast(ray.origin, ray.direction * 999, 1u << 9).Position;
        mouseWorldPosition = math.round(mouseWorldPosition);

        MoveJob job = new MoveJob
        {
            position = mouseWorldPosition
        };

        return job.Schedule(this, inputDeps);
    }

    [BurstCompile]
    [RequireComponentTag(typeof(BeingPlacedTag))]
    public struct MoveJob : IJobForEachWithEntity<Translation, GridOccupation, WorldRenderBounds>
    {
        [ReadOnly]
        public float3 position;

        public void Execute(Entity entity, int index, ref Translation translation, [WriteOnly] ref GridOccupation gridOccupation, [ReadOnly] ref WorldRenderBounds renderBounds)
        {
            if (!translation.Value.Equals(position))
            {
                AABB bounds = renderBounds.Value;
                int2 x = new int2((int)math.round(bounds.Center.x - bounds.Extents.x), (int)math.round(bounds.Center.x + bounds.Extents.x));
                int2 y = new int2((int)math.round(bounds.Center.z - bounds.Extents.z), (int)math.round(bounds.Center.z + bounds.Extents.z));

                gridOccupation.Start = new int2(x.x, y.x);
                gridOccupation.End = new int2(x.y, y.y);
                translation.Value = position;
            }
        }
    }
}
