using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct CitizenHousingData : IComponentData
{
    public float3 HousePosition;
    public Entity HouseEntity;
}
