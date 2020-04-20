using UnityEngine;
using System.Collections;
using Unity.Entities;

[UpdateAfter(typeof(WorkAssignmentGroup))]
public class ProductionGroup : ComponentSystemGroup
{
}
