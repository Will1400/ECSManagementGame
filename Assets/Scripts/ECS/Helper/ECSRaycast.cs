using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using RaycastHit = Unity.Physics.RaycastHit;

public class ECSRaycast : MonoBehaviour
{
    public static RaycastHit Raycast(float3 fromPosition, float3 toPosition)
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
                CollidesWith = ~0u,
                GroupIndex = 0
            }
        };

        if (collisionWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit hit))
        {
            var entity = buildPhysicsWorld.PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity;
            return hit;
        }

        return hit;
    }
}
