using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI fpsCounterText;
    [SerializeField]
    int frameUpdateInterval = 10; // how many frames to wait until you re-calculate the FPS

    [SerializeField]
    double[] times;
    int counter = 10;

    public void Start()
    {
        times = new double[frameUpdateInterval];
    }

    public void Update()
    {
        if (counter <= 0)
        {
            CalcFPS();
            counter = frameUpdateInterval;
        }

        times[counter] = Time.unscaledDeltaTime;
        counter--;
    }

    public void CalcFPS()
    {
        double sum = 0;
        foreach (double F in times)
        {
            sum += F;
        }

        double average = sum / times.Length;
        double fps = 1 / average;

        fpsCounterText.text = Math.Round(fps, 1).ToString() + " FPS";
    }
}
