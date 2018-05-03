using System.Collections.Generic;

namespace PCG
{
    /// <summary>
    /// A class that contains what type of structure the cell is, as well as it's relative position of creation in the grid.
    /// </summary>
    public class Cell
    {
        public enum CellType { Empty, Wall, Floor, Corridor };//a classification method to represent what this cell is in the dungeon
        public Room room;//The room this cell lies in

        private List<Cell> _adjacents = new List<Cell>(); public List<Cell> AdjacentCells { get { return _adjacents; } }//the cells adjacent to this cell 
        private CellType _cellType = CellType.Empty;//This cell's type
        private int _x, _y;//The x and y positions of the cell based on the Dungeon as a whole, not the room or BSP

        public int X { get { return _x; } }
        public int Y { get { return _y; } }
        public CellType Cell_Type { get { return _cellType; } set { _cellType = value; } }

        /// <summary>
        /// Create a new cell with a relative position.
        /// </summary>
        /// <param name="x">The x position of the cell based on the Dungeon as a whole, not the room or BSP</param>
        /// <param name="y">The y position of the cell based on the Dungeon as a whole, not the room or BSP</param>
        public Cell(int x, int y)
        {
            _x = x;
            _y = y;
        }

        /// <summary>
        /// Create a new cell with a relative position AND assign its adjacent neighbors based on its grid.
        /// </summary>
        /// <param name="x">The x position of the cell based on the Dungeon as a whole, not the room or BSP</param>
        /// <param name="y">The y position of the cell based on the Dungeon as a whole, not the room or BSP</param>
        /// <param name="grid">The grid in which this cell and its neighbors are stored in.</param>
        public Cell(int x, int y, Cell[,] grid)
        {
            _x = x;
            _y = y;
            Add_Adjacents(grid);
        }

        /// <summary>
        /// Add the adjacent cells to the AdjacentCells List that are below and to the left of this cell, and vice versa.
        /// </summary>
        /// <param name="grid">The grid in which this cell and its neighbors are stored in.</param>
        private void Add_Adjacents(Cell[,] grid)
        {
            //Add left and down cell to adjacents
            if (_x != 0 && grid[_x - 1, _y] != null)
            {
                AdjacentCells.Add(grid[_x - 1, _y]);

                //and Vice Versa
                grid[_x - 1, _y].AdjacentCells.Add(this);
            }
            if (_y != 0 && grid[_x, _y - 1] != null)
            {
                AdjacentCells.Add(grid[_x, _y - 1]);

                //and Vice Versa
                grid[_x, _y - 1].AdjacentCells.Add(this);
            }
        }
    }

    /// <summary>
    /// Contains a group of cells created together.
    /// </summary>
    public class Room
    {
        public string roomName;// A name for the room just for user identification
        private BSP_Tree _partition;//The BSP the room is located in
        private List<Cell> _room; public List<Cell> RoomCells { get { return _room; } }//The list of cells in the room

        /// <param name="room">The list of cells in the room.</param>
        /// <param name="partition">The BSP the room is located in.</param>
        /// <param name="name">A name for the room just for user identification.</param>
        public Room(List<Cell> room, BSP_Tree partition, string name)
        {
            roomName = name;
            _partition = partition;
            _room = room;
        }
    }
}
