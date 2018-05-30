using System.Collections;
using System.Collections.Generic;
using Utilities;
using System;

namespace PCG
{

    public static class PCG_AgentBased
    {
        /* Algorithm Outline
         * **********************
         *  Note: each corridor and room is added to a stack.
         * 1. Starting in a random height on the left
         * 2. Build a pathway of maximum N length randomly within the bounds
         * 3. Once the path length has been reached, or if the path intersects with a filled tile in all directions or the boundry, the path ends
         * 4. Next, check if a room can be built at the end (the room won't intersect with the boundry or other rooms)
         *      4a. If a room can be built, then build it randomly within the bounds from the point
         *      4b. Then choose a random point along the last built room's boundry
         *      4c. Go back to step 2
         * 5. If that point can't build, randomly choose an existing corridor or room
         *      5a. Go to step 4b.
         *      5b. If all points in the chosen room can't build a path, backtrack through the stack until succesful or the stack is empty
         * 6. The algorithm ends when all possible boundry cells have been tried
         */

        public enum BuildDirection { Up, Down, Right, Left, Invalid };

        private static int _width, _height, _miniumArea, _maximumArea, _minimumCorridorArea, _maximumCorridorLength, _corridorDirectionChangeIncrement, _minimumCorridorLength;
        public static int MinimumArea { get { return _miniumArea; } }//The minimium size a room can be either vertically or horizontally
        public static int MaximumArea { get { return _maximumArea; } }//The minimium size a room can be either vertically or horizontally
        public static int MinimumCorridorArea { get { return _minimumCorridorArea; } }//The minimium size a corridor can be either vertically or horizontally
        public static int MaximumCorridorLength { get { return _maximumCorridorLength; } }//The maximum length a corridor can be
        public static int MinimumCorridorLength { get { return _minimumCorridorLength; } }//The minimum length a corridor can be
        public static List<Room> TotalRooms;//the corridors and rooms that contain the actual cells

        private static List<Room> _roomList;//just the normal rooms
        private static List<Room> _corridorList;//just the corridors

        private static Cell[,] _grid; public static Cell[,] Grid { get { return _grid; } }//each cell represented in a relative position 

        public static List<Room> Generate_Dungeon(int width, int height, int minArea, int maxArea, int minCorridorArea, int minCorridorLength, int maxCorridorLength, int directionChangeChance, int baseBacktrackCount)
        {
            TotalRooms = new List<Room>();
            _roomList = new List<Room>();
            _corridorList = new List<Room>();

            if (!Utilities.PCG_Exceptions.Is_ValidInput_Exception(width, height, minArea, minCorridorArea))
                return null;

            _miniumArea = minArea;
            _maximumArea = maxArea;
            _minimumCorridorArea = minCorridorArea;
            _maximumCorridorLength = maxCorridorLength;
            _minimumCorridorLength = minCorridorLength;
            _width = width;
            _height = height;
            _corridorDirectionChangeIncrement = directionChangeChance;
            _grid = new Cell[_width, _height];

            //Choose random starting point within the area
            int Xpos = 0;
            int Ypos = Utility.RNDG.Next(0, _height);
            Cell currentCell;
            List<Cell> allBoundryCells = new List<Cell>() { new Cell(Xpos, Ypos, _grid) };
            int backtrackCount = baseBacktrackCount;//Keep track of the number of times we had to choose a random cell to backtrack to build from

            while (backtrackCount > 0)//If we have failed too much, then end the dungeon
            {
                //randomly choose a cell from the list of possible boundry cells
                int index = Utility.RNDG.Next(0, allBoundryCells.Count);
                currentCell = allBoundryCells[index];

                //Use it to make a corridor
                Room corridor = Carve_Corridor(currentCell);

                //After it is used to make a corridor, remove it from the list
                allBoundryCells.Remove(currentCell);
                --backtrackCount;//remove every attempt to build from a cell

                if (corridor == null)
                    continue;

                allBoundryCells.AddRange(corridor.BoundryCells);
                TotalRooms.Add(corridor);
                _corridorList.Add(corridor);
                Room room = Carve_Room(corridor);

                if (room != null)//room was successfully built
                {
                    allBoundryCells.AddRange(room.BoundryCells);
                    TotalRooms.Add(room);
                    _roomList.Add(room);
                    ++backtrackCount;//Increment the count every room 
                }

            }

            //Seperate rooms for more control over the order in which tiles are placed
            Place_Walls(_roomList);
            Place_Walls(_corridorList);
            Place_Doors(_roomList);//only nessesary to do this with the rooms
            Fix_InvalidCells(_roomList);
            Fix_InvalidCells(_corridorList);
            return TotalRooms;
        }

