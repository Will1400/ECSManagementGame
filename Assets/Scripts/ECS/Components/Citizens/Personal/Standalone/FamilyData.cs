using Unity.Entities;

public struct FamilyData : IComponentData
{
    public bool HasHome;
    public Entity Husband;
    public Entity Wife;
}
