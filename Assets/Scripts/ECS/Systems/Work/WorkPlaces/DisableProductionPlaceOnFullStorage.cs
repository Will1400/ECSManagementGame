using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

[UpdateBefore(typeof(FireAllWorkersFromWorkplaceSystem))]
public class DisableProductionPlaceOnFullStorage : SystemBase
{
    protected override void OnUpdate()
    {
        var CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        Entities.WithAny<ResourceProductionData, FoodProductionData>().ForEach((Entity entity, ref ResourceStorageData resourceStorage, ref WorkplaceWorkerData workerData) =>
        {
            if (resourceStorage.UsedCapacity >= resourceStorage.MaxCapacity)
            {
                workerData.IsWorkable = false;

                if (workerData.ActiveWorkers > 0)
                    CommandBuffer.AddComponent<FireAllWorkersTag>(entity);
            }
            else
            {
                workerData.IsWorkable = true;
            }

        }).Schedule(Dependency).Complete();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();
    }
}
