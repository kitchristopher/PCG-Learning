using System.Collections;
using System.Collections.Generic;
using Utilities;
using System;

namespace PCG
{
    public static class PCG_CellularAutomata
    {
        /* Algorithim Outline
         *
         * Parameters:
         * -Percentage of rocks in the inital sprinkling
         * -Number of generations to run
         * -Neighborhood threshold value to turn into a rock
         * -Number of cells in a neighborhood
         * ********************
         * States:
         * -Rock
         * -Floor
         * -Wall
         * 
         **********************
         * 1. Generate the inital room, with an outside wall boundry to prevent rooms from being chopped off later when creating the walls.
         * 2. Sprinkle the grid randomly with rocks based on a 50% probability.
         * 3. Iterate through each room for n many generations, perfoming:
         *      3a. For each cell, if the number of other cells in the neighborhood pass the threshold value,
         *          turn the cell into a rock.
         * 4. Perform a flood fill to find each area of open space.
         * 5. Fill in all open spaces that are too small to be meaningful.
         * 6. Connect each open space to one another, based on the closest rooms that are not indirectly connected.
         * 7. After completing the automata generations, turn all cells adjacent to a floor tile into a wall.
         */

        public enum NeighborhoodType
        {
            Moore,      //Use both adjacent and diagonal cells.
            vonNeumann  //Just use the adjacent cells.
        };
        public static NeighborhoodType neighborhoodType;//Which cells are considered to be a part of the neighborhood


        private static int _width, _height, _miniumArea, _minimumCorridorArea, _rockPercentage, _automataGenerations, _rockThreshold;
        public static int MinimumArea { get { return _miniumArea; } }//The minimium size a room can be either vertically or horizontally
        public static int MinimumCorridorArea { get { return _minimumCorridorArea; } }//The minimium size a corridor can be either vertically or horizontally
        public static int AutomataGenerations { get { return _automataGenerations; } }//How many times the automata is run on a room
        public static int RockPercentage { get { return _rockPercentage; } }//The percent of rocks randomly created initally
        public static int RockThreshold { get { return _rockThreshold; } }//The number of neighbor cells that must be rocks to change state for a cell
        public static List<Room> TotalRooms;//the corridors and rooms that contain the actual cells

        private static List<Room> _openAreas;//all of the areas that have a large enough space in them
        private static List<Room> _closedAreas;//all of the areas that are just rock
        private static List<Room> _corridors;//all of the created connecting corridors
        private static List<Cell> _checkedFillCells;//All the cells that have been checked with the floodfill 
        private static List<Cell> _outerWallBoundry;//The cells on the outside boundries

        private static Cell[,] _grid; public static Cell[,] Grid { get { return _grid; } }//each cell represented in a relative position 

        public static List<Room> Generate_Dungeon(int width, int height, int rockPercentage, int rockThreshold, int automataGenerations, int minimumArea,
            int minimumCorridorArea, NeighborhoodType neighborhood_Type, bool shouldConnectAreas = true)
        {
            TotalRooms = new List<Room>();
            _openAreas = new List<Room>();
            _closedAreas = new List<Room>();
            _corridors = new List<Room>();
            _checkedFillCells = new List<Cell>();
            _outerWallBoundry = new List<Cell>();

            _width = width;
            _height = height;
            _rockPercentage = rockPercentage;
            _rockThreshold = rockThreshold;
            _miniumArea = minimumArea;
            _automataGenerations = automataGenerations;
            _minimumCorridorArea = minimumCorridorArea;
            neighborhoodType = neighborhood_Type;
            _grid = new Cell[_width, _height];

            Room initalRoom = Create_InitalRoom(0, 0, _width, _height);

            //Perform Automata 
            for (int i = 0; i < _automataGenerations; i++)
            {
                Change_FloorsToRocks_OnAutomata(initalRoom);
            }

            //Find all areas via flood fill
            Find_FloodFillCellsFromRoom(initalRoom);

            //Fill in all areas that are too small to bother connecting
            Fill_TooSmallAreas();

            //Connect the open rooms
            if (shouldConnectAreas)
                Connect_ClosestRoom(_openAreas);

            //Add all the rooms
            foreach (var room in _corridors)
            {
                TotalRooms.Add(room);
            }
            foreach (var room in _openAreas)
            {
                TotalRooms.Add(room);
            }
            foreach (var room in _closedAreas)
            {
                if (room.RoomCells.Count != 0)//When building corridors, the rock rooms may lose all their cells, so check that they arent empty
                    TotalRooms.Add(room);
            }

            TotalRooms.Add(new Room(_outerWallBoundry, Room.RoomType.Room, "Outside Wall Boundry"));

            //Place walls on boundry
            Place_BoundryWalls();

            return TotalRooms;
        }

