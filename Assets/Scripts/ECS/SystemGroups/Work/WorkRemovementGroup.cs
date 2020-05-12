using UnityEngine;
using System.Collections;
using Unity.Entities;

[UpdateAfter(typeof(CitizenArrivedAtWork))]
public class WorkRemovementGroup : ComponentSystemGroup
{

}
