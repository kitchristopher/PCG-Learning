using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Generate_Dungeon))]
public class Generate_Dungeon_Editor : Editor
{
    private Generate_Dungeon _generator;
    private SerializedObject _generatorObject;

    //Prefab Properties
    private SerializedProperty _floorPrefab_Property;
    private SerializedProperty _wallPrefab_Property;
    private SerializedProperty _doorPrefab_Property;
    private SerializedProperty _corridorPrefab_Property;
    private SerializedProperty _rockPrefab_Property;

    //Global Properties
    private SerializedProperty _generationType_Property;

    //Cellular Automata Properties
    private SerializedProperty _neighborhoodType_Property;

    private void OnEnable()
    {
        _generator = target as Generate_Dungeon;
        _generatorObject = new SerializedObject(target);
        _generationType_Property = _generatorObject.FindProperty("generationType");

        _corridorPrefab_Property = _generatorObject.FindProperty("_Prefab_Corridor");
        _doorPrefab_Property = _generatorObject.FindProperty("_Prefab_Door");
        _floorPrefab_Property = _generatorObject.FindProperty("_Prefab_Floor");
        _wallPrefab_Property = _generatorObject.FindProperty("_Prefab_Wall");
        _rockPrefab_Property = _generatorObject.FindProperty("_Prefab_Rock");
        _neighborhoodType_Property = _generatorObject.FindProperty("neighborhoodType");
    }

    public override void OnInspectorGUI()
    {
        //Global Parameters
        EditorGUILayout.PropertyField(_generationType_Property);

        EditorGUILayout.PropertyField(_corridorPrefab_Property);
        EditorGUILayout.PropertyField(_floorPrefab_Property);
        EditorGUILayout.PropertyField(_doorPrefab_Property);
        EditorGUILayout.PropertyField(_wallPrefab_Property);

        if (_generator.generationType == Generate_Dungeon.Generation_Types.CellularAutomata)
            EditorGUILayout.PropertyField(_rockPrefab_Property);

        _generator.width = EditorGUILayout.IntField("Dungeon Width", Mathf.Max(0, _generator.width));
        _generator.height = EditorGUILayout.IntField("Dungeon Height", Mathf.Max(0, _generator.height));
        _generator.minArea = EditorGUILayout.IntField("Min Room Area", Mathf.Min(Mathf.Max(_generator.width, _generator.height), _generator.minArea));
        _generator.minCorridorArea = EditorGUILayout.IntField("Min Corridor Area", Mathf.Max(0, _generator.minCorridorArea));

        //Agent-Based Parameters
        if (_generator.generationType == Generate_Dungeon.Generation_Types.Agent)
        {
            _generator.baseBacktrackCount = EditorGUILayout.IntField("Base Backtracking Count", Mathf.Max(0, _generator.baseBacktrackCount));
            _generator.maxArea = EditorGUILayout.IntField("Max Room Area", Mathf.Max(0, _generator.maxArea));
            _generator.corridorChangeChance = EditorGUILayout.IntField("Corridor Randomness", Mathf.Max(0, _generator.corridorChangeChance));
            _generator.minCorridorLength = EditorGUILayout.IntField("Min Corridor Length", Mathf.Max(0, _generator.minCorridorLength));
            _generator.maxCorridorLength = EditorGUILayout.IntField("Max Corridor Length", Mathf.Max(0, _generator.maxCorridorLength));
        }
        else if (_generator.generationType == Generate_Dungeon.Generation_Types.CellularAutomata)
        {
            //Don't let user specify threhsold higher than the maximum cells in a neighborhood
            int maxRockThreshold = _generator.neighborhoodType == PCG.PCG_CellularAutomata.NeighborhoodType.vonNeumann ? 4 : 8;

            EditorGUILayout.PropertyField(_neighborhoodType_Property);
            _generator.rockPercentage = EditorGUILayout.IntField("Rock Percentage", Mathf.Clamp(_generator.rockPercentage, 0, 100));
            _generator.rockThreshold = EditorGUILayout.IntField("Rock Threshold", Mathf.Clamp(_generator.rockThreshold, 0, maxRockThreshold));
            _generator.automataGenerations = EditorGUILayout.IntField("Automata Generations", Mathf.Max(0, _generator.automataGenerations));
            _generator.shouldConnectAreas = EditorGUILayout.Toggle("Connect Areas", _generator.shouldConnectAreas);
        }

        _generatorObject.ApplyModifiedProperties();
    }
}
