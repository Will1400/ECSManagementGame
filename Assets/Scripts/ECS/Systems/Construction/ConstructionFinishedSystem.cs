using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;

public class ConstructionFinishedSystem : ComponentSystem
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery allFinishedSitesQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        allFinishedSitesQuery = Entities.WithAll<UnderConstruction, WorkPlaceWorkerData, Translation>()
                        .WithAllReadOnly<ConstructionFinishedTag>()
                        .ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        var CommandBuffer = bufferSystem.CreateCommandBuffer();

        Entities.With(allFinishedSitesQuery).ForEach((Entity entity, DynamicBuffer<ResourceDataElement> resourceDatas, ref UnderConstruction construction, ref WorkPlaceWorkerData workerData, ref Translation translation) =>
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
                EntityManager.SetComponentData(finishedEntity, newWorkerData);
            }

            EntityManager.AddComponentData(finishedEntity, new Translation { Value = translation.Value });

            var occupation = EntityManager.GetComponentData<GridOccupation>(entity);
            EntityManager.AddComponentData(finishedEntity, new GridOccupation { Start = occupation.Start, End = occupation.End });

            workerData.ActiveWorkers = -1;
            EntityManager.RemoveComponent<UnderConstruction>(entity);
            EntityManager.AddComponent<RemoveWorkPlaceTag>(entity);
        });
    }
}
