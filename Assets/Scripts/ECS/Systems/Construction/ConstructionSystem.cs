using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class ConstructionSystem : ComponentSystem
{
    EntityQuery allSitesQuery;
    EntityQuery sitesNeedingWorkersQuery;

    protected override void OnCreate()
    {
        sitesNeedingWorkersQuery = Entities.WithAll<UnderConstruction, NeedsWorkersTag, Translation>()
                        .ToEntityQuery();

        allSitesQuery = Entities.WithAll<UnderConstruction, Translation>()
                        .ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        Entities.With(allSitesQuery).ForEach((Entity entity, ref UnderConstruction construction, ref Translation translation) =>
        {
            if (construction.currentWorkers >= 0)
            {
                construction.remainingConstructionTime -= Time.DeltaTime;
            }
            if (construction.remainingConstructionTime <= 0)
            {
                var prefab = PrefabManager.Instance.GetPrefabByName(construction.finishedPrefabName.ToString());
                GameObject.Instantiate(prefab, translation.Value, prefab.transform.rotation).AddComponent(typeof(ConvertToEntity));
                construction.currentWorkers = -1;
                EntityManager.DestroyEntity(entity);
            }
        });
    }
}
