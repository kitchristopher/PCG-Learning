using System.Collections.Generic;

namespace PCG
{
    /// <summary>
    /// A class that contains what type of structure the cell is, as well as it's relative position of creation in the grid.
    /// </summary>
    public class Cell
    {
        public enum CellType { Empty, Wall, Floor, Corridor, Door, Rock };//a classification method to represent what this cell is in the dungeon

        public Room room;//The room this cell lies in

        private List<Cell> _adjacents = new List<Cell>();
        private List<Cell> _diagonals = new List<Cell>();
        public List<Cell> AdjacentCells { get { return _adjacents; } }//the cells adjacent to this cell 
        public List<Cell> DiagonalCells { get { return _diagonals; } }//the cells diagonal to this cell 

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
            Add_Diagonals(grid);
        }

        /// <summary>
        /// Add the adjacent cells to the AdjacentCells List and vice versa.
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
            //Right and Up adjacent
            if (_x != grid.GetLength(0) - 1 && (_x + 1 < grid.GetLength(0)) && grid[_x + 1, _y] != null)
            {
                AdjacentCells.Add(grid[_x + 1, _y]);

                //and Vice Versa
                grid[_x + 1, _y].AdjacentCells.Add(this);
            }
            if (_y != grid.GetLength(1) - 1 && (_y + 1 < grid.GetLength(1)) && grid[_x, _y + 1] != null)
            {
                AdjacentCells.Add(grid[_x, _y + 1]);

                //and Vice Versa
                grid[_x, _y + 1].AdjacentCells.Add(this);
            }
        }

        /// <summary>
        /// Add the diagonal cells to the DiagonalCells List and vice versa.
        /// </summary>
        /// <param name="grid">The grid in which this cell and its neighbors are stored in.</param>
        private void Add_Diagonals(Cell[,] grid)
        {
            //leftdown Diagonal
            if (_x != 0 && _y != 0 && grid[_x - 1, _y - 1] != null)
            {
                DiagonalCells.Add(grid[_x - 1, _y - 1]);

                //and Vice Versa
                grid[_x - 1, _y - 1].DiagonalCells.Add(this);
            }
            //downright
            if (_y != 0 && _x != grid.GetLength(0) - 1 && grid[_x + 1, _y - 1] != null)
            {
                DiagonalCells.Add(grid[_x + 1, _y - 1]);

                //and Vice Versa
                grid[_x + 1, _y - 1].DiagonalCells.Add(this);
            }
            //rightup
            if (_x != grid.GetLength(0) - 1 && _y != grid.GetLength(1) - 1 && (_x + 1 < grid.GetLength(0)) && grid[_x + 1, _y + 1] != null)
            {
                DiagonalCells.Add(grid[_x + 1, _y + 1]);

                //and Vice Versa
                grid[_x + 1, _y + 1].DiagonalCells.Add(this);
            }
            //leftup
            if (_y != grid.GetLength(1) - 1 && _x != 0 && (_y + 1 < grid.GetLength(1)) && grid[_x - 1, _y + 1] != null)
            {
                DiagonalCells.Add(grid[_x - 1, _y + 1]);

                //and Vice Versa
                grid[_x - 1, _y + 1].DiagonalCells.Add(this);
            }
        }
    }

    /// <summary>
    /// Contains a group of cells created together.
    /// </summary>
    public class Room
    {
        public enum RoomType { Room, Corridor };
        private RoomType _roomType; public RoomType Room_Type { get { return _roomType; } }

        public string roomName;// A name for the room just for user identification
        private List<Cell> _roomCells; public List<Cell> RoomCells { get { return _roomCells; } }//The list of cells in the room

        private List<Room> _connectedRooms = new List<Room>(); public List<Room> ConnectedRooms { get { return _connectedRooms; } }
        private List<Cell> _boundryCells = new List<Cell>(); public List<Cell> BoundryCells { get { return _boundryCells; } }

        public Room(List<Cell> roomCells, RoomType roomType, string name)
        {
            roomName = name;
            _roomCells = roomCells;
            _roomType = roomType;
            Initalize_Cells();
        }

        /// <summary>
        /// Adds the input room to this room's ConnectedRoom list and vice versa.
        /// </summary>
        /// <param name="room">The room to connect this room to and vice versa.</param>
        public void ConnectRoom(Room room)
        {
            if (room == this)//no need to add own room to the connected room list
                return;

            if (!this.ConnectedRooms.Contains(room))
                this.ConnectedRooms.Add(room);

            if (!room.ConnectedRooms.Contains(this))
                room.ConnectedRooms.Add(this);
        }

        /// <summary>
        /// Checks if the input room has been connected to this room, or if the input room is itself.
        /// </summary>
        /// <param name="room">The room to check if it is connected.</param>
        /// <returns>Returns true if the input room is contained in the connected rooms list, or if the input room is itself.</returns>
        public bool Is_ConnectedToRoom(Room room)
        {
            if (this.ConnectedRooms.Contains(room) || room == this)//being connected to self is valid
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks if any of this room's connected rooms have been connected to the otherRoom.
        /// </summary>
        /// <param name="otherRoom">The room being checked to see if it is indirectly connected to this room.</param>
        /// <returns>Returns true if the otherRoom is connected to at least one of the this room's connected rooms.</returns>
        public bool Is_IndirectlyConnected(Room otherRoom)
        {
            foreach (var connectedRoom in this.ConnectedRooms)
            {
                if (connectedRoom.Is_ConnectedToRoom(otherRoom))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the given cell from this room and sets the cell's room to null. The boundry of the room is then recaluated.
        /// </summary>
        /// <param name="cellToRemove"></param>
        /// <returns>Returns true if the cell was removed.</returns>
        public bool Remove_CellFromRoom(Cell cellToRemove)
        {
            if (_roomCells.Contains(cellToRemove))
            {
                _roomCells.Remove(cellToRemove);
                cellToRemove.room = null;

                //Recalculate the boundry
                Calulate_BoundryCells();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Assigns this room to all of the cells contained in this room.
        /// </summary>
        private void Initalize_Cells()
        {
            foreach (var cell in _roomCells)
            {
                cell.room = this;
            }

            Calulate_BoundryCells();
        }

        /// <summary>
        /// Calculates which cells are on the boundry of the room. 
        /// <para>Case 1: A cell on the edge of the dungeon. (less than four adjacent cells). </para>
        /// <para>Case 2: A cell that is neighbored to another room.</para>
        /// </summary>
        private void Calulate_BoundryCells()
        {
            _boundryCells.Clear();

            foreach (var cell in _roomCells)
            {
                if (cell.AdjacentCells.Count < 4 || Is_Cell_NeighboringOtherRoom(cell))
                    _boundryCells.Add(cell);
            }
        }

        /// <summary>
        /// Checks if the input cell is neighboring at least one other room.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>Returns true if the input cell is adjacent to a cell in another room.</returns>
        private bool Is_Cell_NeighboringOtherRoom(Cell cell)
        {
            foreach (var adjacentCell in cell.AdjacentCells)
            {
                if (adjacentCell.room != cell.room)
                    return true;
            }

            return false;
        }

    }
}
