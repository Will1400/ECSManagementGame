using UnityEngine;
using System.Collections;
using Unity.Burst;

[BurstCompile]
public static class TimeHelper
{
    public static readonly float RealTimeSecondRatio = 60;

    [BurstCompile]
    public static float ConvertTime(int hours, int minutes, float seconds)
    {
        minutes += hours * 60;
        seconds += minutes * 60;

        return seconds * RealTimeSecondRatio;
    }

    public static float ConvertTime(int minutes, float seconds)
    {
        return ConvertTime(0, minutes, seconds);
    }

    public static float ConvertTime(float seconds)
    {
        return ConvertTime(0, 0, seconds);
    }
}
