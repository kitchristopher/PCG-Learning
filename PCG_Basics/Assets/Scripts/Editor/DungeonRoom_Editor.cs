using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonRoom))]
public class DungeonRoom_Editor : Editor
{
    private DungeonRoom _dungeonRoom;
    private SerializedProperty _serializedConnectedRooms_Property;
    private SerializedObject _serializedObject;

    private void OnEnable()
    {
        _dungeonRoom = target as DungeonRoom;
        _serializedObject = new SerializedObject(target);
    }

    public override void OnInspectorGUI()
    {
        //room type, name, connected rooms
        EditorGUILayout.LabelField("Room Information", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        //Room Name Data
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("Room Name: ", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField(_dungeonRoom.Room.roomName);  
        EditorGUILayout.EndHorizontal();

        //Room Type Data
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("Room Type: ", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField(_dungeonRoom.Room.Room_Type.ToString());
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }


    [DrawGizmo(GizmoType.Selected)]
    private static void DrawBoundryLines(DungeonRoom dungeonRoom, GizmoType gizmoType)
    {
        Color oldColor = Gizmos.color;
        Gizmos.color = Color.red;

        for (int i = 0; i < dungeonRoom.BoundryTiles.Count; i++)
        {
            Gizmos.DrawCube(dungeonRoom.BoundryTiles[i].transform.position, new Vector3(1.1f, 1.1f, 1.1f));
        }

        Gizmos.color = oldColor;
    }
}