        /// <summary>
        /// Given a corridor, build a room of a random size from a valid cell from the given corridor.
        /// </summary>
        /// <param name="lastCorridor"></param>
        /// <returns>Returns a room that was created, or null if no room could be made from the corridor.</returns>
        private static Room Carve_Room(Room lastCorridor)
        {
            //Randomly choose the size for the room
            int width = Utility.RNDG.Next(_miniumArea, _maximumArea);
            int height = Utility.RNDG.Next(_miniumArea, _maximumArea);

            //Get a list of the last N cells from the corridor
            List<Cell> possibleCellsToBuildRoom = new List<Cell>();
            for (int i = System.Math.Max(0, (lastCorridor.RoomCells.Count - _minimumCorridorArea)); i < lastCorridor.RoomCells.Count; i++)
                possibleCellsToBuildRoom.Add(lastCorridor.RoomCells[i]);

            //Find a cell that can support a room big enough
            Cell startingCell = Find_ValidCellForRoom_FromCorridor(possibleCellsToBuildRoom, width, height);
            if (startingCell == null)
                return null;//No cell was found that couild support the room

            //Build the room
            List<Cell> roomCells = new List<Cell>();
            Utility.Fill_Room(_grid, startingCell, roomCells, Cell.CellType.Floor, width, height);

            return roomCells.Count > 0 ? new Room(roomCells, Room.RoomType.Room, "Room") : null;// no cells in the room indicates a failed creation
        }

        /// <summary>
        /// Given a corridor, this function will randomly choose one of the cells along the corridor's final direction. The chosen cell
        /// will be in a central area that can build a room without interesecting with other rooms.
        /// </summary>
        /// <param name="possibleCellsToBuildRoom">The list of cells to randomly choose from.</param>
        /// <param name="width">The width of the room.</param>
        /// <param name="height">The height of the room.</param>
        /// <returns>Returns a cell that is in a large enough area to create a room in, otherwise returns null.</returns>
        private static Cell Find_ValidCellForRoom_FromCorridor(List<Cell> possibleCellsToBuildRoom, int width, int height)
        {
            Cell currentCell = null;

            //Randomly iterate through the cells, trying to see if they are valid
            while (possibleCellsToBuildRoom.Count != 0)
            {
                int index = Utility.RNDG.Next(0, possibleCellsToBuildRoom.Count);
                Cell cellToTry = possibleCellsToBuildRoom[index];

                if (!Check_ValidRoomPlacement(cellToTry, width, height))
                    possibleCellsToBuildRoom.Remove(cellToTry);
                else
                {
                    currentCell = cellToTry;
                    break;
                }
            }

            return currentCell;
        }

