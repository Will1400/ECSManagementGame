using Unity.Entities;

public struct FamilyData : IComponentData
{
    public bool HasHome;
    public int ChildCount;
    public Entity Husband;
    public Entity Wife;
}
