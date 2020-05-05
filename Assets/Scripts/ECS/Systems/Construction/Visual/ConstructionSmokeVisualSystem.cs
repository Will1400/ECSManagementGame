using UnityEngine;
using System.Collections;
using Unity.Entities;
using System.Collections.Generic;
using UnityEngine.VFX;

[UpdateInGroup(typeof(VisualGroup))]
public class ConstructionSmokeVisualSystem : SystemBase
{
    List<VisualEffect> effectBuffer;

    VisualEffectAsset effectAsset;

    protected override void OnCreate()
    {
        effectAsset = Resources.Load<VisualEffectAsset>("VFX/ConstructionSmoke");
        effectBuffer = new List<VisualEffect>();
    }

    protected override void OnUpdate()
    {
        int i = 0;
        Entities.ForEach((Entity entity, ref ConstructionData constructionData, ref WorkplaceWorkerData workerData, ref GridOccupation gridOccupation) =>
        {
            if (i >= effectBuffer.Count)
            {
                var smokeObject = new GameObject("ConstructionSmoke");

                var effect = smokeObject.AddComponent<VisualEffect>();
                effect.visualEffectAsset = effectAsset;

                effect.Stop();
                effectBuffer.Add(effect);
            }

            if (workerData.ActiveWorkers > 0)
            {
                effectBuffer[i].Play();
                effectBuffer[i].SetVector3("BoxSize", new Vector3(gridOccupation.End.x - gridOccupation.Start.x, 2, gridOccupation.End.y - gridOccupation.Start.y));

                i++;
            }
        }).WithoutBurst().Run();

        for (int j = i; j < effectBuffer.Count; j++)
        {
            effectBuffer[j].Stop();
        }
    }
}
