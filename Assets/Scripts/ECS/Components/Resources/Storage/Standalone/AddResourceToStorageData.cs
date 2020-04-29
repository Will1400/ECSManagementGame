using Unity.Entities;

public struct AddResourceToStorageData : IComponentData
{
    public Entity ResourceEntity;
    public Entity StorageEntity;
}
