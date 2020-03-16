using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class ConstructionSystem : ComponentSystem
{
    EntityQuery allSitesQuery;

    protected override void OnCreate()
    {
        allSitesQuery = Entities.WithAll<UnderConstruction, WorkPlaceWorkerData, Translation>()
                        .ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        Entities.With(allSitesQuery).ForEach((Entity entity, ref UnderConstruction construction, ref WorkPlaceWorkerData workerData, ref Translation translation) =>
        {
            if (workerData.ActiveWorkers >= 0)
            {
                construction.remainingConstructionTime -= Time.DeltaTime * (workerData.ActiveWorkers / .5f);
            }
            else if (workerData.CurrentWorkers != -1 && !EntityManager.HasComponent(entity, typeof(WorkPlaceWorkerData)))
            {
                EntityManager.AddComponent(entity, typeof(WorkPlaceWorkerData));
            }

            if (construction.remainingConstructionTime <= 0)
            {
                var prefab = PrefabManager.Instance.GetPrefabByName(construction.finishedPrefabName.ToString());
                var material = prefab.GetComponent<Renderer>().sharedMaterial;
                material.SetColor("_BaseColor", Color.white);
                Entity finishedEntity = ArcheTypeManager.Instance.GetSetupBuildingEntity(prefab.GetComponent<MeshFilter>().sharedMesh, prefab.GetComponent<Renderer>().sharedMaterial, prefab);


                EntityManager.AddComponentData(finishedEntity, new Translation { Value = translation.Value});

                var occupation = EntityManager.GetComponentData<GridOccupation>(entity);
                EntityManager.AddComponentData(finishedEntity, new GridOccupation { Start = occupation.Start, End = occupation.End});

                workerData.CurrentWorkers = -1;
                workerData.ActiveWorkers = -1;
                EntityManager.RemoveComponent<UnderConstruction>(entity);
                EntityManager.AddComponent<RemoveWorkPlaceTag>(entity);
            }
        });
    }
}
