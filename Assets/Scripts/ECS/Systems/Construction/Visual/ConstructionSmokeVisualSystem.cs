using UnityEngine;
using System.Collections;
using Unity.Entities;
using System.Collections.Generic;
using UnityEngine.VFX;
using Unity.Transforms;

[UpdateInGroup(typeof(VisualGroup))]
public class ConstructionSmokeVisualSystem : SystemBase
{
    List<VisualEffect> effectBuffer;

    VisualEffectAsset effectAsset;

    EntityQuery constructionSiteQuery;
    // Used to make the system always run
    EntityQuery defaultQuery;

    protected override void OnCreate()
    {
        effectAsset = Resources.Load<VisualEffectAsset>("VFX/ConstructionSmoke");
        effectBuffer = new List<VisualEffect>();

        constructionSiteQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ConstructionData), typeof(WorkplaceWorkerData), typeof(GridOccupation), typeof(Translation) }
        });

        defaultQuery = GetEntityQuery(new EntityQueryDesc
        {
        });
    }

    protected override void OnUpdate()
    {
        int neededEffects = 0;
        Entities.ForEach((Entity entity, ref ConstructionData constructionData, ref WorkplaceWorkerData workerData, ref GridOccupation gridOccupation, ref Translation translation) =>
        {
            if (workerData.ActiveWorkers > 0)
            {
                neededEffects++;
            }
        }).Run();

        for (int e = 0; e < neededEffects - effectBuffer.Count; e++)
        {
            var smokeObject = new GameObject("ConstructionSmoke");

            var effect = smokeObject.AddComponent<VisualEffect>();
            effect.visualEffectAsset = effectAsset;

            effect.Stop();
            effectBuffer.Add(effect);
        }

        int i = 0;
        Entities.ForEach((Entity entity, ref ConstructionData constructionData, ref WorkplaceWorkerData workerData, ref GridOccupation gridOccupation, ref Translation translation) =>
        {
            if (workerData.ActiveWorkers > 0 && i < effectBuffer.Count)
            {
                if (effectBuffer[i].gameObject.transform.position == Vector3.zero)
                {
                    effectBuffer[i].gameObject.transform.position = translation.Value;
                    effectBuffer[i].SetVector3("BoxSize", new Vector3(gridOccupation.End.x - gridOccupation.Start.x, 2, gridOccupation.End.y - gridOccupation.Start.y));
                    effectBuffer[i].Play();
                }
                i++;
            }
        }).WithoutBurst().Run();

        for (int j = i; j < effectBuffer.Count; j++)
        {
            effectBuffer[j].Stop();
            effectBuffer[j].gameObject.transform.position = Vector3.zero;
        }


        for (int e = 0; e < effectBuffer.Count - neededEffects; e++)
        {
            GameObject.Destroy(effectBuffer[e].gameObject);
            effectBuffer.RemoveAt(e);
        }
    }
}
