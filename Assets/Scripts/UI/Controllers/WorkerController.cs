using UnityEngine;
using System.Collections;
using Unity.Entities;
using TMPro;

public class WorkerController : MonoBehaviour
{
    private EntityManager EntityManager;
    private Entity entity;

    [SerializeField]
    private TextMeshProUGUI workerCountText;

    public void Initialize(Entity entity)
    {
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (entity == Entity.Null || !EntityManager.Exists(entity))
            Destroy(gameObject);

        this.entity = entity;
        InvokeRepeating("UpdateText", 0, 1);
    }

    void UpdateText()
    {
        var data = EntityManager.GetComponentData<WorkplaceWorkerData>(entity);

        workerCountText.text = $"{data.CurrentWorkers} of {data.MaxWorkers}";
    }

    public void IncreaseMaxWorkerLimit()
    {
        var data = EntityManager.GetComponentData<WorkplaceWorkerData>(entity);
        data.MaxWorkers++;
        EntityManager.SetComponentData(entity, data);

        UpdateText();
    }

    public void DecreaseMaxWorkerLimit()
    {
        var data = EntityManager.GetComponentData<WorkplaceWorkerData>(entity);
        data.MaxWorkers--;

        if (data.MaxWorkers < 0)
            return;

        EntityManager.SetComponentData(entity, data);

        UpdateText();
    }
}
