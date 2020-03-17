using UnityEngine;
using System.Collections;
using Unity.Entities;

public class RemoveCitizenFromWorkSystem : ComponentSystem
{
    EntityQuery citizens;

    protected override void OnCreate()
    {
        citizens = Entities.WithAll<RemoveFromWorkTag, CitizenWork>()
                           .ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        Entities.With(citizens).ForEach((Entity entity, ref CitizenWork citizenWork) =>
        {
            if (citizenWork.WorkPlaceEntity != null)
            {
                var index = citizenWork.WorkPlaceEntity.Index;
                var workerData = EntityManager.GetComponentData<WorkPlaceWorkerData>(citizenWork.WorkPlaceEntity);

                workerData.ActiveWorkers--;
                workerData.CurrentWorkers--;

                EntityManager.SetComponentData(citizenWork.WorkPlaceEntity, workerData);
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
