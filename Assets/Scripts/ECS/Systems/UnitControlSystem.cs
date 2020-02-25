using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Collections;

public class UnitControlSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float3 targetPosition = ECSRaycast.Raycast(ray.origin, ray.direction * 100).Position;

            int row = -1;
            int i = 0;
            Entities.ForEach((ref MoveTowards moveTowards) =>
            {
                if (i % 10 == 0)
                    row++;

                float3 offset = new float3(1.5f, 0, 0) * (i % 10) + new float3(0, 0, row * 2);

                moveTowards.TargetPosition = targetPosition + offset;
                i++;
            });
        }
    }
}
