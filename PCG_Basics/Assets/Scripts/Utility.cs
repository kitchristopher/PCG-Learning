using System.Collections;
using System.Collections.Generic;
using System;

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

