using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

[UpdateInGroup(typeof(ResourceStorageInteractionGroup))]
public class RemoveResourceFromStorageSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityCommandBuffer CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
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
        CommandBuffer.Dispose();
    }
}
