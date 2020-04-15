using Unity.Entities;

[GenerateAuthoringComponent]
public struct ResourceRequestElement : IComponentData
{
    public ResourceRequestData Value;

    public static implicit operator ResourceRequestData(ResourceRequestElement e)
    {
        return e.Value;
    }

    public static implicit operator ResourceRequestElement(ResourceRequestData e)
    {
        return new ResourceRequestElement { Value = e };
    }
}
