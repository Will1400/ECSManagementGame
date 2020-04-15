﻿using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

public class DestroyResourceInStorageSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery resourcesInStorageQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        resourcesInStorageQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceData), typeof(ResourceInStorageData) }
        });
    }

    protected override void OnUpdate()
    {
        if (resourcesInStorageQuery.CalculateChunkCount() <= 0 || resourcesInStorageQuery.CalculateChunkCount() <= 0)
            return;

        var CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent();
        NativeArray<ResourceInStorageData> resourcesInStorage = resourcesInStorageQuery.ToComponentDataArray<ResourceInStorageData>(Allocator.TempJob);
        NativeArray<Entity> resourcesInStorageEntities = resourcesInStorageQuery.ToEntityArray(Allocator.TempJob);

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref DestroyResourceInStorageData resourceToDestroyData) =>
        {
            for (int i = 0; i < resourcesInStorage.Length; i++)
            {
                ResourceInStorageData resource = resourcesInStorage[i];
                if (resourcesInStorage[i].StorageEntity.Index == resourceToDestroyData.StorageId)
                {
                    CommandBuffer.DestroyEntity(entityInQueryIndex, resourcesInStorageEntities[i]);
                }
            }
            CommandBuffer.DestroyEntity(entityInQueryIndex, entity);

        }).Schedule(Dependency).Complete();

        resourcesInStorage.Dispose();
        resourcesInStorageEntities.Dispose();
    }
}
