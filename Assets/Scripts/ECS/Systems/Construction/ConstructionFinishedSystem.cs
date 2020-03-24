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
            Entity finishedEntity;
            //if (construction.finishedPrefabName.ToString() != "TallHouse")
            //{
            //    var prefab = PrefabManager.Instance.GetPrefabByName(construction.finishedPrefabName.ToString());
            //    finishedEntity = EntityCreationManager.Instance.GetSetupBuildingEntity(prefab);
            //    translation.Value.y = prefab.transform.position.y;
            //    EntityManager.AddComponentData(finishedEntity, new Rotation { Value = prefab.transform.rotation });
            //}
            //else
            //{
            finishedEntity = EntityPrefabManager.Instance.SpawnEntityPrefab(construction.finishedPrefabName.ToString());
            translation.Value.y = EntityManager.GetComponentData<Translation>(finishedEntity).Value.y;
            //}

            EntityManager.AddComponentData(finishedEntity, new Translation { Value = translation.Value });

            var occupation = EntityManager.GetComponentData<GridOccupation>(entity);
            EntityManager.AddComponentData(finishedEntity, new GridOccupation { Start = occupation.Start, End = occupation.End });

            workerData.ActiveWorkers = -1;
            EntityManager.RemoveComponent<UnderConstruction>(entity);
            EntityManager.AddComponent<RemoveWorkPlaceTag>(entity);
        });
    }
}
