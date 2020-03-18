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
            Entity finishedEntity = ArcheTypeManager.Instance.GetSetupBuildingEntity(prefab.GetComponent<MeshFilter>().sharedMesh, prefab.GetComponent<Renderer>().sharedMaterial, prefab);

            EntityManager.AddComponentData(finishedEntity, new Translation { Value = translation.Value });

            var occupation = EntityManager.GetComponentData<GridOccupation>(entity);
            EntityManager.AddComponentData(finishedEntity, new GridOccupation { Start = occupation.Start, End = occupation.End });

            workerData.CurrentWorkers = -1;
            workerData.ActiveWorkers = -1;
            EntityManager.RemoveComponent<UnderConstruction>(entity);
            EntityManager.AddComponent<RemoveWorkPlaceTag>(entity);
        });
    }
}
