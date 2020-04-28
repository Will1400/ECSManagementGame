﻿using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

[UpdateInGroup(typeof(WorkCreationGroup))]
public class EmptyResourceStorageJobCreationSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery resourcesInStorageQuery;
    EntityQuery StorageAreasQuery;
    EntityQuery resourceStoragesToEmptyQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        resourceStoragesToEmptyQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceStorageData), typeof(EmptyResourceStorageTag) },
        });

        resourcesInStorageQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceData), typeof(ResourceInStorageData) },
            None = new ComponentType[] { typeof(ResourceIsUnderTransportationTag) }
        });

        StorageAreasQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceStorageData), typeof(ResourceStorageAreaTag) },
            None = new ComponentType[] { typeof(ResourceStorageFullTag) }
        });
    }

    protected override void OnUpdate()
    {
        if (resourcesInStorageQuery.CalculateChunkCount() == 0 || StorageAreasQuery.CalculateChunkCount() == 0 || resourceStoragesToEmptyQuery.CalculateChunkCount() == 0)
            return;

        bool isResourceStorageEmpty = true;
        Entities.WithAll<EmptyResourceStorageTag>().ForEach((Entity entity, ref ResourceStorageData resourceStorage) =>
        {
            if (resourceStorage.UsedCapacity > 0)
            {
                isResourceStorageEmpty = false;
                return;
            }
        }).Run();

        if (isResourceStorageEmpty)
            return;

        NativeArray<Entity> resourcesInStorageEntities = resourcesInStorageQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<ResourceInStorageData> resourcesInStorage = resourcesInStorageQuery.ToComponentDataArray<ResourceInStorageData>(Allocator.TempJob);
        NativeArray<Entity> storageAreas = StorageAreasQuery.ToEntityArray(Allocator.TempJob);

        var CommandBuffer = bufferSystem.CreateCommandBuffer();

        Entities.WithAll<EmptyResourceStorageTag>().ForEach((Entity entity, int entityInQueryIndex, DynamicBuffer<ResourceDataElement> resourceDatas, ref ResourceStorageData resourceStorage) =>
        {
            if (resourceDatas.Length == 0)
                return;

            for (int i = 0; i < resourcesInStorage.Length; i++)
            {
                if (resourcesInStorage[i].StorageEntity == entity)
                {
                    int indexInBuffer = -1;
                    for (int r = 0; r < resourceDatas.Length; r++)
                    {
                        if (resourceDatas[r].Value == resourcesInStorage[i].ResourceData)
                        {
                            indexInBuffer = r;
                            break;
                        }
                    }

                    if (indexInBuffer != -1)
                    {
                        var jobEntity = CommandBuffer.CreateEntity();
                        CommandBuffer.AddComponent<ResourceTransportJobData>(jobEntity);
                        CommandBuffer.SetComponent(jobEntity, new ResourceTransportJobData
                        {
                            ResourceEntity = resourcesInStorageEntities[i],
                            DestinationEntity = storageAreas[0],
                        });

                        CommandBuffer.AddComponent<ResourceIsUnderTransportationTag>(resourcesInStorageEntities[i]);

                        resourceDatas.RemoveAt(indexInBuffer);
                    }
                }
            }
        }).Schedule(Dependency).Complete();

        resourcesInStorage.Dispose();
        resourcesInStorageEntities.Dispose();
        storageAreas.Dispose();
    }
}
