using Unity.Entities;

public struct CitizenPregnancyData : IComponentData
{
    public float TimeRemaining;

    public Entity Mother;
    public Entity Father;
}
