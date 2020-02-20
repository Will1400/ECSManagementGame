﻿using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct PlayerInputComponent : IComponentData
{
    public bool LeftClick;
    public bool RightClick;
}