        /// <summary>
        /// Iterate through the given list of rooms, and connect each room with a corridor to the closest room that has not been indirectly connected yet.
        /// </summary>
        /// <param name="rooms"></param>
        private static void Connect_ClosestRoom(List<Room> rooms)
        {
            foreach (var room in rooms)
            {
                Room closestRoom = null;
                int closestDistance = int.MaxValue;
                //connect to closest rooms that arent connected to yourself
                foreach (var otherRoom in rooms)
                {
                    if (otherRoom == room || room.Is_ConnectedToRoom(otherRoom) ||//skip if the rooms are already connected
                        room.Is_IndirectlyConnected(otherRoom))//skip this room if it has been connected to a room that is connected to this room
                        continue;

                    int currentDistance = Math.Abs(room.RoomCells[0].X - otherRoom.RoomCells[0].X) + Math.Abs(room.RoomCells[0].Y - otherRoom.RoomCells[0].Y);

                    if (currentDistance <= closestDistance)
                    {
                        closestDistance = currentDistance;
                        closestRoom = otherRoom;
                    }
                }

                room.ConnectRoom(closestRoom);
                Room corridor = Build_ConnectingCorridor(_grid, room, closestRoom);
                _corridors.Add(corridor);
            }
        }

        /// <summary>
        /// Creates a corridor room from the closest points from two rooms.
        /// </summary>
        /// <param name="grid">The Cell[,] storing the dungeon.</param>
        /// <param name="room1">The first room.</param>
        /// <param name="room2">The last room.</param>
        /// <returns>Returns a new room containing the created corridor.</returns>
        private static Room Build_ConnectingCorridor(Cell[,] grid, Room room1, Room room2)
        {
            List<Cell> roomCells = new List<Cell>();

            //Find the closest points in each room
            Cell room1Start;
            Cell room2Start;
            Utility.Find_ClosestTwoPoints_FromRooms(room1, room2, out room1Start, out room2Start);

            //The current position of the pathway
            int currentPointX = room1Start.X;
            int currentPointY = room1Start.Y;

            //Figure out What direction the corridor needs to go
            int XDirection = room1Start.X - room2Start.X > 0 ? -1 : 1;
            int YDirection = room1Start.Y - room2Start.Y > 0 ? -1 : 1;

            while ((currentPointX - room2Start.X) != 0 || (currentPointY - room2Start.Y) != 0)
            {
                if ((currentPointX - room2Start.X) != 0)//Go horizontal
                    currentPointX += XDirection;
                if ((currentPointY - room2Start.Y) != 0)//Go vertical
                    currentPointY += YDirection;

                Cell currentCell = _grid[currentPointX, currentPointY];
                currentCell.room.Remove_CellFromRoom(currentCell);//Take the current cell and move it into the new corridor room and out of the old room
                roomCells.Add(currentCell);
                currentCell.Cell_Type = Cell.CellType.Floor;//change it into a floor just in case
            }

            //Fill out the created corridor to the proper path
            List<Cell> fillCells = Utility.Copy_List<Cell>(roomCells);//copy the list since the filled cells will be added to the roomCells and this will affect the iteration
            foreach (var cell in fillCells)
            {
                Utility.Fill_Room(_grid, cell, roomCells, Cell.CellType.Floor, _minimumCorridorArea, _minimumCorridorArea);
            }

            return new Room(roomCells, Room.RoomType.Corridor, "Corridor");
        }

        /// <summary>
        /// Given a room, iterates through all of the cells, and performs flood fill on each unfilled cell.
        /// </summary>
        /// <param name="room"></param>
        private static void Find_FloodFillCellsFromRoom(Room room)
        {
            foreach (var cell in room.RoomCells)
            {
                if (!_checkedFillCells.Contains(cell))//This cell hasn't been checked yet and is a valid type
                {
                    if (cell.Cell_Type == Cell.CellType.Floor)
                        _openAreas.Add(Create_FloodFillRoom(cell, Cell.CellType.Floor, "Cavern"));
                    else if (cell.Cell_Type == Cell.CellType.Rock)
                        _closedAreas.Add(Create_FloodFillRoom(cell, Cell.CellType.Rock, "Rock"));
                }
            }
        }

        /// <summary>
        /// Fills all open areas with rock that are smaller than the minimum area
        /// and moves the area to the closedArea list
        /// </summary>
        private static void Fill_TooSmallAreas()
        {
            for (int i = 0; i < _openAreas.Count; i++)
            {
                if (_openAreas[i].RoomCells.Count == 0)
                {
                    _openAreas.Remove(_openAreas[i]);
                    --i;
                    continue;
                }

                if (_openAreas[i].RoomCells.Count < _miniumArea)
                {
                    foreach (var cell in _openAreas[i].RoomCells)
                    {
                        cell.Cell_Type = Cell.CellType.Rock;
                    }

                    _openAreas[i].roomName = "Rock";
                    _closedAreas.Add(_openAreas[i]);
                    _openAreas.Remove(_openAreas[i]);

                    --i;
                }
            }
        }

