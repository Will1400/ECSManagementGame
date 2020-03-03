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
    EntityQuery ChangedCachedGridOccupations;

    int2 GridSize;

    protected override void OnCreate()
    {
        nonCachedGridOccupations = Entities.WithAll<GridOccupation, GridOccupationIsValidTag>()
                                           .WithNone<IsInCacheTag>()
                                           .ToEntityQuery();

        ChangedCachedGridOccupations = Entities.WithAll<GridOccupation, IsInCacheTag>()
                                               .ToEntityQuery();
        ChangedCachedGridOccupations.SetChangedVersionFilter(ComponentType.ReadOnly<GridOccupation>());
    }

    protected override void OnStartRunning()
    {
        Instance = this;
        GridOccupations = new NativeList<GridOccupation>(Allocator.Persistent);
        Grid = new NativeList<int>(Allocator.Persistent);
    }

    protected override void OnUpdate()
    {
        Entities.With(nonCachedGridOccupations).ForEach((Entity entity, ref GridOccupation gridOccupation) =>
        {
            GridOccupations.Add(gridOccupation);

            if (Grid.Length < gridOccupation.End.x * gridOccupation.End.y)
                Grid.AddRange(new NativeArray<int>(gridOccupation.End.x * gridOccupation.End.y - Grid.Length, Allocator.Temp));

            for (int x = gridOccupation.Start.x; x < gridOccupation.End.x; x++)
            {
                for (int y = gridOccupation.Start.y; y < gridOccupation.End.y; y++)
                {
                    Grid[x * GridSize.x + y] = 6;
                }
            }

            EntityManager.AddComponent<IsInCacheTag>(entity);
        });
    }
}
