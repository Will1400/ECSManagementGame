using UnityEngine;
using System.Collections;
using Unity.Entities;

[UpdateAfter(typeof(ResourceInteractionGroup))]
public class ResourceStorageInteractionGroup : ComponentSystemGroup
{

}
