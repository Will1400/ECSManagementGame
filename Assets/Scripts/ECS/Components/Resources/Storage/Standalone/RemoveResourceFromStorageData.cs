using UnityEngine;
using System.Collections;
using Unity.Entities;

public struct RemoveResourceFromStorageData : IComponentData
{
    public Entity ResourceEntity;
    public Entity StorageEntity;
}
