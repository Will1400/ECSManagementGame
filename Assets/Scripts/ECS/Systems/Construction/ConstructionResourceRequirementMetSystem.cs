using UnityEngine;
using System.Collections;
using Unity.Entities;

public class ConstructionResourceRequirementMetSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent();

        Entities.WithAll<HasNoResourceCost>().ForEach((Entity entity, int entityInQueryIndex, ref WorkplaceWorkerData workerData) =>
        {
            workerData.IsWorkable = true;

            CommandBuffer.RemoveComponent<HasNoResourceCost>(entityInQueryIndex, entity);

        }).Schedule(Dependency).Complete();

        // Make workplace workable if resource requirement is met
        Entities.WithNone<ConstructionFinishedTag>().ForEach((Entity entity, DynamicBuffer<ResourceCostElement> resourceCosts, DynamicBuffer<ResourceDataElement> resourceDatas, ref WorkplaceWorkerData workerData) =>
        {
            if (workerData.IsWorkable)
                return;

            bool requirementsMet = true;
            for (int i = 0; i < resourceCosts.Length; i++)
            {
                ResourceCostData cost = resourceCosts[i];

                if (cost.Amount == 0)
                    continue;

                if (!requirementsMet)
                    break;

                int amount = 0;
                for (int d = 0; d < resourceDatas.Length; d++)
                {
                    if (resourceDatas[d].Value.ResourceType == cost.ResourceType)
                    {
                        amount += resourceDatas[d].Value.Amount;
                    }
                }

                if (amount < cost.Amount)
                    requirementsMet = false;
            }

            if (requirementsMet)
            {
                workerData.IsWorkable = true;
            }

        }).Schedule(Dependency).Complete();
    }
}
