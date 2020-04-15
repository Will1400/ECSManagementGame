using Unity.Entities;

[GenerateAuthoringComponent]
public struct ResourceCostElement : IBufferElementData
{
    public ResourceCostData Value;

    public static implicit operator ResourceCostData(ResourceCostElement e)
    {
        return e.Value;
    }

    public static implicit operator ResourceCostElement(ResourceCostData e)
    {
        return new ResourceCostElement { Value = e };
    }
}
