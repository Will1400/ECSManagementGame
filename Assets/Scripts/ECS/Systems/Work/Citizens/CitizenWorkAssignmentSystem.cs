using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public class CitizenWorkAssignmentSystem : ComponentSystem
{
    EntityQuery idleCitizensQuery;
    EntityQuery needsWorkersQuery;

    protected override void OnCreate()
    {
        idleCitizensQuery = Entities.WithAll<Citizen, IdleTag>()
                                    .ToEntityQuery();
        needsWorkersQuery = Entities.WithAll<WorkPlaceWorkerData>()
                                    .WithNone<RemoveWorkPlaceTag, BeingPlacedTag>()
                                    .ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        var idleCitizens = Entities.With(idleCitizensQuery).ToEntityQuery().ToEntityArray(Allocator.TempJob);
        int citizenIndex = 0;
        Entities.With(needsWorkersQuery).ForEach((Entity workPlace, ref WorkPlaceWorkerData workerData) =>
        {
            if (idleCitizens.Length == 0)
                return;

            if (workerData.CurrentWorkers < workerData.MaxWorkers)
            {
                int currentWorkers = workerData.CurrentWorkers;

                WorkPlaceWorkerData tempWorkerData = workerData;

                Entity citizen;
                for (int i = citizenIndex; i < idleCitizens.Length; i++)
                {
                    if (currentWorkers < tempWorkerData.MaxWorkers)
                    {
                        citizen = idleCitizens[i];
                        EntityManager.AddComponent<GoingToWorkTag>(citizen);
                        EntityManager.AddComponent<CitizenWork>(citizen);
                        EntityManager.AddComponent<NavAgentRequestingPath>(citizen);

                        EntityManager.AddComponentData(citizen, new CitizenWork { WorkPlaceEntity = workPlace, WorkPosition = tempWorkerData.WorkPosition });
                        EntityManager.AddComponentData(citizen, new NavAgentRequestingPath { StartPosition = EntityManager.GetComponentData<Translation>(citizen).Value, EndPosition = tempWorkerData.WorkPosition });
                        currentWorkers++;
                        EntityManager.RemoveComponent<IdleTag>(citizen);

                        citizenIndex++;
                    }
                    else
                    {
                        break;
                    }
                }
                workerData.CurrentWorkers = currentWorkers;
            }
        });

        idleCitizens.Dispose();
    }
}
