using UnityEngine;
using System.Collections;
using Unity.Entities;

[UpdateAfter(typeof(WorkCreationGroup))]
public class WorkAssignmentGroup : ComponentSystemGroup
{
}
