using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CitySpawner))]
public class CitySpawerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Spawn City"))
        {
            CitySpawner placer = target as CitySpawner;
            placer.SpawnCity();
        }
    }
}