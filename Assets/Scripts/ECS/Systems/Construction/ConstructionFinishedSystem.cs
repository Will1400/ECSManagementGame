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

        Entities.WithAll<ConstructionFinishedTag>().ForEach((Entity entity, DynamicBuffer<ResourceDataElement> resourceDatas, ref UnderConstruction construction, ref WorkPlaceWorkerData workerData, ref Translation translation) =>
        {
            for (int i = 0; i < resourceDatas.Length; i++)
            {
                var resource = resourceDatas[i].Value;
                var destroyEntity = CommandBuffer.CreateEntity();
                CommandBuffer.AddComponent<DestroyResourceInStorage>(destroyEntity);
                CommandBuffer.SetComponent(destroyEntity, new DestroyResourceInStorage
                {
                    ResourceData = resource,
                    StorageId = entity.Index
                });
            }

            Entity finishedEntity = EntityPrefabManager.Instance.SpawnEntityPrefab(construction.finishedPrefabName.ToString());

            if (finishedEntity == Entity.Null)
            {
                CommandBuffer.DestroyEntity(entity);
                return;
            }

            translation.Value.y = EntityManager.GetComponentData<Translation>(finishedEntity).Value.y;

            if (EntityManager.HasComponent<WorkPlaceWorkerData>(finishedEntity))
            {
                WorkPlaceWorkerData newWorkerData = EntityManager.GetComponentData<WorkPlaceWorkerData>(finishedEntity);
                newWorkerData.WorkPosition = workerData.WorkPosition;
                CommandBuffer.SetComponent(finishedEntity, newWorkerData);
            }

            CommandBuffer.SetComponent(finishedEntity, new Translation { Value = translation.Value });

            var occupation = EntityManager.GetComponentData<GridOccupation>(entity);
            CommandBuffer.SetComponent(finishedEntity, new GridOccupation { Start = occupation.Start, End = occupation.End });

            workerData.ActiveWorkers = -1;
            CommandBuffer.RemoveComponent<UnderConstruction>(entity);
            CommandBuffer.AddComponent<RemoveWorkPlaceTag>(entity);
        }).WithoutBurst().WithStructuralChanges().Run();
    }
}
