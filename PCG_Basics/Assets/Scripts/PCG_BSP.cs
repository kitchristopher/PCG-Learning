using System.Collections.Generic;
using System;

namespace PCG
{
    /// <summary>
    /// A class that handles the Generation of the cells based on Binary Space Partitioning that is DECOUPlED from MonoBehavior.
    /// </summary>
    public static class PCG_BSP
    {
        /* Algorithm Outline
         * This class contains a static list<Rooms> that the BSP_Tree class will add to. After the BSP_Trees have created the dungeon,
         * this class will simply return the list of rooms.
         * 
         * 1. Create a base Cell[width,height] array and populate it with cells based on their relative position to the array.
         * 2. Partition the array recursivly in halves until it cannot be made smaller than it's minimum area.
         * 3. At the lowest layer, randomly build rooms within each partition.
         * 4. Starting at the lowest parent layer, combine each room of the children and connect them with a corridor, recursivly working upward to the top.
         * 5. Create the walls around the dungeon.
         */

        public static List<Room> Rooms;//the corridors and rooms that contain the actual cells

        private static int _width, _height, _miniumArea, _minimumCorridorArea;
        public static int MinimumArea { get { return _miniumArea; } }//The minimium size a room can be either vertically or horizontally
        public static int MinimumCorridorArea { get { return _minimumCorridorArea; } }//The minimium size a corridor can be either vertically or horizontally

        private static Cell[,] _grid; public static Cell[,] Grid { get { return _grid; } }//each cell represented in a relative position 
        public static System.Random RNDG = new System.Random();

        /// <summary>
        /// Generate a BSP dungeon.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="minArea"></param>
        /// <param name="minCorridorArea"></param>
        /// <returns>Returns a list of Rooms, each of which contain their respective cells.</returns>
        public static List<Room> Generate_Dungeon(int width, int height, int minArea, int minCorridorArea)
        {
            Rooms = new List<Room>();

            if (!Is_ValidInput_Exception(width, height, minArea, minCorridorArea))
                return null;

            _miniumArea = minArea;
            _minimumCorridorArea = minCorridorArea;
            _width = width;
            _height = height;

            _grid = new Cell[_width, _height];

            var rootPartition = new BSP_Tree(Create_InitalPartition());//This is the first partition

            rootPartition.Partition();
            rootPartition.Generate_Dungeon();

            return Rooms;
        }

        /// <summary>
        /// Check that proper values have been used: No negative sizes, or a minimum room size larger than the dungeon.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="minArea"></param>
        /// <param name="minCorridorArea"></param>
        private static bool Is_ValidInput_Exception(int width, int height, int minArea, int minCorridorArea)
        {
            try
            {
                if (width < 0 || height < 0 || minArea < 0 || minCorridorArea < 0)
                {
                    throw new Exception("An input has a negative value!");
                }
            }
            catch (Exception e)
            {
                return false;
            }

            try
            {
                if (width < minArea || height < minArea)
                {
                    throw new Exception("The size of the dungeon is smaller than the Minimum Area for a room!");
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates the first partition and fills it with its cells. 
        /// </summary>
        /// <returns>Returns a full cell[,] of created cells.</returns>
        private static Cell[,] Create_InitalPartition()
        {
            Cell[,] rootPartition = new Cell[_width, _height];

            //Set the Inital Cells for the Root Partition
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Cell cell = new Cell(x, y, _grid);
                    _grid[x, y] = cell;
                    rootPartition[x, y] = cell;
                }
            }

            return rootPartition;
        }
    }

    /// <summary>
    /// A recurisve Binary Tree that contains functions to generate a dungeon. The BSP contains a c
    /// </summary>
    public class BSP_Tree
    {
        public BSP_Tree LeftLeaf = null, RightLeaf = null, Parent = null;

        private Cell[,] _partition; public Cell[,] Cells { get { return _partition; } }//the cells based on their relative position in this BSP
        private Room _room;//The room contained inside this partition

        public BSP_Tree(Cell[,] partition)
        {
            _partition = partition;
        }

