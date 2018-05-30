using System.Collections;
using System.Collections.Generic;
using System;
using PCG;

namespace Utilities
{
    public static class Utility
    {
        public static System.Random RNDG = new System.Random();

        public static List<T> Copy_List<T>(this List<T> cloneList)
        {
            List<T> returnList = new List<T>();

            foreach (var item in cloneList)
                returnList.Add(item);

            return returnList;
        }

        /// <summary>
        /// Returns the number of cells in a list that match the given CellType and pass an input function.
        /// </summary>
        /// <param name="cellType"></param>
        /// <param name="adjacentCells"></param>
        /// <param name="filter">A function that takes a cell and returns a bool if that cell passes the function.</param>
        /// <returns></returns>
        public static int Get_CountOfCelltypeInRoom(Cell.CellType cellType, List<Cell> adjacentCells, Func<Cell, bool> filter)
        {
            int count = 0;

            foreach (var cell in adjacentCells)
            {
                if (cell.Cell_Type == cellType && filter(cell))
                {
                    ++count;
                }
            }

            return count;
        }

        /// <summary>
        /// Fills out an area from a centre point and places a new cell in the area. Existing corridors are added to the new fill area.
        /// </summary>
        /// <param name="startingCell">The centre point of the area.</param>
        /// <param name="cellsInRoom">Outputs all of the new cells into this List of Cells.</param>
        /// <param name="fillType">The type of cell to create the new cells as.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        public static void Fill_Room(Cell[,] grid, Cell startingCell, List<Cell> cellsInRoom, Cell.CellType fillType, int width, int height)
        {
            int offsetX, offsetY;
            Get_RoomCentreOffsets(grid, startingCell, width, height, out offsetX, out offsetY);

            for (int x = offsetX; x < offsetX + width; x++)
            {
                for (int y = offsetY; y < offsetY + height; y++)
                {
                    if (grid[x, y] == null)//only fill if the tile doesnt exist
                    {
                        Cell newCell = new Cell(x, y, grid);
                        newCell.Cell_Type = fillType;
                        cellsInRoom.Add(newCell);//add each filled cell to the room
                        grid[x, y] = newCell;
                    }
                    else if ((grid[x, y].Cell_Type == Cell.CellType.Rock || grid[x, y].Cell_Type == Cell.CellType.Corridor) && !cellsInRoom.Contains(grid[x, y]))//if its a corridor or a rock, then take it over
                    {
                        grid[x, y].Cell_Type = fillType;
                        grid[x, y].room.Remove_CellFromRoom(grid[x, y]);
                        cellsInRoom.Add(grid[x, y]);
                    }
                }
            }
        }

        /// <summary>
        /// Find the two closest points from one another between two rooms, and outputs the found cells.
        /// </summary>
        /// <param name="room1"></param>
        /// <param name="room2"></param>
        /// <param name="cell1">The cell in Room1 that is cloesest to the closest cell in Room2.</param>
        /// <param name="cell2">The cell in Room2 that is cloesest to the closest cell in Room1.</param>
        public static void Find_ClosestTwoPoints_FromRooms(Room room1, Room room2, out Cell cell1, out Cell cell2)
        {
            int closestDistance = int.MaxValue;
            Cell room1Closest = null;
            Cell room2Closest = null;

            //Iterate through all of the two rooms and compare their cells to find the closest pair
            foreach (var room1Cell in room1.BoundryCells)
            {
                foreach (var room2Cell in room2.BoundryCells)
                {
                    int currentDistance = Math.Abs(room1Cell.X - room2Cell.X) + Math.Abs(room1Cell.Y - room2Cell.Y);

                    if (currentDistance < closestDistance)
                    {
                        room1Closest = room1Cell;
                        room2Closest = room2Cell;

                        closestDistance = currentDistance;
                    }
                }
            }

            cell1 = room1Closest;
            cell2 = room2Closest;
        }

        /// <summary>
        /// Given a cell and the width and height for an area, this functions outputs the X and Y offsets of the starting boundries from the left and bottom 
        /// such that the startingCell is in the centre.
        /// </summary>
        /// <param name="startingCell">The centre of the offsets.</param>
        /// <param name="width">Width of the room.</param>
        /// <param name="height">Height of the room.</param>
        /// <param name="offsetX">The X offset to return.</param>
        /// <param name="offsetY">The Y offset to return.</param>
        public static void Get_RoomCentreOffsets(Cell[,] grid, Cell startingCell, int width, int height, out int offsetX, out int offsetY)
        {
            //Get the mid point from the starting cell to fill evenly around it
            int roundedHalfWidth = (width / 2);
            int roundedHalfHeight = (height / 2);

            //the lowest index so we don't have negative values; just for readability
            int lowerRange = 0;

            //Take into consideration the case where the cell is right ON the boundryline and we need to build further inward
            int upperRangeX = System.Math.Min((grid.GetLength(0) - width), (startingCell.X - roundedHalfWidth));
            int upperRangeY = System.Math.Min((grid.GetLength(1) - height), (startingCell.Y - roundedHalfHeight));

            //Find the proper offset values to leave enough room to ensure that the proper area is filled in
            offsetX = System.Math.Max(lowerRange, upperRangeX);
            offsetY = System.Math.Max(lowerRange, upperRangeY);
        }
    }

    public static class PCG_Exceptions
    { /// <summary>
      /// Check that proper values have been used: No negative sizes, or a minimum room size larger than the dungeon.
      /// </summary>
      /// <param name="width"></param>
      /// <param name="height"></param>
      /// <param name="minArea"></param>
      /// <param name="minCorridorArea"></param>
        public static bool Is_ValidInput_Exception(int width, int height, int minArea, int minCorridorArea)
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
    }

}

