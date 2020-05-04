using Unity.Entities;

public struct ConsumeFoodData : IComponentData
{
    public Entity ConsumerEntity;
    public Entity FoodEntity;

    public FoodData FoodData;
}