        /// <summary>
        /// Recursivly divides this BSP into halves, creating children BSPs in the process, until they are too small to be cut in either direction. 
        /// </summary>
        public void Partition()
        {
            bool canHorizontalCut = (Cells.GetLength(0) - PCG_BSP.MinimumArea >= PCG_BSP.MinimumArea);
            bool canVerticalCut = (Cells.GetLength(1) - PCG_BSP.MinimumArea >= PCG_BSP.MinimumArea);

            if (!(canHorizontalCut && canVerticalCut))// Ensure that the partition can be cut from either side
                return;

            //Create two new partitions
            BSP_Tree partitionA;
            BSP_Tree partitionB;//This is either to the right of A or below A, depending on if it was a vertical or horizontal cut

            //Divide in half randomly
            int CutX_B = 0, CutY_B = 0;//The positions to cut for partitionB's side
            int CutX_A, CutY_A;//The positions to cut for partitionA's side

            //Randomly make a Cut
            Set_PartitionCutSize(ref CutX_B, ref CutY_B, Cells);

            //Initalize the new partitions to the proper sizes
            partitionB = new BSP_Tree(new Cell[Cells.GetLength(0) - CutX_B, Cells.GetLength(1) - CutY_B]);//The new side
            /*In order to get the size and position for partitionA:
            Since cut direction is random, we need to set partitionA's cut to be either where the cut is if it is on that axis,
            otherwise it then must be set to the parent partition's size since cutB would be 0 on that axis if no cut was made. */
            CutX_A = CutX_B == 0 ? Cells.GetLength(0) : CutX_B;
            CutY_A = CutY_B == 0 ? Cells.GetLength(1) : CutY_B;
            partitionA = new BSP_Tree(new Cell[CutX_A, CutY_A]); //the remaining side

            //Iterate through the old partition and if the cell is lesser than one half add it to A, else to B
            AddTo_NewPartitions(this, partitionA, partitionB, CutX_B, CutY_B, CutX_A, CutY_A);

            //Update the links to the leaf nodes
            LeftLeaf = partitionA;
            RightLeaf = partitionB;
            partitionA.Parent = this;
            partitionB.Parent = this;

            //Recursivly partition each side until they are as small as possible
            partitionA.Partition();
            partitionB.Partition();
        }

        /// <summary>
        /// Randomly chooses a X or Y size to cut within the partition size but , and uploads that position to the referenced inputs.
        /// <para>NOTE: the cuts are based on the size of how many cells are along that axis; they are NOT indices. In other words, the
        /// value that is returned is always 1 higher than the its index.</para>
        /// </summary>
        /// <param name="CutX_B">The X position referenced to cut.</param>
        /// <param name="CutY_B">The Y position referenced to cut.</param>
        /// <param name="partition">The cell[,] used for the cutting dimensions.</param>
        private void Set_PartitionCutSize(ref int CutX_B, ref int CutY_B, Cell[,] partition)//take only 1 vavlue, but also an index to cut
        {
            if (PCG_BSP.RNDG.Next(0, 2) == 0)//Cut along X axis
                CutX_B = PCG_BSP.RNDG.Next(PCG_BSP.MinimumArea, partition.GetLength(0) - PCG_BSP.MinimumArea + 1);
            else//Cut along Y axis
                CutY_B = PCG_BSP.RNDG.Next(PCG_BSP.MinimumArea, partition.GetLength(1) - PCG_BSP.MinimumArea + 1);
        }

