using Unity.Entities;

[GenerateAuthoringComponent]
public struct ResourceCostElement : IBufferElementData
{
    public ResourceCost Value;

    public static implicit operator ResourceCost(ResourceCostElement e)
    {
        return e.Value;
    }

    public static implicit operator ResourceCostElement(ResourceCost e)
    {
        return new ResourceCostElement { Value = e };
    }
}
