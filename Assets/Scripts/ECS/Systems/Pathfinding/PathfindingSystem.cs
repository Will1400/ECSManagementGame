using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class PathfindingSystem : JobComponentSystem
{
    public NativeArray<int2> neighbours;

    protected override void OnCreate()
    {
        neighbours = new NativeArray<int2>(8, Allocator.Persistent);
        neighbours[1] = new int2(0, -1); // Bottom
        neighbours[3] = new int2(-1, 0); // Left
        neighbours[4] = new int2(1, 0); // Right
        neighbours[6] = new int2(0, 1); // Top
        //neighbours[0] = new int2(-1, -1);// Bottom left
        //neighbours[2] = new int2(1, -1); // Bottom Right
        //neighbours[7] = new int2(1, 1); // Top Right
        //neighbours[5] = new int2(-1, 1); // Top Left
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        PathRequest request = new PathRequest { StartPosition = new int3(20, 0, 100), EndPosition = new int3(10, 0, 340) };
        NativeList<int2> path = new NativeList<int2>(Allocator.TempJob);
        var job = new FindPathJob
        {
            PathRequest = request,
            gridSize = GridCacheSystem.Instance.GridSize,
            Grid = GridCacheSystem.Instance.Grid,
            NeighbourOffsets = neighbours,
            Path = path
        };
        JobHandle handle = job.Schedule();
        handle.Complete();
        path.Dispose();
        return handle;
    }

    protected override void OnDestroy()
    {
        neighbours.Dispose();
    }

    [BurstCompile]
    private struct FindPathJob : IJob
    {
        public PathRequest PathRequest;
        public int2 gridSize;

        [ReadOnly] public NativeArray<int2> NeighbourOffsets;
        [ReadOnly] public NativeArray<int> Grid;

        public NativeList<int2> Path;

        public void Execute()
        {
            int2 start = PathRequest.EndPosition.xz;
            int2 end = PathRequest.StartPosition.xz;
            FindPath(start, end);
        }

        void FindPath(int2 start, int2 end)
        {
            AStar aStar = new AStar();
            Path = aStar.GetPath(Grid, gridSize, NeighbourOffsets, start, end);
        }
    }
}
