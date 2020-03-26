using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;

public class ConstructionFinishedSystem : ComponentSystem
{
    EntityQuery allFinishedSitesQuery;

    protected override void OnCreate()
    {
        allFinishedSitesQuery = Entities.WithAll<UnderConstruction, WorkPlaceWorkerData, Translation>()
                        .WithAllReadOnly<ConstructionFinishedTag>()
                        .ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        Entities.With(allFinishedSitesQuery).ForEach((Entity entity, ref UnderConstruction construction, ref WorkPlaceWorkerData workerData, ref Translation translation) =>
        {
            Entity finishedEntity = EntityPrefabManager.Instance.SpawnEntityPrefab(construction.finishedPrefabName.ToString());
            translation.Value.y = EntityManager.GetComponentData<Translation>(finishedEntity).Value.y;

            if (EntityManager.HasComponent<WorkPlaceWorkerData>(finishedEntity))
            {
                WorkPlaceWorkerData newWorkerData = EntityManager.GetComponentData<WorkPlaceWorkerData>(finishedEntity);
                newWorkerData.WorkPosition = workerData.WorkPosition;
                EntityManager.SetComponentData(finishedEntity, newWorkerData);
            }

            EntityManager.AddComponentData(finishedEntity, new Translation { Value = translation.Value });

            var occupation = EntityManager.GetComponentData<GridOccupation>(entity);
            EntityManager.AddComponentData(finishedEntity, new GridOccupation { Start = occupation.Start, End = occupation.End });

            workerData.ActiveWorkers = -1;
            EntityManager.RemoveComponent<UnderConstruction>(entity);
            EntityManager.AddComponent<RemoveWorkPlaceTag>(entity);
        });
    }
}
