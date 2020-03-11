using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;

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
        Entities.With(needsWorkersQuery).ForEach((Entity workPlace, ref BuildingWorkerData workerData) =>
        {
            if (workerData.CurrentWorkers < workerData.MaxWorkers)
            {
                int currentWorkers = workerData.CurrentWorkers;
                BuildingWorkerData tempWorkerData = workerData;
                Entities.With(idleCitizensQuery).ForEach((Entity citizen) =>
                {
                    if (currentWorkers < tempWorkerData.MaxWorkers)
                    {
                        EntityManager.AddComponent<GoingToWorkTag>(citizen);
                        EntityManager.AddComponent<CitizenWork>(citizen);
                        EntityManager.AddComponent<NavAgentRequestingPath>(citizen);

                        EntityManager.AddComponentData(citizen, new CitizenWork { WorkPlaceEntity = workPlace, WorkPosition = tempWorkerData.WorkPosition });
                        EntityManager.AddComponentData(citizen, new NavAgentRequestingPath { StartPosition = EntityManager.GetComponentData<Translation>(citizen).Value, EndPosition = tempWorkerData.WorkPosition});
                        currentWorkers++;
                        EntityManager.RemoveComponent<IdleTag>(citizen);
                    }
                });
                workerData.CurrentWorkers = currentWorkers;
            }
        });
    }
}
