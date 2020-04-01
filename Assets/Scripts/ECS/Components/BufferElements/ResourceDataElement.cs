using Unity.Entities;

[GenerateAuthoringComponent]
public struct ResourceDataElement : IBufferElementData
{
    public ResourceData Value;

    public static implicit operator ResourceData(ResourceDataElement e)
    {
        return e.Value;
    }

    public static implicit operator ResourceDataElement(ResourceData e)
    {
        return new ResourceDataElement { Value = e };
    }
}
