﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class CitySpawner : MonoBehaviour
{
    public int count = 1000;
    public void SpawnCity()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var entityPrefab = entityManager.CreateEntity(ArcheTypeManager.Instance.GetArcheType(PredifinedArchetype.ConstructionSite));

        entityManager.AddComponentData(entityPrefab, new UnderConstruction { totalConstructionTime = 4, remainingConstructionTime = 4, finishedPrefabName = "TallHouse" });

        NativeArray<Entity> constructionSites = entityManager.Instantiate(entityPrefab, count, Allocator.Temp);
        int columnCount = (int)math.round(math.sqrt(count));

        int row = -1;
        for (int i = 0; i < constructionSites.Length; i++)
        {
            float currentColumn = 15 * (i % columnCount);
            float currentRow = 20 * (row + 1);

            if (i % columnCount == 0)
            {
                row++;
            }

            float3 position = new float3(100, 0, 0) + new float3(currentColumn, 0, currentRow);

            var entity = constructionSites[i];

            var occupation = GridHelper.CalculateGridOccupationFromBounds(new AABB { Center = position, Extents = new float3(4.6f, 5.5f, 4f) });

            entityManager.AddComponentData(entity, new GridOccupation { Start = new int2(occupation.x, occupation.y), End = new int2(occupation.z, occupation.w) });
            entityManager.AddComponentData(entity, new Translation { Value = position });
            entityManager.AddComponentData(entity, new WorkPlaceWorkerData { MaxWorkers = 4, WorkPosition = position + new float3(0, 0, -(position.z - occupation.y + 1)) });
        }

        constructionSites.Dispose();
    }
}
