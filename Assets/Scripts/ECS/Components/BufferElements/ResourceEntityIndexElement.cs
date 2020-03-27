using System;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct ResourceEntityIndexElement : IBufferElementData, IComparable<ResourceEntityIndexElement>
{
    public int Value;

    public static implicit operator int(ResourceEntityIndexElement e)
    {
        return e.Value;
    }

    public static implicit operator ResourceEntityIndexElement(int e)
    {
        return new ResourceEntityIndexElement { Value = e };
    } 
    
    public override bool Equals(object obj)
    {
        return obj is ResourceEntityIndexElement element && Equals(element);
    }

    public bool Equals(ResourceEntityIndexElement other)
    {
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return -1937169414 + Value.GetHashCode();
    }

    public int CompareTo(ResourceEntityIndexElement other)
    {
        if (other.Value > Value)
            return -1;
        else if (other.Value < Value)
            return 1;
        return 0;
    }
}