        /// <summary>
        /// Adds the cells in the old partition into the new partitions based upon the cut sizes.
        /// <para>Cells that are left or below the cut are added to partitionA, the other cells to partitionB.</para>
        /// </summary>
        /// <param name="oldPartition"></param>
        /// <param name="partitionA">The left or lower side of the cut.</param>
        /// <param name="partitionB">The right or upper side of the cut.</param>
        /// <param name="CutX_B"></param>
        /// <param name="CutY_B"></param>
        /// <param name="CutX_A"></param>
        /// <param name="CutY_A"></param>
        private void AddTo_NewPartitions(BSP_Tree oldPartition, BSP_Tree partitionA, BSP_Tree partitionB, int CutX_B, int CutY_B, int CutX_A, int CutY_A)
        {
            for (int x = 0; x < oldPartition.Cells.GetLength(0); x++)
            {
                for (int y = 0; y < oldPartition.Cells.GetLength(1); y++)
                {
                    if (x < CutX_A && y < CutY_A)
                        partitionA.Cells[x, y] = oldPartition.Cells[x, y];
                    else
                        partitionB.Cells[x - CutX_B, y - CutY_B] = oldPartition.Cells[x, y];
                }
            }
        }

        /// <summary>
        /// A helper function to call the recursive generation of the BSP dungeon.
        /// </summary>
        public void Generate_Dungeon()
        {
            Create_LowestLayerRoom();
            Merge_LowestRoomLayers();
            Assign_WallTiles();
        }

        /// <summary>
        /// A helper function to call the room creation process in the lowest layer of the BSP (when both leafs are null).
        /// </summary>
        private void Create_LowestLayerRoom()
        {
            if (LeftLeaf == null && RightLeaf == null)//lowest layer, create Room here
                Assign_RoomFloorTiles();
            else//Check both children
            {
                LeftLeaf.Create_LowestLayerRoom();
                RightLeaf.Create_LowestLayerRoom();
            }
        }

        /// <summary>
        /// Creates a randomly sized room for this BSP within the BSP's size.
        /// </summary>
        private void Assign_RoomFloorTiles()
        {
            //Starting Point
            int startX = PCG_BSP.RNDG.Next(0, _partition.GetLength(0) - PCG_BSP.MinimumArea + 1);
            int startY = PCG_BSP.RNDG.Next(0, _partition.GetLength(1) - PCG_BSP.MinimumArea + 1);

            //Ending point: any random position from the starting point, but still greater than the minium area
            int endX = PCG_BSP.RNDG.Next(PCG_BSP.MinimumArea + startX, _partition.GetLength(0) + 1);
            int endY = PCG_BSP.RNDG.Next(PCG_BSP.MinimumArea + startY, _partition.GetLength(1) + 1);

            _room = new Room(new List<Cell>(), this, "Room");
            PCG_BSP.Rooms.Add(_room);
            //Iterate through the room
            for (int x = 0; x < endX - startX; x++)
            {
                for (int y = 0; y < endY - startY; y++)
                {
                    Cell currentCell = _partition[x + startX, y + startY];
                    _room.RoomCells.Add(currentCell);
                    currentCell.Cell_Type = Cell.CellType.Floor;
                }
            }
        }

        /// <summary>
        /// A helper function that calls the merging of the rooms of a Parent BSP's children at the lowest level (its children have rooms but it does not)
        /// </summary>
        private void Merge_LowestRoomLayers()
        {
            //find the parent who has no room, but has children with rooms
            if (_room == null &&
                LeftLeaf != null && LeftLeaf._room != null &&
                RightLeaf != null && RightLeaf._room != null)
                Set_CombinedRoom();
            else if (LeftLeaf != null && RightLeaf != null)
            {
                LeftLeaf.Merge_LowestRoomLayers();
                RightLeaf.Merge_LowestRoomLayers();
            }
            //Otherwise we've reached the bottom of the BSP Tree
        }

        /// <summary>
        /// Sets the Room of this BSP to a combined room consisting of this BSP's Children's rooms and a connecting corridor.
        /// </summary>
        private void Set_CombinedRoom()
        {
            if (LeftLeaf._room == null || RightLeaf._room == null || //no rooms to merge
               _room != null)//already merged
                return;

            Room combinedRoom = new Room(new List<Cell>(), this, "Combined Room");//This room is bubbled up through the tree
            Create_ConnectingCorridor(combinedRoom);

            //Merge the rooms
            foreach (var cell in LeftLeaf._room.RoomCells)
                combinedRoom.RoomCells.Add(cell);

            foreach (var cell in RightLeaf._room.RoomCells)
                combinedRoom.RoomCells.Add(cell);

            //Assign the room 
            _room = combinedRoom;

            if (Parent != null)
                Parent.Set_CombinedRoom();//Recurse up
        }

