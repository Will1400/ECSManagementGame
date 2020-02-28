using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TreePlacer))]
public class TreePlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Replant Trees"))
        {
            TreePlacer placer = target as TreePlacer;
            placer.Replant();
        }
    }
}