using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokescapeServer
{
    public static class GameConfig
    {
        public const int VisibleGridWidth = 50; //length and width of the square map
        public const int VisibleGridSize = VisibleGridWidth*VisibleGridWidth; //length and width of the square map


        public const int MaxRoomWidth = VisibleGridWidth;
        public const int MinRoomWidth = 30;

        public const int MinPondWidth = 2; //min number of blocks in a room 
        public const int MaxPondWidth = 15;

       

        public const int MinDecompositionOfCorners = 2; //THIS MUST BE LESS THAN HALF OF THE MINIMUM ROOM WIDTH

        public const int MinDecompositionOfPondCorners = 0;

        public const double ProbabilityOfWater =1;


        public const int MinRooms = 4; //IN USE MUST BE GREATER THAN 1
        public const int MaxRooms = 6;//IN USE

        public const double ProbabilityOfScapemonster = 0.1;
        

        public const int MaxItemsInRoom = 2;
        public const int MinItemsInRoom = 10;

        public const int MaxDoorsInRoom = 3;
        public const int MinDoorsInRoom = 1;


        /** RULES
         * 
         * CHARCTER MUST SPAWN IN A ROOM
         * 
         *  ROOM MUST BE SURROUNDED BY WALL BLOCKS
         * 
         * ROOM MUST BE FILLED WITH FLOOR BLOCKS
         * 
         * YOU CAN ONLY SEE IN ONE ROOM AT A TIME
         * 
         * 
         * 
         **/


    }
}
