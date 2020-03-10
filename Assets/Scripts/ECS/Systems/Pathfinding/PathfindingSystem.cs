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
        neighbours[0] = new int2(-1, -1);// Bottom left
        neighbours[1] = new int2(0, -1); // Bottom
        neighbours[2] = new int2(1, -1); // Bottom Right
        neighbours[3] = new int2(-1, 0); // Left
        neighbours[4] = new int2(1, 0); // Right
        neighbours[5] = new int2(-1, 1); // Top Left
        neighbours[6] = new int2(0, 1); // Top
        neighbours[7] = new int2(1, 1); // Top Right
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
            var openList = new NativeList<Node>(Allocator.Temp);
            var closedList = new NativeList<Node>(Allocator.Temp);
            if (start.Equals(end))
                return;

            var startNode = new Node(start);
            openList.Add(startNode);

            Node currentNode = new Node();

            while (openList.Length > 0)
            {
                currentNode = openList[0];
                openList.RemoveAtSwapBack(0);

                closedList.Add(currentNode);

                for (int i = 0; i < NeighbourOffsets.Length; i++)
                {
                    var neighbour = new Node(currentNode.Position + NeighbourOffsets[i]);
                    if (Grid[GetIndex(neighbour.Position.x, neighbour.Position.y)] != 0)
                        return;

                    if (closedList.Contains(neighbour) || openList.Contains(neighbour))
                        continue;

                    neighbour.ParentIndex = openList.IndexOf(currentNode);
                    neighbour.H = math.abs(neighbour.Position.x - end.x) + math.abs(neighbour.Position.y - end.y);
                    neighbour.G = currentNode.G + 1;

                    openList.Add(neighbour);
                }
            }

            if (currentNode.Equals(new Node(end)))
            {
                Node current = currentNode;
                while (current.ParentIndex != -1)
                {
                    Path.Add(current.Position);
                    current = openList[current.ParentIndex];
                }
            }

            openList.Dispose();
            closedList.Dispose();

        }

        int GetIndex(int x, int y)
        {
            return x * gridSize.x + y;
        }

        struct Node : IEquatable<Node>
        {
            public int G;
            public int H;
            public int F
            {
                get
                {
                    return H + G;
                }
            }

            public int2 Position;
            public int ParentIndex;

            public Node(int2 position)
            {
                Position = position;
                G = 0;
                H = 0;

                ParentIndex = -1;
            }

            public int CompareTo(Node other)
            {
                int compare = F.CompareTo(other.F);
                if (compare == 0)
                    compare = H.CompareTo(other.H);

                return -compare;
            }

            public bool Equals(Node other)
            {
                return G == other.G && H == other.H && Position.Equals(other.Position);
            }
        }
    }
}
