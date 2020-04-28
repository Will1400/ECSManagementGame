using Unity.Collections;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct DisplayData : IComponentData
{
    public NativeString128 Name;
}
