using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokescapeServer
{/**
    enum GridCoordinateType
    {
        OpenGroundStone,
        WallStone,
        CavePassage
    }**/
    public class GridCoordinate
    {
        public bool CanWalkThrough;
        public string ImageSource;
        public bool CanStorePokemon;
       
    }
    public class Wallstone: GridCoordinate
    {
        public Wallstone()
        {
            ImageSource = "../Image/blockImages/wallImage.jpg";
            CanStorePokemon = false;
            CanWalkThrough = false;
            //set imageSource here
        }
    }
    public class OpenGroundStone: GridCoordinate
    {
        
        public OpenGroundStone()
        {
            ImageSource = "../Image/blockImages/pathImage.png";
            CanStorePokemon = true;
            CanWalkThrough = true;
        }
    }
}