        /// <summary>
        /// Given an inital cell, this function will randomly create a corridor for the given length, or it runs out of valid space to build in.
        /// </summary>
        /// <param name="startingCell"></param>
        /// <returns>Returns the completed corridor, or null if it was not successful at creating a corridor of at least 1 cell.</returns>
        private static Room Carve_Corridor(Cell startingCell)
        {
            int currentLength = 0;
            int changeDirectionChance = 0;
            int buildLength = Utility.RNDG.Next(_minimumCorridorLength, _maximumCorridorLength);
            BuildDirection buildDirection = BuildDirection.Right;//Start going to the right
            List<BuildDirection> validDirections = new List<BuildDirection>() { BuildDirection.Up, BuildDirection.Down, BuildDirection.Right, BuildDirection.Left };

            bool wasInvalidDirection = false;//force a change if a invalid direction was chosen
            Cell currentCell = startingCell;
            List<Cell> roomCells = new List<Cell>();

            while (currentLength < buildLength)
            {
                //Randomly choose a valid direction
                if (changeDirectionChance >= Utility.RNDG.Next(1, 101) || wasInvalidDirection)
                {
                    int index = Utility.RNDG.Next(0, validDirections.Count);
                    buildDirection = validDirections[index];
                    changeDirectionChance = 0;
                }

                //Check if it is valid
                if (!Check_ValidCellBuildDirection(currentCell, buildDirection, _minimumCorridorArea + 1))//give a little padding so the corridors dont run adjacent
                {
                    validDirections.Remove(buildDirection);//Remove the direction from the valid directions

                    if (validDirections.Count == 0)//If the valid directions.count == 0, end the path
                        break;

                    wasInvalidDirection = true;
                    continue;
                }
                else//Increment the chance of change
                {
                    wasInvalidDirection = false;
                    changeDirectionChance += _corridorDirectionChangeIncrement;
                }

                //Place the Cell
                currentCell = Place_Cell_In_Direction(currentCell, Cell.CellType.Corridor, buildDirection);
                roomCells.Add(currentCell);
                ++currentLength;

                //Refill the valid directions
                if (validDirections.Count < 4)
                    validDirections = new List<BuildDirection>() { BuildDirection.Up, BuildDirection.Down, BuildDirection.Right, BuildDirection.Left };
            }

            //Fill in the area around each cell in the corridor
            int initalLength = roomCells.Count;
            for (int i = 0; i < initalLength; i++)//iterate through the inital cells and fill around them, adding the fill cells to the same list
                Utility.Fill_Room(_grid,roomCells[i], roomCells, Cell.CellType.Corridor, _minimumCorridorArea, _minimumCorridorArea);

            return roomCells.Count > 0 ? new Room(roomCells, Room.RoomType.Corridor, "Corridor") : null;// no cells in the room indicates a failed creation
        }

        /// <summary>
        /// Places a new cell 1 unit away from a previous cell based on the direction.
        /// </summary>
        /// <param name="previousCell">The location to use as a reference of where to place the new cell based on the direction.</param>
        /// <param name="type">The cellType of the new cell.</param>
        /// <param name="buildDirection">The direction to place the new cell in.</param>
        private static Cell Place_Cell_In_Direction(Cell previousCell, Cell.CellType type, BuildDirection buildDirection)
        {
            Cell newCell;

            //Get the proper coordinates for the new cell
            int signX;
            int signY;
            Get_DirectionSign(out signX, out signY, buildDirection);

            //Place the Cell
            newCell = new Cell(previousCell.X + signX, previousCell.Y + signY, _grid);
            newCell.Cell_Type = Cell.CellType.Corridor;
            _grid[newCell.X, newCell.Y] = newCell;

            return newCell;
        }

        /// <summary>
        /// Checks if there are any empty cells along a direction and a given length.
        /// </summary>
        /// <param name="startingCell">The cell to perform the check from.</param>
        /// <param name="directionToCheck">What direction the check should be performed in.</param>
        /// <param name="count">The length of the check.</param>
        /// <returns>Returns true if all the cells along the given direction for the given length are empty.</returns>
        private static bool Check_ValidCellBuildDirection(Cell startingCell, BuildDirection directionToCheck, int count)
        {
            bool isValid = true;

            for (int i = 1; i <= count; i++)
            {
                switch (directionToCheck)
                {
                    case BuildDirection.Up:
                        if (startingCell.Y + i >= _grid.GetLength(1) || _grid[startingCell.X, startingCell.Y + i] != null)
                            isValid = false;
                        break;
                    case BuildDirection.Down:
                        if (startingCell.Y - i < 0 || _grid[startingCell.X, startingCell.Y - i] != null)
                            isValid = false;
                        break;
                    case BuildDirection.Right:
                        if (startingCell.X + i >= _grid.GetLength(0) || _grid[startingCell.X + i, startingCell.Y] != null)
                            isValid = false;
                        break;
                    case BuildDirection.Left:
                        if (startingCell.X - i < 0 || _grid[startingCell.X - i, startingCell.Y] != null)
                            isValid = false;
                        break;
                    case BuildDirection.Invalid:
                    default:
                        isValid = false;
                        break;
                }

                if (isValid == false)
                    break;
            }

            return isValid;
        }

