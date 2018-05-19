using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PCG;

public class DungeonRoom : MonoBehaviour
{
    private Room _room; public Room Room { get { return _room; } }

    private List<Tile> _boundryTiles = new List<Tile>(); public List<Tile> BoundryTiles { get { return _boundryTiles; } }

    public void Init(Room room)
    {
        _room = room;
    }

    private void Start()
    {
        //Store the unity gameobjects based on their boundry
        foreach (var tile in this.GetComponentsInChildren<Tile>())
        {
            if (_room.BoundryCells.Contains(tile.Cell))
                _boundryTiles.Add(tile);
        }
    }
}
