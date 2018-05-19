using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Tile))]
public class Cell_Editor : Editor
{
    private Tile _tile;

    private void OnEnable()
    {
        _tile = target as Tile;    
    }

    public override void OnInspectorGUI()
    {
        DrawDataGUI();
    }

    private void DrawDataGUI()
    {
        EditorGUILayout.LabelField("Cell Information", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        //Cell Type Data
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("Cell Type: ", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField(_tile.Cell.Cell_Type.ToString());
        EditorGUILayout.EndHorizontal();

        //Cell Location Data
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("Local Coordinates: ", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField("[" + _tile.Cell.X.ToString() + ", " + _tile.Cell.Y.ToString() + "]");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
}