        /// <summary>
        /// Changes cells into wall if the cell is a rock tile that is adjacent to a floor tile
        /// </summary>
        private static void Place_BoundryWalls()
        {
            for (int x = 0; x < _grid.GetLength(0); x++)
            {
                for (int y = 0; y < _grid.GetLength(1); y++)
                {
                    if (_grid[x, y].Cell_Type == Cell.CellType.Rock && // a rock that is adjacent to a floor tile
                       Utility.Get_CountOfCelltypeInRoom(Cell.CellType.Floor, _grid[x, y].AdjacentCells, ((c) => true)) > 0)
                    {
                        _grid[x, y].Cell_Type = Cell.CellType.Wall;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new room based on all of the cells indirectly touching the starting cell that match the input CellType.
        /// </summary>
        /// <param name="startingCell">The cell to perform the flood fill from.</param>
        /// <param name="floodCellType">The type of cell that will be filled.</param>
        /// <param name="roomName">What to name the room created by the fill.</param>
        /// <returns>A new room of all of the connected cells of the input type.</returns>
        private static Room Create_FloodFillRoom(Cell startingCell, Cell.CellType floodCellType, string roomName)
        {
            Cell currentCell = startingCell;

            List<Cell> roomCells = new List<Cell>();
            Stack<Cell> cellsToCheck = new Stack<Cell>();

            roomCells.Add(currentCell);
            cellsToCheck.Push(currentCell);
            _checkedFillCells.Add(currentCell);

            while (cellsToCheck.Count != 0)
            {
                //Iterate through all neighbors
                //add to list if they are match the flood cell type
                foreach (var neighbor in currentCell.AdjacentCells)
                {
                    if (neighbor.Cell_Type != floodCellType)
                        continue;

                    //don't check this cell if its been check already
                    if (roomCells.Contains(neighbor))
                        continue;

                    cellsToCheck.Push(neighbor);
                    roomCells.Add(neighbor);
                    _checkedFillCells.Add(neighbor);
                }

                currentCell = cellsToCheck.Pop();
            }

            return new Room(roomCells, Room.RoomType.Room, roomName);
        }

        /// <summary>
        /// Creates a base room with a starting coordinate at the given X and Y location, and the area based on the given dimensions.
        /// Each cell in the room will initally be created as a floor, but some will randomly be created as rocks based on rockPercentage.
        /// </summary>
        /// <param name="startX">The starting corner's X position. </param>
        /// <param name="startY">The starting corner's Y position. </param>
        /// <param name="width">How many cells wide the room is.</param>
        /// <param name="height">How many cells tall the room is.</param>
        /// <returns>Returns the created room.</returns>
        private static Room Create_InitalRoom(int startX, int startY, int width, int height)
        {
            List<Cell> cells = new List<Cell>();

            //Fill out the room based on its starting point and dimensions
            for (int x = startX; x < width; x++)
            {
                for (int y = startY; y < height; y++)
                {
                    Cell newCell = new Cell(x, y, _grid);

                    if (x == startX || y == startY || x == width - 1 || y == height - 1)//Place border walls immediatly 
                    {
                        _outerWallBoundry.Add(newCell);
                        newCell.Cell_Type = Cell.CellType.Wall;
                    }
                    else if (Utility.RNDG.Next(0, 100) >= _rockPercentage)//Randomly create rocks
                        newCell.Cell_Type = Cell.CellType.Floor;
                    else
                        newCell.Cell_Type = Cell.CellType.Rock;

                    cells.Add(newCell);
                    _grid[x, y] = newCell;
                }
            }

            return new Room(cells, Room.RoomType.Room, "Cave");
        }

        /// <summary>
        /// This is the automation step that transforms floor tiles into rock tiles. Given a room, each cell is iterated over and if enough of its
        /// neighbors are rocks, then it will change into a rock as well.
        /// </summary>
        /// <param name="room"></param>
        private static void Change_FloorsToRocks_OnAutomata(Room room)
        {
            List<Cell> toTurnToRock = new List<Cell>();//all of the cells that will be changed to rock

            foreach (var cell in room.RoomCells)
            {
                if (cell.Cell_Type == Cell.CellType.Wall)
                    continue;

                //Get the count of rock cells in the current cell's neighborhood
                int rockCells = 0;
                rockCells += Utility.Get_CountOfCelltypeInRoom(Cell.CellType.Rock, cell.AdjacentCells, ((x) => true));

                if (neighborhoodType == NeighborhoodType.Moore)
                    rockCells += Utility.Get_CountOfCelltypeInRoom(Cell.CellType.Rock, cell.DiagonalCells, ((x) => true));

                if (rockCells >= _rockThreshold)
                    toTurnToRock.Add(cell);
            }

            foreach (var cell in toTurnToRock)//turn all of cells to rock at the same time, otherwise they will affect the rest of the automata
            {
                cell.Cell_Type = Cell.CellType.Rock;
            }
        }

    }

}

