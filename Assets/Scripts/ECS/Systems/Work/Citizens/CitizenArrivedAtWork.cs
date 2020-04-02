using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class CitizenArrivedAtWork : ComponentSystem
{
    EntityQuery citizensGoingToWork;

    protected override void OnCreate()
    {
        citizensGoingToWork = Entities.WithAll<Citizen, HasArrivedAtDestinationTag, CitizenWork>().ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        Entities.With(citizensGoingToWork).ForEach((Entity entity, ref CitizenWork citizenWork) =>
        {
            EntityManager.RemoveComponent<GoingToWorkTag>(entity);
            EntityManager.AddComponent<IsWorkingTag>(entity);
            var workerData = EntityManager.GetComponentData<WorkPlaceWorkerData>(citizenWork.WorkPlaceEntity);
            workerData.ActiveWorkers++;
            EntityManager.AddComponentData(citizenWork.WorkPlaceEntity, workerData);
            EntityManager.RemoveComponent<HasArrivedAtDestinationTag>(entity);
        });
    }
}
