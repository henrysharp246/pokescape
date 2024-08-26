using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokescapeServer
{
    public static class GameConfig
    {
        public const int GridWidth = 225; //length and width of the square map
        public const int GridSize = GridWidth*GridWidth; //length and width of the square map

        public const int MinRoomSize = 11; //min number of blocks in a room 
        public const int MaxRoomSize = 51; //max number of blocks in a room  

        public const int MaxRoomWidth = 20;
        public const int MinRoomWidth = 5;

        public const double MinWaterSizePcInRoom = 0;
        public const double MaxWaterSizePcInRoom = 0.4;
        
        public const int MinWaterBlocksInRoom = 0;
        public const int MaxWaterBlocksInRoom = 20;

        public const int MinRooms = 5; //IN USE
        public const int MaxRooms = 20;//IN USE


        public const int MaxItemsInRoom = 2;
        public const int MinItemsInRoom = 0;

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
