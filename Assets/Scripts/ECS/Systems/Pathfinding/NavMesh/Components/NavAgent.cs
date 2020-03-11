using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

public enum AgentStatus
{
    Idle = 0,
    PathQueued = 1,
    Moving = 2,
    Paused = 4
}

[GenerateAuthoringComponent]
public struct NavAgent : IComponentData
{
    public AgentStatus Status;
    public int CurrentWaypointIndex;
    public int TotalWaypoints;
}
