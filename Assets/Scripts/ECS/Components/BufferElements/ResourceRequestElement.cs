using Unity.Entities;

[GenerateAuthoringComponent]
public struct ResourceRequestElement : IComponentData
{
    public ResourceRequest Value;

    public static implicit operator ResourceRequest(ResourceRequestElement e)
    {
        return e.Value;
    }

    public static implicit operator ResourceRequestElement(ResourceRequest e)
    {
        return new ResourceRequestElement { Value = e };
    }
}
