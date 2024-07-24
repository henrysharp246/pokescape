﻿using Newtonsoft.Json;
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

public class SerializableBlock
{
    public string BlockId { get; set; }
    public string Name { get; set; }
    public bool CanPass { get; set; }
    public bool CanSpawn { get; set; }
    public string Image { get; set; }
}

public class Block
{
    public string BlockId { get; set; }
    public string Name { get; set; }
    public bool CanPass { get; set; }
    public bool CanSpawn { get; set; }
    public string Image { get; set; }
    public int RoomIndex { get; set; }

    public string Serialize()
    {
        var block = new SerializableBlock
        {
            BlockId = this.BlockId,
            Name = this.Name,
            CanPass = this.CanPass,
            CanSpawn = this.CanSpawn,
            Image = this.Image,
        };
        return JsonConvert.SerializeObject(block);
    }
}

public class FloorBlock : Block
{
    public FloorBlock()
    {
        CanPass = true;
    }
}

public class StoneBlock : FloorBlock
{
    public bool ContainsItem { get; set; }

    public StoneBlock()
    {
        Image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Stonefloorblock.png";
    }
}

public class Blank : Block
{
    public Blank()
    {
        CanPass = false;
        Image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Blankblock.png";
    }
}

public class WallBlock : Block
{
    public WallBlock()
    {
        CanPass = false;
    }
}

public class StoneWallBlock : WallBlock
{
    public StoneWallBlock()
    {
        Image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Wallblock.png";
    }
}

public class WaterBlock : FloorBlock
{
    public WaterBlock()
    {
        Image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Waterblock.png";
    }
}

public class Entrance : WallBlock
{
    public Entrance()
    {
        CanPass = true;
    }
}

public class TopEntrance : Entrance
{
    public int CorrespondingBottomEntrance { get; set; }

    public TopEntrance()
    {
        Image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Topentrance.png";
    }
}

public class BottomEntrance : Entrance
{
    public int CorrespondingTopEntrance { get; set; }

    public BottomEntrance()
    {
        Image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Bottomentrance.png";
    }
}

public class RightEntrance : Entrance
{
    public int CorrespondingLeftEntrance { get; set; }

    public RightEntrance()
    {
        Image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Rightentrance.png";
    }
}

public class LeftEntrance : Entrance
{
    public int CorrespondingRightEntrance { get; set; }

    public LeftEntrance()
    {
        Image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Leftentrance.png";
    }
}

public class User
{
    public List<Item> Inventory { get; set; } = new List<Item>();
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public int UserGold { get; set; }
    public (int x, int y) UserCoordinates { get; set; } = (60, 60);
    public string UserImage { get; set; } = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Characterfacingdownblock.png";
}

public class ScapeMonster
{
    public class ScapeMonsterMove
    {
        public int MoveDamage { get; set; }
        public string MoveName { get; set; }
        public Type MoveType { get; set; }
    }

    public string ScapeMonsterID { get; set; }
    public (int xCord, int yCord) ScapemonsterCoordinates { get; set; }
    public string ScapeMonsterName { get; set; }
    public double MaximumHealth { get; set; }
    public bool IsBoss { get; set; }
    public double Health { get; set; }
    public double DamagePerHit { get; set; }
    public List<ScapeMonsterMove> Moves { get; set; } = new List<ScapeMonsterMove>();
    public int Level { get; set; }
}
