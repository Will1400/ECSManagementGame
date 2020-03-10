using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

public class Node : IHeapItem<Node>, IEquatable<Node>
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
    public Node parent;

    public int HeapIndex { get; set; }

    public Node(int2 position, Node parent = null)
    {
        Position = position;
        this.parent = parent;
    }

    public int CompareTo(Node other)
    {
        int compare = F.CompareTo(other.F);
        if (compare == 0)
            compare = H.CompareTo(other.H);

        return -compare;
    }

    public override bool Equals(object obj)
    {
        if (obj is Node node)
            return Position.Equals(node.Position);

        return false;
    }

    public bool Equals(Node other)
    {
        return other != null &&
               G == other.G &&
               H == other.H &&
               F == other.F &&
               Position.Equals(other.Position) &&
               EqualityComparer<Node>.Default.Equals(parent, other.parent) &&
               HeapIndex == other.HeapIndex;
    }

    public override int GetHashCode()
    {
        var hashCode = -1847991470;
        hashCode = hashCode * -1521134295 + G.GetHashCode();
        hashCode = hashCode * -1521134295 + H.GetHashCode();
        hashCode = hashCode * -1521134295 + F.GetHashCode();
        hashCode = hashCode * -1521134295 + Position.GetHashCode();
        hashCode = hashCode * -1521134295 + HeapIndex.GetHashCode();
        return hashCode;
    }
}