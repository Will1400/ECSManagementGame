using Unity.Entities;

[GenerateAuthoringComponent]
public struct CitizenElement : IBufferElementData
{
    public Citizen Value;

    public static implicit operator Citizen(CitizenElement e)
    {
        return e.Value;
    }

    public static implicit operator CitizenElement(Citizen e)
    {
        return new CitizenElement { Value = e };
    }
}
