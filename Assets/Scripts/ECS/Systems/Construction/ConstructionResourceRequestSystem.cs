using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

public class ConstructionResourceRequestSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery allConstructionSitesQuery;
    EntityQuery resourcesInStorageQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        allConstructionSitesQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(UnderConstruction) },
            None = new ComponentType[] { typeof(HasRequestedResourcesTag) }
        });

        resourcesInStorageQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceInStorage) },
        });
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.Concurrent CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent();

        Entities.WithAll<UnderConstruction>().WithNone<HasRequestedResourcesTag>().ForEach((Entity entity, int entityInQueryIndex, DynamicBuffer<ResourceCostElement> resourceCosts) =>
        {
            for (int i = 0; i < resourceCosts.Length; i++)
            {
                if (resourceCosts[i].Value.Amount == 0)
                    continue;

                var requestEntity = CommandBuffer.CreateEntity(entityInQueryIndex);
                CommandBuffer.AddComponent<ResourceRequest>(entityInQueryIndex, requestEntity);
                CommandBuffer.SetComponent(entityInQueryIndex, requestEntity, new ResourceRequest
                {
                    Amount = resourceCosts[i].Value.Amount,
                    RequestingEntity = entity,
                    ResourceType = resourceCosts[i].Value.ResourceType
                });
            }
            CommandBuffer.AddComponent<HasRequestedResourcesTag>(entityInQueryIndex, entity);
        }).Schedule(Dependency).Complete();
    }
}
