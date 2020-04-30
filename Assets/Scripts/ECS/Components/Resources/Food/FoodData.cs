using Unity.Entities;

[GenerateAuthoringComponent]
public struct FoodData : IComponentData
{
    public FoodType FoodType;
    //public float Amount;
    public float HungerReplenished;
}
