using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

[UpdateInGroup(typeof(ResourceStorageInteractionGroup))]
public class AddResourceToStorageSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityCommandBuffer CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        Entities.ForEach((Entity entity, ref AddResourceToStorageData addResourceToStorage) =>
        {
            ResourceData resourceData = EntityManager.GetComponentData<ResourceData>(addResourceToStorage.ResourceEntity);
            var position = EntityManager.GetComponentData<Translation>(addResourceToStorage.StorageEntity).Value;

            // Set storage capacity
            var resourceStorage = EntityManager.GetComponentData<ResourceStorageData>(addResourceToStorage.StorageEntity);
            resourceStorage.UsedCapacity++;
            CommandBuffer.SetComponent(addResourceToStorage.StorageEntity, resourceStorage);

            // Add to storage
            var resourceBuffer = EntityManager.GetBuffer<ResourceDataElement>(addResourceToStorage.StorageEntity);
            resourceBuffer.Add(new ResourceDataElement { Value = resourceData });

            CommandBuffer.AddComponent<ResourceInStorageData>(addResourceToStorage.ResourceEntity);
            CommandBuffer.SetComponent(addResourceToStorage.ResourceEntity, new ResourceInStorageData
            {
                StorageEntity = addResourceToStorage.StorageEntity,
                ResourceData = resourceData,
                StorageAreaStartPosition = position,
                StorageAreaEndPosition = position + new float3(1, 1, 1),
            });

            CommandBuffer.DestroyEntity(entity);
        }).WithoutBurst().Run();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();
    }
}
