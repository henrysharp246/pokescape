﻿using Newtonsoft.Json;
using PokescapeServer;
using System;
using System.Collections.Generic;

public class Message
{
    public string MessageType { get; set; } // move_left, move_right, move_up, move_down, battle, save, load, grid
    public string Data { get; set; }
    public string SocketId { get; set; }
}
public class Battle
{
    public ScapeMonster UserScapeMonster { get; set; } 
    public ScapeMonster OpponentScapeMonster { get; set; }

}


public abstract class Item
{
    public Item(){
        this.ItemId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
    }
    public static Item GetRandomItem()
    {
        Random random = new Random();
        int num = random.Next(1, 120);
        switch (num)
        {
            case >118:
                return new BlueBerryItem(); 
                break;
            default:
                return new BlueBerryItem();


        }

    }
    public string Name { get; set; }
    public string ItemId { get; set; }
    public bool IsPortable { get; set; }

    public string Image { get; set; }   
    public bool IsInventory { get; set; }

    public virtual ScapeMonster UseItem(ScapeMonster monster) { return null; }
}

public class BlueBerryItem : Item
{
    public BlueBerryItem() {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Bluetwistblock.png";
        Name = "Blueberry";
    }
    public override ScapeMonster UseItem(ScapeMonster monster)
    {
        var newMonster = monster;
        newMonster.Health += 100;
        if (newMonster.Health > newMonster.MaximumHealth)
        {
            newMonster.Health = newMonster.MaximumHealth;
        }
        return newMonster;
    }
}


public class Block
{
    public string DefaultImage { get; set; }
    public bool ContainsItem { get; set; } = false;

    public Item item { get; set; }
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
    

    public StoneFloorBlock()
    {
        DefaultImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Stonefloorblock.png";
        Name = "stonefloorblock";
        CanPass = true;
        Random random = new Random();
        int num = random.Next(1,150);
        switch (num) 
        {

            case <15: Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Mossyfloorblock.png";
                break;
            case < 149:
                Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Stonefloorblock.png";
                break;
            default:
                var item = Item.GetRandomItem();
               this.item = item;
                this.Image = item.Image;
                this.ContainsItem = true;
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
        CanPass = false;
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
    public bool InBattle { get; set; }= false;
    public bool IsTurn { get; set; }= false;

    public string ItemSelectedId { get; set; }  
public Battle CurrentBattle { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public int UserGold { get; set; }
    public (int x, int y) UserCoordinates { get; set; } 
    public string UserImage { get; set; } = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingdownblock.png";
}

