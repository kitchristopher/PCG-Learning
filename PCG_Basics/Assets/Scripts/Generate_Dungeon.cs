using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PCG;

/// <summary>
/// A Unity Class to generate the dungeon with Prefabs.
/// </summary>
public class Generate_Dungeon : MonoBehaviour
{
    [SerializeField] private GameObject _Prefab_Floor, _Prefab_Wall, _Prefab_Corridor, _Prefab_Door;//The prefabs used to create the dungeon cells
    public enum Generation_Types { BSP, Agent };//Which algorithim to use

    [Tooltip("The selected PCG Algorithim.")] public Generation_Types generationType;

    [Tooltip("How many cells the dungeon has in the X axis.")] public int width;
    [Tooltip("How many cells the dungeon has in the Z axis.")] public int height;
    [Tooltip("The smallest size a room can be partitioned to either vertically or horizontally.")] public int minArea;
    [Tooltip("The smmallest size a corridor can be, vertically and horizontally.")] public int minCorridorArea;

    //Variables for AgentBased 
    [Tooltip("The smallest size a room can be partitioned to either vertically or horizontally.")] public int maxArea;
    [Tooltip("The largest size a corridor can be, vertically and horizontally.")] public int maxCorridorLength;
    [Tooltip("The smmallest size a corridor can be, vertically and horizontally.")] public int minCorridorLength;
    [Tooltip("The chance the direction will change when building a corridor.")] public int corridorChangeChance;
    [Tooltip("How many total times the algorithm will keep trying " +
        "to build after a unsuccesful attempt.")]
    public int baseBacktrackCount;


    private GameObject _Cell_Holder;//A transform to store all of the rooms


    public void Generate()
    {
        if (_Cell_Holder != null)
            DestroyImmediate(_Cell_Holder);

        Generate_Tiles(Get_GeneratedRooms());
    }

    /// <summary>
    /// Calls the selected PCG algorithim to create the dungeon.
    /// </summary>
    /// <returns>Returns a list of all of the generated rooms. </returns>
    private List<Room> Get_GeneratedRooms()
    {
        List<Room> result;

        switch (generationType)
        {
            case Generation_Types.Agent:
                result = PCG_AgentBased.Generate_Dungeon(width, height, minArea, maxArea,
                    minCorridorArea, minCorridorLength, maxCorridorLength, corridorChangeChance, baseBacktrackCount);
                break;
            case Generation_Types.BSP:
            default:
                result = PCG_BSP.Generate_Dungeon(width, height, minArea, minCorridorArea);
                break;
        }

        return result;
    }

    /// <summary>
    /// Iterate through all of the generated rooms and instantiate the appropriate prefab for each cell based on it's type.
    /// </summary>
    /// <param name="rooms">All of the rooms created by the algorithim.</param>
    private void Generate_Tiles(List<Room> rooms)
    {
        if (_Cell_Holder == null)
            _Cell_Holder = Create_Cell_Holder("Room Holder", transform);

        foreach (var room in rooms)
        {
            GameObject roomHolder = Create_Cell_Holder(room.roomName, _Cell_Holder.transform);//give each room a transform holder
            roomHolder.AddComponent<DungeonRoom>().Init(room);

            foreach (var cell in room.RoomCells)//instansiate the appropriate prefabs for the cells.
            {
                Vector3 offset = new Vector3(cell.X, 0, cell.Y);//Add the cell's offset to the actual position of this gameObject.
                GameObject tile = null;

                if (cell.Cell_Type == Cell.CellType.Floor)
                    tile = Instantiate(_Prefab_Floor, transform.position + offset, Quaternion.identity, roomHolder.transform) as GameObject;
                else if (cell.Cell_Type == Cell.CellType.Corridor)
                    tile = Instantiate(_Prefab_Corridor, transform.position + offset, Quaternion.identity, roomHolder.transform) as GameObject;
                else if (cell.Cell_Type == Cell.CellType.Wall)
                    tile = Instantiate(_Prefab_Wall, transform.position + offset, Quaternion.identity, roomHolder.transform) as GameObject;
                else if (cell.Cell_Type == Cell.CellType.Door)
                    tile = Instantiate(_Prefab_Door, transform.position + offset, Quaternion.identity, roomHolder.transform) as GameObject;

                if (tile != null)
                    tile.GetComponent<Tile>().Init(cell);
            }
        }
    }


    /// <summary>
    /// Creates a new GameObject to be used a Holder for the cells in Unity.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    private GameObject Create_Cell_Holder(string name, Transform parent)
    {
        GameObject holder = new GameObject(name);
        holder.transform.parent = parent;
        return holder;
    }
}
