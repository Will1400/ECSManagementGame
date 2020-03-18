using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;

public class ConstructionFinishedSystem : ComponentSystem
{
    EntityQuery allSitesQuery;

    protected override void OnCreate()
    {
        allSitesQuery = Entities.WithAll<UnderConstruction, WorkPlaceWorkerData, Translation>()
                        .WithAllReadOnly<ConstructionFinishedTag>()
                        .ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        Entities.With(allSitesQuery).ForEach((Entity entity, ref UnderConstruction construction, ref WorkPlaceWorkerData workerData, ref Translation translation) =>
        {
            var prefab = PrefabManager.Instance.GetPrefabByName(construction.finishedPrefabName.ToString());
            Entity finishedEntity = EntityCreationManager.Instance.GetSetupBuildingEntity(prefab);

            EntityManager.AddComponentData(finishedEntity, new Translation { Value = translation.Value });
            EntityManager.AddComponentData(finishedEntity, new Rotation { Value = prefab.transform.rotation });

            var occupation = EntityManager.GetComponentData<GridOccupation>(entity);
            EntityManager.AddComponentData(finishedEntity, new GridOccupation { Start = occupation.Start, End = occupation.End });

            workerData.ActiveWorkers = -1;
            EntityManager.RemoveComponent<UnderConstruction>(entity);
            EntityManager.AddComponent<RemoveWorkPlaceTag>(entity);
        });
    }
}
