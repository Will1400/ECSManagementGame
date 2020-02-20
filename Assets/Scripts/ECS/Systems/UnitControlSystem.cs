using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

public class UnitControlSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float3 pos = ECSRaycast.Raycast(ray.origin, ray.direction * 100).Position;

            Entities.ForEach((ref MoveTowardsComponent moveTowards) =>
            {
                moveTowards.TargetPosition = pos;
            });
        }
    }
}
