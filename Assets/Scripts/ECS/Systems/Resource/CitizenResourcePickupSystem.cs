﻿using UnityEngine;
using System.Collections;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(ResourceInteractionGroup))]
public class CitizenResourcePickupSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.Concurrent CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent();

        Entities.WithAll<HasArrivedAtDestinationTag, MovingToPickupResource>().ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref ResourceTransportJobData resourceTransportJob) =>
        {
            var removeEntity = CommandBuffer.CreateEntity(entityInQueryIndex);
            CommandBuffer.AddComponent<RemoveResourceFromStorageData>(entityInQueryIndex, removeEntity);
            CommandBuffer.SetComponent(entityInQueryIndex, removeEntity, new RemoveResourceFromStorageData
            {
                ResourceEntity = resourceTransportJob.ResourceEntity,
            });

            // Pickup
            CommandBuffer.RemoveComponent<MovingToPickupResource>(entityInQueryIndex, entity);
            CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(entityInQueryIndex, entity);


            CommandBuffer.AddComponent<IsCarryingResourceTag>(entityInQueryIndex, entity);

            // Move to destination
            CommandBuffer.AddComponent<NavAgentRequestingPath>(entityInQueryIndex, entity);
            CommandBuffer.SetComponent(entityInQueryIndex, entity, new NavAgentRequestingPath { StartPosition = translation.Value, EndPosition = resourceTransportJob.DestinationPosition });

        }).Schedule(Dependency).Complete();
    }
}
