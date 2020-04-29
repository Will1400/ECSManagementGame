using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;

public class ConstructionFinishedSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var CommandBuffer = bufferSystem.CreateCommandBuffer();

        Entities.WithAll<ConstructionFinishedTag>().ForEach((Entity entity, DynamicBuffer<ResourceDataElement> resourceDatas, ref ConstructionData construction, ref WorkplaceWorkerData workerData, ref Translation translation) =>
        {
            for (int i = 0; i < resourceDatas.Length; i++)
            {
                var resource = resourceDatas[i].Value;
                var destroyEntity = CommandBuffer.CreateEntity();
                CommandBuffer.AddComponent<DestroyResourceInStorageData>(destroyEntity);
                CommandBuffer.SetComponent(destroyEntity, new DestroyResourceInStorageData
                {
                    ResourceData = resource,
                    StorageEntity = entity
                });
            }

            Entity finishedEntity = EntityPrefabManager.Instance.SpawnEntityPrefab(CommandBuffer, construction.FinishedPrefabName.ToString());
            Entity prefabEntity = EntityPrefabManager.Instance.GetEntityPrefab(construction.FinishedPrefabName.ToString());

            if (finishedEntity == Entity.Null)
            {
                CommandBuffer.DestroyEntity(entity);
                return;
            }

            translation.Value.y = EntityManager.GetComponentData<Translation>(prefabEntity).Value.y;
            CommandBuffer.SetComponent(finishedEntity, new Translation { Value = translation.Value });

            if (EntityManager.HasComponent<WorkplaceWorkerData>(prefabEntity))
            {
                WorkplaceWorkerData newWorkerData = EntityManager.GetComponentData<WorkplaceWorkerData>(prefabEntity);
                newWorkerData.WorkPosition = workerData.WorkPosition;
                CommandBuffer.SetComponent(finishedEntity, newWorkerData);
            }

            var occupation = EntityManager.GetComponentData<GridOccupation>(entity);
            CommandBuffer.SetComponent(finishedEntity, new GridOccupation { Start = occupation.Start, End = occupation.End });

            workerData.ActiveWorkers = -1;
            CommandBuffer.RemoveComponent<ConstructionData>(entity);
            CommandBuffer.AddComponent<RemoveWorkplaceTag>(entity);
        }).WithoutBurst().Run();
    }
}
