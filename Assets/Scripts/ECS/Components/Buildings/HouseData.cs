using UnityEngine;
using System.Collections;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct HouseData : IComponentData
{
    public int MaxResidents;
    public int CurrentResidents;
}
