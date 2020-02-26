using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

public class CitizenWorkAssignmentSystem : ComponentSystem
{
    EntityQuery idleCitizensQuery;
    EntityQuery needsWorkersQuery;

    protected override void OnCreate()
    {
        idleCitizensQuery = Entities.WithAll<Citizen, IdleTag>()
                                    .ToEntityQuery();
        needsWorkersQuery = Entities.WithAll<NeedsWorkers>()
                                   .ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        NativeArray<Entity> availableWorkPlaces = needsWorkersQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<NeedsWorkers> availableWork = needsWorkersQuery.ToComponentDataArray<NeedsWorkers>(Allocator.TempJob);

        int workIndex = 0;
        Entities.With(idleCitizensQuery).ForEach((Entity entity, ref Citizen citizen) =>
        {
            if (availableWork.Length > 0)
            {
                if (availableWork[workIndex].WorkersNeeded > 0)
                {
                    EntityManager.AddComponent<GoingToWorkTag>(entity);
                    if (!EntityManager.HasComponent<CitizenWork>(entity))
                        EntityManager.AddComponent<CitizenWork>(entity);

                    EntityManager.RemoveComponent<IdleTag>(entity);

                    EntityManager.AddComponentData(entity, new CitizenWork { WorkPlaceEntity = availableWorkPlaces[workIndex], WorkPosition = availableWork[workIndex].WorkPosition });
                }
            }
        });

        availableWorkPlaces.Dispose();
        availableWork.Dispose();
    }
}
