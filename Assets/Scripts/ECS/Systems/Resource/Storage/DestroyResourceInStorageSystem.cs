using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

[UpdateInGroup(typeof(ResourceStorageInteractionGroup))]
public class DestroyResourceInStorageSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery resourcesInStorageQuery;
    EntityQuery resourcesToDestroyQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        resourcesInStorageQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceData), typeof(ResourceInStorageData) }
        });

        resourcesToDestroyQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(DestroyResourceInStorageData) }
        });
    }

    protected override void OnUpdate()
    {
        if (resourcesInStorageQuery.CalculateEntityCount() == 0 || resourcesInStorageQuery.CalculateEntityCount() == 0 || resourcesToDestroyQuery.CalculateEntityCount() == 0)
            return;

        var CommandBuffer = bufferSystem.CreateCommandBuffer();
        NativeArray<Entity> resourcesInStorageEntities = resourcesInStorageQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<ResourceInStorageData> resourcesInStorageDatas = resourcesInStorageQuery.ToComponentDataArray<ResourceInStorageData>(Allocator.TempJob);

        Entities.ForEach((Entity entity, ref DestroyResourceInStorageData resourceToDestroyData) =>
        {
            for (int i = 0; i < resourcesInStorageDatas.Length; i++)
            {
                ResourceInStorageData resource = resourcesInStorageDatas[i];
                if (resourcesInStorageDatas[i].StorageEntity == resourceToDestroyData.StorageEntity)
                {
                    var storageEntity = EntityManager.GetComponentData<ResourceInStorageData>(resourcesInStorageEntities[i]).StorageEntity;
                    var resourceStorage = EntityManager.GetComponentData<ResourceStorageData>(storageEntity);
                    resourceStorage.UsedCapacity--;

                    if (EntityManager.HasComponent<ResourceDataElement>(resourceToDestroyData.StorageEntity))
                    {
                        var buffer = EntityManager.GetBuffer<ResourceDataElement>(resourceToDestroyData.StorageEntity);

                        for (int j = 0; j < buffer.Length; j++)
                        {
                            if (buffer[j].Value == resourcesInStorageDatas[i].ResourceData)
                            {
                                buffer.RemoveAt(j);
                                break;
                            }
                        }
                    }

                    CommandBuffer.DestroyEntity(resourcesInStorageEntities[i]);
                }
            }
            CommandBuffer.DestroyEntity(entity);

        }).WithoutBurst().Run();

        resourcesInStorageEntities.Dispose();
        resourcesInStorageDatas.Dispose();
    }
}
