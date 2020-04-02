using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct ResourceData : IComponentData, IEquatable<ResourceData>
{
    public int Amount;
    public ResourceType ResourceType;

    public static bool operator ==(ResourceData first, ResourceData second)
    {
        if (first.ResourceType == second.ResourceType && first.Amount == second.Amount)
            return true;

        return false;
    }

    public static bool operator !=(ResourceData first, ResourceData second)
    {
        if (first.ResourceType == second.ResourceType && first.Amount == second.Amount)
            return false;

        return true;
    }

    public override bool Equals(object obj)
    {
        return obj is ResourceData data && Equals(data);
    }

    public bool Equals(ResourceData other)
    {
        return Amount == other.Amount &&
               ResourceType == other.ResourceType;
    }

    public override int GetHashCode()
    {
        int hashCode = 772315182;
        hashCode = hashCode * -1521134295 + Amount.GetHashCode();
        hashCode = hashCode * -1521134295 + ResourceType.GetHashCode();
        return hashCode;
    }
}
