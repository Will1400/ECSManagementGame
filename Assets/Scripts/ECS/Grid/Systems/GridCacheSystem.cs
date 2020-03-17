using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

[UpdateAfter(typeof(GridValidationSystem))]
public class GridCacheSystem : ComponentSystem
{
    public static GridCacheSystem Instance;

    /// <summary>
    /// 0 = Empty
    /// 5 = Unwalkable
    /// 6 = Occupied
    /// </summary>
    public NativeArray<int> Grid;
    public NativeList<GridOccupation> GridOccupations;

    public int2 GridSize;

    EntityQuery nonCachedGridOccupations;
    EntityQuery deletedGridOccupationsInCache;
    //EntityQuery ChangedGridOccupationsInCache;


    protected override void OnCreate()
    {
        nonCachedGridOccupations = Entities.WithAll<GridOccupation, GridOccupationIsValidTag>()
                                           .WithNone<IsInCache, BeingPlacedTag, GridOccupationIsInValidTag>()
                                           .ToEntityQuery();

        //ChangedGridOccupationsInCache = Entities.WithAll<GridOccupation, IsInCache>()
        //                                        .WithNone<IsStationary, BeingPlacedTag>()
        //                                        .ToEntityQuery();
        //ChangedGridOccupationsInCache.SetChangedVersionFilter(ComponentType.ReadOnly<GridOccupation>());

        deletedGridOccupationsInCache = Entities.WithAll<IsInCache>()
                                          .WithNone<GridOccupation>()
                                          .ToEntityQuery();

        Instance = this;
        GridOccupations = new NativeList<GridOccupation>(Allocator.Persistent);
        GridSize = new int2(5000, 5000);
        Grid = new NativeArray<int>(GridSize.x * GridSize.y, Allocator.Persistent);
    }

    protected override void OnUpdate()
    {
        // Add to cache
        Entities.With(nonCachedGridOccupations).ForEach((Entity entity, ref GridOccupation gridOccupation) =>
        {
            GridOccupations.Add(gridOccupation);
            Debug.Log("Adding occupation to cache");

            for (int x = gridOccupation.Start.x; x <= gridOccupation.End.x; x++)
            {
                for (int y = gridOccupation.Start.y; y <= gridOccupation.End.y; y++)
                {
                    Grid[x * GridSize.x + y] = 6;
                }
            }

            EntityManager.AddComponent<IsInCache>(entity);
            EntityManager.AddComponentData(entity, new IsInCache { OccupationHashCode = gridOccupation.GetHashCode() });
        });

        // Remove from cache
        Entities.With(deletedGridOccupationsInCache).ForEach((Entity entity, ref IsInCache isInCache) =>
        {
            Debug.Log("Deleting occupation from cache");
            for (int i = 0; i < GridOccupations.Length; i++)
            {
                var gridOccupation = GridOccupations[i];
                if (gridOccupation.GetHashCode() != isInCache.OccupationHashCode)
                    continue;

                for (int x = gridOccupation.Start.x; x <= gridOccupation.End.x; x++)
                {
                    for (int y = gridOccupation.Start.y; y <= gridOccupation.End.y; y++)
                    {
                        Grid[x * GridSize.x + y] = 0;
                    }
                }
                GridOccupations.RemoveAtSwapBack(i);
            }
            EntityManager.RemoveComponent<IsInCache>(entity);
        });
    }

    public int CheckIndex(int x, int y)
    {
        if (x * GridSize.x + y > 0 && x * GridSize.x + y < Grid.Length)
            return Grid[x * GridSize.x + y];
        else
            return -1;
    }

    void GenerateGrid()
    {
        Grid.Dispose();
        Grid = new NativeArray<int>(GridSize.x * GridSize.y, Allocator.Persistent);

        for (int i = 0; i < GridOccupations.Length; i++)
        {
            var occupation = GridOccupations[i];

            for (int x = occupation.Start.x; x <= occupation.End.x; x++)
            {
                for (int y = occupation.Start.y; y <= occupation.End.y; y++)
                {
                    Grid[x * GridSize.x + y] = 6;
                }
            }
        }
    }

    protected override void OnDestroy()
    {
        GridOccupations.Dispose();
        Grid.Dispose();
    }
}