        /// <summary>
        /// Checks if a given central cell has enough empty space or corridors around it to create a room.
        /// </summary>
        /// <param name="cellToTry">The centre cell.</param>
        /// <param name="width">The width to check.</param>
        /// <param name="height">The height to check.</param>
        /// <returns>Returns true if the area has enough empty space or corridors around it, false otherwise if the area intersects with another room.</returns>
        private static bool Check_ValidRoomPlacement(Cell cellToTry, int width, int height)
        {
            int offsetX, offsetY;
            Utility.Get_RoomCentreOffsets(_grid,cellToTry, width, height, out offsetX, out offsetY);

            for (int x = offsetX; x < offsetX + width; x++)
            {
                for (int y = offsetY; y < offsetY + height; y++)
                {
                    if (_grid[x, y] != null && _grid[x, y].Cell_Type != Cell.CellType.Corridor)//only allow null spaces or existing corridors
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Given a direction, this function outputs an integer value for the X and Y directions to move on the Grid.
        /// </summary>
        /// <param name="signX"></param>
        /// <param name="signY"></param>
        /// <param name="direction"></param>
        private static void Get_DirectionSign(out int signX, out int signY, BuildDirection direction)
        {
            switch (direction)//Randomly choose a boundry list and set the proper dimensions
            {
                case BuildDirection.Up://Upper
                    signX = 0;
                    signY = 1;
                    break;
                case BuildDirection.Down://Lower
                    signX = 0;
                    signY = -1;
                    break;
                case BuildDirection.Right://Right
                    signX = 1;
                    signY = 0;
                    break;
                case BuildDirection.Left://Left
                    signX = -1;
                    signY = 0;
                    break;
                default:
                    signX = 0;
                    signY = 0;
                    break;
            }
        }

        /// <summary>
        /// Given a list of rooms, iterate through all their cells and place walls in valid positions:
        /// <para>Case 1: Boundry walls, walls that are on the outside of the dungeon. </para>
        /// <para>Case 2: Corridor walls, walls that are adjacent to a corridor. </para>
        /// </summary>
        /// <param name="rooms"></param>
        private static void Place_Walls(List<Room> rooms)
        {
            foreach (var room in rooms)
            {
                foreach (var cell in room.BoundryCells)
                {
                    //Case 1: boundry wall
                    if (cell.AdjacentCells.Count < 4)
                    {
                        cell.Cell_Type = Cell.CellType.Wall;
                        continue;
                    }

                    //Case 2: Corridor wall
                    if (cell.Cell_Type == Cell.CellType.Floor)
                    {
                        foreach (var adjacentCell in cell.AdjacentCells)
                        {
                            if (adjacentCell.Cell_Type == Cell.CellType.Corridor)
                            {
                                cell.Cell_Type = Cell.CellType.Wall;
                                break;
                            }
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Given a list of rooms, iterates through the wall cells in a room to find a cell adjacent to a corridor.
        /// </summary>
        /// <param name="rooms"></param>
        private static void Place_Doors(List<Room> rooms)
        {
            foreach (var room in rooms)
            {
                foreach (var cell in room.BoundryCells)
                {
                    if (cell.Cell_Type == Cell.CellType.Wall && cell.AdjacentCells.Count == 4)//ensure the door is not on the boundry
                    {
                        //adjacent counts
                        int wallCount_Adj = Utility.Get_CountOfCelltypeInRoom(Cell.CellType.Wall, cell.AdjacentCells, ((x) => true));
                        int floorCount_Adj = Utility.Get_CountOfCelltypeInRoom(Cell.CellType.Floor, cell.AdjacentCells, ((x) => true));
                        int corridorCount_Adj = Utility.Get_CountOfCelltypeInRoom(Cell.CellType.Corridor, cell.AdjacentCells, ((x) => true));
                        int emptyCount_Adj = Utility.Get_CountOfCelltypeInRoom(Cell.CellType.Empty, cell.AdjacentCells, ((x) => true));

                        int doorCount_Diagonal = Utility.Get_CountOfCelltypeInRoom(Cell.CellType.Door, cell.DiagonalCells, ((x) => true));

                        foreach (var adjacent in cell.AdjacentCells)
                        {
                            if (adjacent.room != cell.room && !cell.room.Is_ConnectedToRoom(adjacent.room) &&//differnt rooms that havent been connected already
                                adjacent.AdjacentCells.Count == 4 && //ensure its not on the boundry
                                adjacent.Cell_Type != Cell.CellType.Empty && emptyCount_Adj == 0 &&//ensure the door wont lead to nowhere
                                doorCount_Diagonal == 0 && //dont place doors together if they're diagonal
                                wallCount_Adj < 3 && //ensure it is not surrounded by walls, ie a corner
                                ((floorCount_Adj < 3 && corridorCount_Adj == 1) || (floorCount_Adj == 1 && corridorCount_Adj < 3)))// ensure it is not in the middle of floorspace
                            {
                                cell.Cell_Type = Cell.CellType.Door;

                                //Connect the rooms
                                cell.room.ConnectRoom(adjacent.room);
                            }
                        }

                    }
                }
            }
        }

        /// <summary>
        /// A final pass on the dungeon to fix any artifacts created by the generation.
        /// <para>Case 1: Closed off corner corridors: Carves out a floor and a door from the room's walls.</para>
        /// Case 2: Too thick walls: The wall cells are set to empty.
        /// </summary>
        /// <param name="rooms"></param>
        private static void Fix_InvalidCells(List<Room> rooms)
        {
            List<Cell> invalidCells = new List<Cell>();
            List<Cell> newFloorCells = new List<Cell>();
            List<Cell> newDoorCells = new List<Cell>();

            foreach (var room in rooms)
            {
                foreach (var cell in room.RoomCells)
                {
                    //Case 1: Closed off corridors
                    // If a corridor is built on the corner of a room, there might not be a door.
                    foreach (var adjacentCell in cell.AdjacentCells)
                    {
                        if (!cell.room.Is_ConnectedToRoom(adjacentCell.room))
                        {
                            if (cell.AdjacentCells.Count == 4 && cell.room.Room_Type == Room.RoomType.Room && cell.Cell_Type == Cell.CellType.Wall &&  //if im a room wall,
                                Utility.Get_CountOfCelltypeInRoom(Cell.CellType.Wall, cell.AdjacentCells, ((x) => x.room == cell.room)) == 2 && //and have 2 wall adjacents from the same room,
                                (Utility.Get_CountOfCelltypeInRoom(Cell.CellType.Wall, cell.AdjacentCells, ((x) => x.room.Room_Type == Room.RoomType.Corridor)) == 1//and only 1 corridor wall adjacent
                                || Utility.Get_CountOfCelltypeInRoom(Cell.CellType.Wall, cell.DiagonalCells, ((x) => x.room.Room_Type == Room.RoomType.Corridor)) == 1))
                            {
                                if (Utility.Get_CountOfCelltypeInRoom(Cell.CellType.Corridor, cell.AdjacentCells, ((x) => x.room.Room_Type == Room.RoomType.Corridor)) == 1)
                                    newDoorCells.Add(cell);
                                else
                                    newFloorCells.Add(cell);
                                continue;
                            }
                        }
                    }

                    int wallCount_Adj = Utility.Get_CountOfCelltypeInRoom(Cell.CellType.Wall, cell.AdjacentCells, ((x) => true));
                    int wallCount_Dia = Utility.Get_CountOfCelltypeInRoom(Cell.CellType.Wall, cell.DiagonalCells, ((x) => true));

                    //Case 2: Walls that are over 1 unit in thickness.
                    //cell is on the outside boundry and neighbored by walls, orCell is surrounded by walls
                    if (wallCount_Adj == 4 ||
                        (cell.Cell_Type == Cell.CellType.Wall && wallCount_Adj == cell.AdjacentCells.Count) && // all adjacencts are walls
                       wallCount_Dia == cell.DiagonalCells.Count) //all diagonals are walls
                    {
                        invalidCells.Add(cell);
                        continue;
                    }
                }
            }

            //Set them all empty at once, or else the logic will become flawed if the cells are changing midway through the inital iteration
            foreach (var cell in invalidCells)
            {
                cell.Cell_Type = Cell.CellType.Empty;
            }
            //Set the proper floor cells
            foreach (var cell in newFloorCells)
            {
                cell.Cell_Type = Cell.CellType.Floor;
            }
            //Set the proper door cells
            foreach (var cell in newDoorCells)
            {
                cell.Cell_Type = Cell.CellType.Door;
            }
        }

    }
}


