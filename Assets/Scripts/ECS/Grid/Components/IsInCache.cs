﻿using UnityEngine;
using System.Collections;
using Unity.Entities;

public struct IsInCache : ISystemStateComponentData
{
    public int OccupationHashCode;
}
