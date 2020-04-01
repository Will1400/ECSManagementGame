using Unity.Entities;

public struct DestroyResourceInStorage : IComponentData
{
    public int StorageId;
    public ResourceData ResourceData;
}
