using Unity.Entities;

public struct DestroyResourceInStorageData : IComponentData
{
    public Entity StorageEntity;
    public ResourceData ResourceData;
}