        /// <summary>
        /// Randomly creates a new corridor that connects this BSP's Children's Rooms and adds the created corridor into the combinedRoom of this BSP.
        /// </summary>
        /// <param name="combinedRoom">This BSP's room containing its children's rooms and the created corridor.</param>
        private void Create_ConnectingCorridor(Room combinedRoom)
        {
            //Create new room array
            Room corridorRoom = new Room(new List<Cell>(), this, "Corridor");//This corridor is added as a room in case we want to acess the corridor seperatly
            PCG_BSP.Rooms.Add(corridorRoom);

            //Randomly choose a point from each room
            Cell startCell = LeftLeaf._room.RoomCells[PCG_BSP.RNDG.Next(0, LeftLeaf._room.RoomCells.Count)];
            Cell endCell = RightLeaf._room.RoomCells[PCG_BSP.RNDG.Next(0, RightLeaf._room.RoomCells.Count)];

            int leftRoomPointX = startCell.X;
            int leftRoomPointY = startCell.Y;

            int rightRoomPointX = endCell.X;
            int rightRoomPointY = endCell.Y;

            //Figure out What direction the corridor needs to go
            int XDirection = rightRoomPointX - leftRoomPointX > 0 ? 1 : -1;
            int YDirection = rightRoomPointY - leftRoomPointY > 0 ? 1 : -1;

            //Keep weaving from point 1 until we reach point 2
            do
            {
                Cell currentCell = PCG_BSP.Grid[leftRoomPointX, leftRoomPointY];

                if ((leftRoomPointX - rightRoomPointX) != 0)//Go horizontal
                    leftRoomPointX += XDirection;
                if ((leftRoomPointY - rightRoomPointY) != 0)//Go vertical
                    leftRoomPointY += YDirection;

                //Set the start point for x,y to be upper bound from the middle
                int startOffset = ((PCG_BSP.MinimumCorridorArea - 1) / 2);

                //Add extra tiles above and beside, based on the opposite direction
                for (int x = -startOffset; x < PCG_BSP.MinimumCorridorArea - startOffset; x++)
                {
                    for (int y = -startOffset; y < PCG_BSP.MinimumCorridorArea - startOffset; y++)
                    {
                        if (leftRoomPointX + x < 0 || leftRoomPointY + y < 0 ||
                            leftRoomPointX + x >= PCG_BSP.Grid.GetLength(0) || leftRoomPointY + y >= PCG_BSP.Grid.GetLength(1))//out of bounds
                            continue;

                        Cell fillCell = PCG_BSP.Grid[leftRoomPointX + x, leftRoomPointY + y];

                        if (fillCell.Cell_Type == Cell.CellType.Corridor || fillCell.Cell_Type == Cell.CellType.Floor)//already placed
                            continue;

                        fillCell.Cell_Type = Cell.CellType.Corridor;
                        combinedRoom.RoomCells.Add(fillCell);
                        corridorRoom.RoomCells.Add(fillCell);
                    }
                }
            } while ((leftRoomPointX - rightRoomPointX) != 0 || (leftRoomPointY - rightRoomPointY) != 0);
        }

        /// <summary>
        /// Iterates through this BSP's Room's cells, changing their CellType to walls if they are adjacent to either the grid boundry or an empty cell.
        /// </summary>
        private void Assign_WallTiles()
        {
            foreach (var cell in _room.RoomCells)
            {
                //if any cell adjacent type is empty or its on the bounds, then make it a wall
                foreach (var adjacentCell in cell.AdjacentCells)
                    if (adjacentCell.Cell_Type == Cell.CellType.Empty ||
                        cell.X == 0 || cell.X == _partition.GetLength(0) - 1 ||
                        cell.Y == 0 || cell.Y == _partition.GetLength(1) - 1)
                        cell.Cell_Type = Cell.CellType.Wall;
            }

        }
    }
}



