﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[Serializable]
[GenerateAuthoringComponent]
public struct Citizen : IComponentData
{
    public int Id;
    public CitizenPersonalInfo CitizenPersonalData;
}
