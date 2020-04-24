using Unity.Entities;

[GenerateAuthoringComponent]
public struct CitizenElement : IBufferElementData
{
    public Entity Value;

    public static implicit operator Entity(CitizenElement e)
    {
        return e.Value;
    }

    public static implicit operator CitizenElement(Entity e)
    {
        return new CitizenElement { Value = e };
    }
}
