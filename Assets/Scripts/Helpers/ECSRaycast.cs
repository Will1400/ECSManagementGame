using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using RaycastHit = Unity.Physics.RaycastHit;
using System;

public class ECSRaycast : MonoBehaviour
{
    public static RaycastHit Raycast(float3 fromPosition, float3 toPosition, uint layerMask)
    {
        var buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();

        var collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

        RaycastInput raycastInput = new RaycastInput
        {
            Start = fromPosition,
            End = toPosition, 
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = layerMask,
                GroupIndex = 0
            }
        };

        if (collisionWorld.CastRay(raycastInput, out RaycastHit hit))
        {
            return hit;
        }

        return hit;
    }

    public static RaycastHit Raycast(float3 fromPosition, float3 toPosition)
    {
        return Raycast(fromPosition, toPosition, ~0u);
    }
}
