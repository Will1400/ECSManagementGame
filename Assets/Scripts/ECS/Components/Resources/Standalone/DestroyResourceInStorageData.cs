using Unity.Entities;

public struct DestroyResourceInStorageData : IComponentData
{
    public int StorageId;
    public ResourceData ResourceData;
}
