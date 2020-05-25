using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class CitySpawner : MonoBehaviour
{
    public string PrefabName;
    public int count = 1000;

    public void SpawnCity()
    {
        EntityManager EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var entityPrefab = EntityPrefabManager.Instance.SpawnConstructionEntityPrefab(PrefabName);

        if (entityPrefab == Entity.Null)
            return;

        EntityManager.AddComponentData(entityPrefab, new ConstructionData { TotalConstructionTime = 4, RemainingConstructionTime = 4, FinishedPrefabName = PrefabName });

        NativeArray<Entity> constructionSites = EntityManager.Instantiate(entityPrefab, count, Allocator.Temp);

        EntityManager.DestroyEntity(entityPrefab);

        int columnCount = (int)math.round(math.sqrt(count));

        for (int i = 0; i < constructionSites.Length; i++)
        {
            float currentColumn = 15 * (i % columnCount);
            float currentRow = 20 * ((int)math.floor(i / columnCount) + 1);

            float3 position = new float3(100, 0, 10) + new float3(currentColumn, 0, currentRow);

            var entity = constructionSites[i];

            var occupation = GridHelper.CalculateGridOccupationFromBounds(new AABB { Center = position, Extents = new float3(4.6f, 5.5f, 4f) });

            EntityManager.AddComponentData(entity, new GridOccupation { Start = new int2(occupation.x, occupation.y), End = new int2(occupation.z, occupation.w) });
            EntityManager.AddComponentData(entity, new Translation { Value = position });
            EntityManager.AddComponentData(entity, new WorkplaceWorkerData { MaxWorkers = 4, WorkPosition = position + new float3(0, 0, -(position.z - occupation.y + 1)) });
        }

        constructionSites.Dispose();
    }
}
