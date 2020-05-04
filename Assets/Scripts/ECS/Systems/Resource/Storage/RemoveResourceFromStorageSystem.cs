using UnityEngine;
using System.Collections;
using Unity.Entities;

public class RemoveResourceFromStorageSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer CommandBuffer = bufferSystem.CreateCommandBuffer();
        Entities.ForEach((Entity entity, ref RemoveResourceFromStorageData removeResourceFromStorage) =>
        {
            ResourceInStorageData resourceInStorageData = EntityManager.GetComponentData<ResourceInStorageData>(removeResourceFromStorage.ResourceEntity);
            var resourceStorage = EntityManager.GetComponentData<ResourceStorageData>(resourceInStorageData.StorageEntity);
            resourceStorage.UsedCapacity--;

            if (EntityManager.HasComponent<ResourceDataElement>(resourceInStorageData.StorageEntity))
            {
                var buffer = EntityManager.GetBuffer<ResourceDataElement>(resourceInStorageData.StorageEntity);

                for (int j = 0; j < buffer.Length; j++)
                {
                    if (buffer[j].Value == resourceInStorageData.ResourceData)
                    {
                        buffer.RemoveAt(j);
                        break;
                    }
                }
            }

            CommandBuffer.RemoveComponent<ResourceInStorageData>(removeResourceFromStorage.ResourceEntity);

            CommandBuffer.SetComponent(resourceInStorageData.StorageEntity, resourceStorage);

            CommandBuffer.DestroyEntity(entity);
        }).WithoutBurst().Run();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.ShouldPlayback = false;
    }
}
