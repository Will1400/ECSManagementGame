using UnityEngine;
using System.Collections;
using Unity.Entities;

public class RemoveCitizenFromWorkSystem : ComponentSystem
{
    EntityQuery citizens;
    EntityQuery workPlaces;

    protected override void OnCreate()
    {
        citizens = Entities.WithAll<RemoveFromWorkTag, CitizenWork>()
                           .ToEntityQuery();
        workPlaces = Entities.WithAll<BuildingWorkerData>()
                          .ToEntityQuery();

    }

    protected override void OnUpdate()
    {

        Entities.With(citizens).ForEach((Entity entity, ref CitizenWork citizenWork) =>
        {
            if (citizenWork.WorkPlaceEntity != null)
            {
                var index = citizenWork.WorkPlaceEntity.Index;
                Entities.With(workPlaces).ForEach((Entity workPlaceEntity, ref BuildingWorkerData workerData) =>
                {
                    if (workPlaceEntity.Index == index)
                    {
                        workerData.ActiveWorkers--;
                        workerData.CurrentWorkers--;
                    }
                });
            }

            EntityManager.RemoveComponent<IsWorkingTag>(entity);
            EntityManager.RemoveComponent<RemoveFromWorkTag>(entity);
            EntityManager.RemoveComponent<CitizenWork>(entity);

            if (EntityManager.HasComponent<GoingToWorkTag>(entity))
                EntityManager.RemoveComponent<GoingToWorkTag>(entity);

            EntityManager.AddComponent<IdleTag>(entity);
        });
    }
}
