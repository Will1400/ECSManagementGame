using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class ConstructionSystem : ComponentSystem
{
    EntityQuery allSitesQuery;

    protected override void OnCreate()
    {
        allSitesQuery = Entities.WithAll<UnderConstruction, BuildingWorkerData, Translation>()
                        .ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        Entities.With(allSitesQuery).ForEach((Entity entity, ref UnderConstruction construction, ref BuildingWorkerData workerData, ref Translation translation) =>
        {
            if (workerData.ActiveWorkers >= 0)
            {
                construction.remainingConstructionTime -= Time.DeltaTime * (workerData.ActiveWorkers / .5f);
            }
            else if (workerData.CurrentWorkers != -1 && !EntityManager.HasComponent(entity, typeof(BuildingWorkerData)))
            {
                EntityManager.AddComponent(entity, typeof(BuildingWorkerData));
            }

            if (construction.remainingConstructionTime <= 0)
            {
                var prefab = PrefabManager.Instance.GetPrefabByName(construction.finishedPrefabName.ToString());
                Object.Instantiate(prefab, translation.Value, prefab.transform.rotation).AddComponent(typeof(ConvertToEntity));
                workerData.CurrentWorkers = -1;
                workerData.ActiveWorkers = -1;
                EntityManager.RemoveComponent<UnderConstruction>(entity);
                EntityManager.AddComponent<RemoveWorkPlaceTag>(entity);
            }
        });
    }
}
