using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[UpdateAfter(typeof(CitizenWorkMovement))]
public class CitizenArrivedAtWork : ComponentSystem
{
    EntityQuery citizensGoingToWork;

    protected override void OnCreate()
    {
        citizensGoingToWork = Entities.WithAll<Citizen, GoingToWorkTag, CitizenWork, Translation>().ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        Entities.With(citizensGoingToWork).ForEach((Entity entity, ref CitizenWork citizenWork, ref Translation translation) =>
        {
            float3 correctedTranslation = translation.Value;
            correctedTranslation.y = citizenWork.WorkPosition.y;
            float distance = math.distance(correctedTranslation, citizenWork.WorkPosition);
            if (distance <= .5f)
            {
                EntityManager.RemoveComponent<GoingToWorkTag>(entity);
                EntityManager.AddComponent<IsWorkingTag>(entity);
                var workerData = EntityManager.GetComponentData<WorkPlaceWorkerData>(citizenWork.WorkPlaceEntity);
                workerData.ActiveWorkers++;
                EntityManager.AddComponentData(citizenWork.WorkPlaceEntity, workerData);
            }
        });
    }
}
