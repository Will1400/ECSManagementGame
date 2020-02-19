using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct PlayerInputComponent : IComponentData
{
    public bool LeftClick;
    public bool RightClick;
}