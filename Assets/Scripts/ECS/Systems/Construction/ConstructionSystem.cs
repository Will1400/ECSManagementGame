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
        allSitesQuery = Entities.WithAll<UnderConstruction, Translation>()
                        .ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        Entities.With(allSitesQuery).ForEach((Entity entity, ref UnderConstruction construction, ref Translation translation) =>
        {
            if (construction.currentWorkers >= 0)
            {
                construction.remainingConstructionTime -= Time.DeltaTime * (construction.currentWorkers / .5f);
            }
            else if (construction.currentWorkers != -1 &&  !EntityManager.HasComponent(entity, typeof(NeedsWorkers)))
            {
                EntityManager.AddComponent(entity, typeof(NeedsWorkers));
            }

            if (construction.remainingConstructionTime <= 0)
            {
                var prefab = PrefabManager.Instance.GetPrefabByName(construction.finishedPrefabName.ToString());
                Object.Instantiate(prefab, translation.Value, prefab.transform.rotation).AddComponent(typeof(ConvertToEntity));
                construction.currentWorkers = -1;
                EntityManager.DestroyEntity(entity);
            }
        });
    }
}
