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
    public NativeList<int> Grid;
    public NativeList<GridOccupation> GridOccupations;

    EntityQuery nonCachedGridOccupations;
    EntityQuery deletedGridOccupationsInCache;
    EntityQuery ChangedGridOccupationsInCache;

    protected override void OnCreate()
    {
        nonCachedGridOccupations = Entities.WithAll<GridOccupation, GridOccupationIsValidTag>()
                                           .WithNone<IsInCache, BeingPlacedTag, GridOccupationIsInValidTag>()
                                           .ToEntityQuery();

        ChangedGridOccupationsInCache = Entities.WithAll<GridOccupation, IsInCache>()
                                                .WithNone<IsStationary, BeingPlacedTag>()
                                                .ToEntityQuery();
        ChangedGridOccupationsInCache.SetChangedVersionFilter(ComponentType.ReadOnly<GridOccupation>());

        deletedGridOccupationsInCache = Entities.WithAll<IsInCache>()
                                          .WithNone<GridOccupation>()
                                          .ToEntityQuery();

        Instance = this;
        GridOccupations = new NativeList<GridOccupation>(Allocator.Persistent);
        Grid = new NativeList<int>(Allocator.Persistent);
    }

    protected override void OnUpdate()
    {
        // Add to cache
        Entities.With(nonCachedGridOccupations).ForEach((Entity entity, ref GridOccupation gridOccupation) =>
        {
            GridOccupations.Add(gridOccupation);

            EntityManager.AddComponent<IsInCache>(entity);
            EntityManager.AddComponentData(entity, new IsInCache { Index = GridOccupations.IndexOf(gridOccupation) });
        });

        // Update Changed occupations
        //Entities.With(ChangedGridOccupationsInCache).ForEach((Entity entity, ref IsInCache isInCache, ref GridOccupation gridOccupation) =>
        //{
        //});

        // Remove from cache
        Entities.With(deletedGridOccupationsInCache).ForEach((Entity entity, ref IsInCache isInCache) =>
        {
            GridOccupations.RemoveAtSwapBack(isInCache.Index);

            EntityManager.RemoveComponent<GridOccupation>(entity);
            EntityManager.RemoveComponent<IsInCache>(entity);
            EntityManager.RemoveComponent<IsStationary>(entity);
        });
    }

    protected override void OnDestroy()
    {
        GridOccupations.Dispose();
        Grid.Dispose();
    }
}
