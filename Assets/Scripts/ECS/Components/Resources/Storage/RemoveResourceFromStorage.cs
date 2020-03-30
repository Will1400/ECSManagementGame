using UnityEngine;
using System.Collections;
using Unity.Entities;

public struct RemoveResourceFromStorage : IComponentData
{
    public Entity ResourceEntity;
}
