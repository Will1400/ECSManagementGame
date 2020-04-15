using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using System;

[Serializable]
public struct CitizenPersonalInfo
{
    public NativeString128 Name;
    public float Age;
    public Gender Gender;

    public DateTime BirthDate;
}
