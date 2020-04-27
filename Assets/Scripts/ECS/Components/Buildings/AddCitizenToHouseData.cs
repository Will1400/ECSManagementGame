using Unity.Entities;

public struct AddCitizenToHouseData : IComponentData
{
    public Entity CitizenEntity;
    public Entity HouseEntity;
}
