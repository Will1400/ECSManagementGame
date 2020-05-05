using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Rendering;

[UpdateBefore(typeof(GridValidationSystem))]
public class PlacementPositionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float3 mouseWorldPosition = ECSRaycast.Raycast(ray.origin, ray.direction * 9999, 1u << 9).Position;
        mouseWorldPosition = math.round(mouseWorldPosition);

        Entities.WithAll<BeingPlacedTag>().WithNone<IsInCache>().ForEach((ref Translation translation, ref GridOccupation gridOccupation, ref WorldRenderBounds renderBounds) =>
        {
            AABB bounds = renderBounds.Value;
            var result = GridHelper.CalculateGridOccupationFromBounds(bounds);

            gridOccupation.Start = new int2(result.x, result.y);
            gridOccupation.End = new int2(result.z, result.w);

            var heightAdjustedPosition = mouseWorldPosition;
            heightAdjustedPosition.y = translation.Value.y;

            translation.Value = heightAdjustedPosition;
        }).Schedule(Dependency).Complete();
    }
}
