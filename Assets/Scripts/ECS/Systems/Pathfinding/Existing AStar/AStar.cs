using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

public class AStar
{
    private NativeArray<int> grid;
    private int2 gridSize;

    [BurstCompile]
    public NativeList<int2> GetPath(NativeArray<int> grid, int2 gridSize, NativeArray<int2> neighbourOffsets, int2 start, int2 end)
    {
        this.gridSize = gridSize;
        this.grid = grid;
        NativeList<int2> path = new NativeList<int2>();

        Node startNode = new Node(start);
        Node endNode = new Node(end);

        Heap<Node> openList = new Heap<Node>(gridSize.x * gridSize.y);
        HashSet<Node> closedList = new HashSet<Node>();

        openList.Add(startNode);
        Node currentNode = null;

        while (openList.Count > 0 && !closedList.Select(x => x.Position).Contains(end)) // Loop til end is found
        {
            currentNode = openList.RemoveFirst();

            closedList.Add(currentNode);


            for (int i = 0; i < neighbourOffsets.Length; i++)
            {
                var neighbour = new Node(currentNode.Position + neighbourOffsets[i]);

                if (!IsValidPosition(neighbour.Position))
                    continue;
                if (closedList.Contains(neighbour) || openList.Contains(neighbour))
                    continue;

                neighbour.parent = currentNode;
                neighbour.H = math.abs(neighbour.Position.x - endNode.Position.x) + math.abs(neighbour.Position.y - endNode.Position.y);
                neighbour.G = currentNode.G + 1;

                openList.Add(neighbour);
            }
        }

        if (currentNode.Equals(endNode)) // Found goal
        {
            Node current = currentNode;

            while (current != null)
            {
                path.Add(current.Position);
                current = current.parent;
            }
        }

        return path;
    }

    bool IsValidPosition(int2 pos)
    {
        if (pos.x > gridSize.x || pos.y > gridSize.y || pos.x < 0 || pos.y < 0) // outside of map
            return false;

        if (grid[GetIndex(pos.x, pos.y)] != 0)
        {
            return true;
        }

        return false;
    }

    int GetIndex(int x, int y)
    {
        return x * gridSize.x + y;
    }
}
