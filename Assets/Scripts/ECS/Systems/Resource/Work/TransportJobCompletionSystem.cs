using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(EmptyResourceStorageJobCreationSystem))]
[UpdateBefore(typeof(ResourceRequestTransportJobCreationSystem))]
[UpdateInGroup(typeof(WorkCreationGroup))]
public class TransportJobCompletionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref ResourceTransportJobData resourceTransportJob) =>
        {
            if (math.all(resourceTransportJob.ResourcePosition == float3.zero))
            {
                resourceTransportJob.ResourcePosition = EntityManager.GetComponentData<Translation>(resourceTransportJob.ResourceEntity).Value;
            }
            if (math.all(resourceTransportJob.DestinationPosition == float3.zero))
            {
                resourceTransportJob.DestinationPosition = EntityManager.GetComponentData<Translation>(resourceTransportJob.DestinationEntity).Value;
            }
        }).WithoutBurst().Run();
    }
}
