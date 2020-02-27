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
        needsWorkersQuery = Entities.WithAll<BuildingWorkerData>()
                                    .WithNone<RemoveWorkPlaceTag>()
                                    .ToEntityQuery();
    }

    protected override void OnUpdate()
    {

        Entities.With(idleCitizensQuery).ForEach((Entity entity, ref Citizen citizen) =>
        {
            Entities.With(needsWorkersQuery).ForEach((Entity workPlaceEntity, ref BuildingWorkerData workerData) =>
            {
                if (workerData.CurrentWorkers < workerData.MaxWorkers)
                {
                    EntityManager.AddComponent<GoingToWorkTag>(entity);
                    if (!EntityManager.HasComponent<CitizenWork>(entity))
                        EntityManager.AddComponent<CitizenWork>(entity);

                    EntityManager.AddComponentData(entity, new CitizenWork { WorkPlaceEntity = workPlaceEntity, WorkPosition = workerData.WorkPosition });

                    workerData.CurrentWorkers++;
                    EntityManager.RemoveComponent<IdleTag>(entity);
                }
            });
        });
    }
}
