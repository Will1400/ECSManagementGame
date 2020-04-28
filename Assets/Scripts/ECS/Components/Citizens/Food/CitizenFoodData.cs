using Unity.Entities;

[GenerateAuthoringComponent]
public struct CitizenFoodData : IComponentData
{
    public float MaxFoodLevel;
    public float CurrentFoodLevel;
    public float DepletionMultiplier;
}
