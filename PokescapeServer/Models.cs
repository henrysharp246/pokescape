using Newtonsoft.Json;
using PokescapeServer;
using System;
using System.Collections.Generic;

public class Message
{
    public string MessageType { get; set; } // move_left, move_right, move_up, move_down, battle, save, load, grid
    public string Data { get; set; }
    public string SocketId { get; set; }
}

public abstract class Item
{
    public string ItemId { get; set; }
    public bool IsPortable { get; set; }
    public bool IsInventory { get; set; }

    public virtual void UseItem() { }
}

public class ScrollItem : Item
{
    public string Text { get; set; }

    public override void UseItem()
    {
        Console.WriteLine(Text);
    }
}


public class Block
{
    public bool HasUser { get; set; } = false;  
    public string BlockId { get; set; }
    public string Name { get; set; }
    public bool CanPass { get; set; }
    public bool CanSpawn { get; set; }
    public string Image { get; set; }
    public int RoomIndex { get; set; }

    
}

public class FloorBlock : Block
{
    public FloorBlock()
    {
        CanPass = true;
    }
}

public class StoneFloorBlock : FloorBlock
{
    public bool ContainsItem { get; set; }

    public StoneFloorBlock()
    {
        Name = "stonefloorblock";
        CanPass = true;
        Random random = new Random();
        int num = random.Next(1,5);
        switch (num) 
        {

            case 1: Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Mossyfloorblock.png";
                break;
           /* case 2: Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Mossyfloorblock.png";
                break;*/
            default: Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Stonefloorblock.png";
                break;


        }



     
    }
}

public class BlankBlock : Block
{
    public BlankBlock()
    {
        Name = "blank";
        CanPass = false;
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Blankblock.png";
    }
}

public class WallBlock : Block
{
    public WallBlock()
    {
        Name = "wallblock";
        CanPass = false;
    }
}

public class StoneWallBlock : WallBlock
{
    public StoneWallBlock()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Wallblock.png";
        Name = "stonewallblock";
    }
}

public class WaterBlock : FloorBlock
{
    public WaterBlock()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Waterblock.png";
    }
}

public class Entrance : WallBlock
{
    public int EntranceId { get; set; }
    public int CorrespondingRoomId { get; set; } = -1;
    public Entrance()
    {
        CanPass = true;
    }
}

public class TopEntrance : Entrance
{
  
  
    public TopEntrance()
    {
      
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Topentrance.png";
    }
}

public class BottomEntrance : Entrance
{
   

    public BottomEntrance()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Bottomentrance.png";
    }
}

public class RightEntrance : Entrance
{
  

    public RightEntrance()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Rightentrance.png";
    }
}

public class LeftEntrance : Entrance
{
    

    public LeftEntrance()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Leftentrance.png";
    }
}

public class User
{
    public List<Item> Inventory { get; set; } = new List<Item>();
    public List<ScapeMonster> ScapeMonsters = new List<ScapeMonster>();

    public User(){
        ScapeMonsters.Add(new Fuzzy());
    }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public int UserGold { get; set; }
    public (int x, int y) UserCoordinates { get; set; } 
    public string UserImage { get; set; } = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingdownblock.png";
}

