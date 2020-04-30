using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct FoodProductionData : IComponentData
{
    public FoodType FoodType;
    public int AmountPerProduction;

    public float ProductionTime;
    public float ProductionTimeRemaining;

    public float3 SpawnPointOffset;
}
