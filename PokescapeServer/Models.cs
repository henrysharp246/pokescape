using Newtonsoft.Json;
using PokescapeServer;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

public class Coordinate
{
    public int x;
    public int y;
}
public class Message
{
    public string MessageType { get; set; } // move_left, move_right, move_up, move_down, battle, save, load, grid
    public string Data { get; set; }
    public string SocketId { get; set; }
}
public class Battle
{


    public Dictionary<ScapeMonsterMove, int> UserCooldowns;
    public Dictionary<ScapeMonsterMove, int> OpponentCooldowns;
    public ScapeMonster UserScapeMonster { get; set; } 
    public ScapeMonster OpponentScapeMonster { get; set; }

    public List<ScapeMonsterMove> ActiveUserMoves = new List<ScapeMonsterMove>();

    public List<ScapeMonsterMove> ActiveOpponentMoves = new List<ScapeMonsterMove>();
}


public abstract class Item
{
    public int index;

    private static long _uniqueItemId = 0; 

    public Item()
    {
        this.ItemId = $"ITEM_{++_uniqueItemId}"; // 
    }

    public Item ContainedItem { get; set; }


    public static Item GetItemFromChest()
    {
        Random random = new Random();
        int num2 = random.Next(1, 6);
        switch (num2)
        {
            case 1:
                return new ResistanceCharm();
            case 2:
                return new HystericalPotion();
            case 3:
                return new UltraCharm();
            case 4:
                return new CombatCharm();
            case 5:
                return new MysticOrb();
            default:
                return new CombatCharm();
        }
        
    }

    public static Item ChestOrKey()
    {
        if (Game.ChestIndex <= Game.KeyIndex)
        {
            // Generate a chest if counts are equal
            Game.ChestIndex++;
            return new ChestClosed
            {
                index = Game.ChestIndex
            };

        }
        else
        {
            // Generate a key if there are more chests than keys
            Game.KeyIndex++;
            return new Key
            {
                index = Game.KeyIndex
            };

        }
    }
    public static Item GetRandomItem()
    {
      
        Random random = new Random();
        int num = random.Next(1, 100);

        if (num < 95)
        {
            int num1 = random.Next(1, 100);  
            if (num1 < 40)
                return new SpikeBerry();
            else if (num1 < 70)
                return new OrangeGrape();
            else if (num1 < 85)
                return new GuavaBerry();
            else
                return new BlueTwistItem();
        }
        else if (num < 101)
        {
            int num2 = random.Next(1, 6);
            switch (num2)
            {
                case 1:
                    return new ResistanceCharm();
                case 2:
                    return new HystericalPotion();
                case 3:
                    return new UltraCharm();
                case 4:
                    return new CombatCharm();
                case 5:
                    return new MysticOrb();
                default:
                    return new FortificationCharm();
            }
        }
        else // Chest or key 
        {

            Item item = ChestOrKey();
            return item;
        }

       
        
        
    }



    public string Name { get; set; }
    public string ItemId { get; set; }
    public bool IsPortable { get; set; }

    public string Image { get; set; }   
   
    public virtual ScapeMonster UseItem(ScapeMonster monster) { return null; }
}

public abstract class BerryItem : Item
{ 
    



}


public class SpikeBerry : BerryItem
{
    public SpikeBerry()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Spikeberryblock.png";
        Name = "Spike Berry";
    }
    public override ScapeMonster UseItem(ScapeMonster monster)
    {
        if (monster.Health == monster.MaximumHealth)
        {
            return null;
        }    

        var newMonster = monster;
        newMonster.Health += 0.15*newMonster.MaximumHealth;
        if (newMonster.Health > newMonster.MaximumHealth)
        {
            newMonster.Health = newMonster.MaximumHealth;
        }
        return newMonster;
    }

}



public class OrangeGrape : BerryItem
{
    public OrangeGrape()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Orangegrapeblock.png";
        Name = "Orange Grape";
    }
    public override ScapeMonster UseItem(ScapeMonster monster)
    {
        if (monster.Health == monster.MaximumHealth)
        {
            return null;
        }

        var newMonster = monster;
        newMonster.Health += 0.3 * newMonster.MaximumHealth;
        if (newMonster.Health > newMonster.MaximumHealth)
        {
            newMonster.Health = newMonster.MaximumHealth;
        }
        return newMonster;
    }
}

public class GuavaBerry : BerryItem
{
    public GuavaBerry()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Guavaberryblock.png";
        Name = "Guava Berry";
    }
    public override ScapeMonster UseItem(ScapeMonster monster)
    {
        if (monster.Health == monster.MaximumHealth)
        {
            return null;
        }

        var newMonster = monster;
        newMonster.Health += 0.45 * newMonster.MaximumHealth;
        if (newMonster.Health > newMonster.MaximumHealth)
        {
            newMonster.Health = newMonster.MaximumHealth;
        }
        return newMonster;
    }
}

public class BlueTwistItem : BerryItem
{
    public BlueTwistItem() {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Bluetwistblock.png";
        Name = "Blue Twist";
    }
    public override ScapeMonster UseItem(ScapeMonster monster)
    {
        if (monster.Health == monster.MaximumHealth)
        {
            return null;
        }

        var newMonster = monster;
        newMonster.Health += 0.6*  newMonster.MaximumHealth;
        if (newMonster.Health > newMonster.MaximumHealth)
        {
            newMonster.Health = newMonster.MaximumHealth;
        }
        return newMonster;
    }
}



public class MysticOrb : Item
{
    public MysticOrb()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Mysticorbcharmblock.png";
        Name = "Mystic Orb";
    }
    public override ScapeMonster UseItem(ScapeMonster monster)
    {
        var newMonster = monster;
      
        ScapeMonsterMove move = monster.Moves.FirstOrDefault(x => x.MoveName == "Epic drain");
        
        if (newMonster.Moves.Count <4 && move == null)
        {
            newMonster.Moves.Add(new Epicdrain());
        }
        else
        {
            return null;
        }
        return newMonster;
    }
}


public class ResistanceCharm : Item
{
    public ResistanceCharm()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Resistancecharmblock.png";
        Name = "Resistance Charm";
    }
    public override ScapeMonster UseItem(ScapeMonster monster)
    {
        var newMonster = monster;
        ScapeMonsterMove move = monster.Moves.FirstOrDefault(x => x.MoveName == "Resist");
        if (newMonster.Moves.Count < 4 && move == null)
        {
            newMonster.Moves.Add(new Resist());
        }
        else
        {
            return null;
        }
        return newMonster;
    }
}


public class UltraCharm : Item
{
    public UltraCharm()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Ultracharmblock.png";
        Name = "Ultra Charm";
    }

    public override ScapeMonster UseItem(ScapeMonster monster)
    {
        var newMonster = monster;
        ScapeMonsterMove move = monster.Moves.FirstOrDefault(x => x.MoveName == "Ultra Ray");// here and below:
        if (newMonster.Moves.Count < 4 && move == null) //checking that the move isnt already present in the list 
        {
            newMonster.Moves.Add(new UltraRay());
        }
        else
        {
            return null;
        }
        return newMonster;
    }
}

public class HystericalPotion : Item
{
    public HystericalPotion()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Hystericalpotioncharmblock.png";
        Name = "Hysterical Potion";
    }
    public override ScapeMonster UseItem(ScapeMonster monster)
    {
        var newMonster = monster;
        ScapeMonsterMove move = monster.Moves.FirstOrDefault(x => x.MoveName == "Hysteria");

        if (newMonster.Moves.Count < 4 && move == null )
        {
            newMonster.Moves.Add(new Hysteria());
        }
        else
        {
            return null;
        }
        return newMonster;
    }
}

public class CombatCharm : Item
{
    public CombatCharm()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Combatcharmblock.png";
        Name = "Combat Charm";
    }
    public override ScapeMonster UseItem(ScapeMonster monster)
    {
        var newMonster = monster;
        newMonster.BaseDamage += 0.1;
        return newMonster;
    }
}



public class FortificationCharm : Item
{
    public FortificationCharm()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Fortificationblock.png";
        Name = "Fortification Charm ";
    }
    public override ScapeMonster UseItem(ScapeMonster monster)
    {
        var newMonster = monster;
        newMonster.BaseDefence += 0.1;
        return newMonster;
    }
}




public class ChestClosed : Item
{
    public ChestClosed()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Chestblock.png";
        Name = "A locked chest";
        Random random = new Random();
        int num2 = random.Next(1, 6);
        switch (num2)
        {
            case 1:
                ContainedItem = new ResistanceCharm();
                break;
            case 2:
                ContainedItem = new HystericalPotion();
                break;
            case 3:
                ContainedItem = new UltraCharm();
                break;
            case 4:
                ContainedItem = new CombatCharm();
                break;
            case 5:
                ContainedItem = new MysticOrb();
                break;
            case 6:
                ContainedItem = new FortificationCharm();
                break;
            default:
                ContainedItem = new HystericalPotion();
                break;
        }

    }
    public Item ContainedItem;


}



public class Key : Item
{
    public Key()
    {
        Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Keyblock.png";
        Name = "key";
    }
  
}



public abstract class Block
{
    public string DefaultImage { get; set; }
    public bool ContainsItem { get; set; } = false;

    public Item item { get; set; }
    public bool HasUser { get; set; } = false;  
    
    public string Name { get; set; }
    public bool CanPass { get; set; }
   
    public string Image { get; set; }
    

    
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
        int num = random.Next(1, 50);

        
        
        switch (num)
        {

            case < 4:
                Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Mossyfloorblock.png";
                break;
            case < 49:
                Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Stonefloorblock.png";
                break;
            default:
                var item = Item.GetRandomItem();
                this.item = item;
                this.Image = item.Image;
                this.ContainsItem = true;
                if (this.item is ChestClosed)
                {
                    this.CanPass = false;
                }
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

public abstract class WallBlock : Block
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
 
        DefaultImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Waterblock.png";
        Name = "waterblock";
        CanPass = false;
        Random random = new Random();
        int num = random.Next(1, 150);
        switch (num)
        {

            case < 25:
                Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Waterblock.png";
                break;
            case < 50:
                Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Waterblock1.png";
                break;
            case < 75:
                Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Waterblock2.png";
                break;
            case < 100:
                Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Waterblock3.png";
                break;

            case < 125:
                Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Waterblock4.png";
                break;
            case < 150:
                Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Waterblock5.png";
                break;
                
        }
    }
}

public class Entrance : WallBlock
{
    //public int EntranceId { get; set; }
    [JsonIgnore]   // This decoration is necessary to prevent JSON serialization problems due to recursive references (objects containing references to each other would cause infinite loop when serializing)
    public Entrance CorrespondingEntrance = null;
    public int RoomId { get; set; } = -1;

    public (int x, int y) Coordinates;

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
    Dictionary<string, Item> _Inventory = new();
    public List<Item> Inventory
    {
        get
        {
            return _Inventory.Values.ToList();
        }
    }

    public List<ScapeMonster> ScapeMonsters = new List<ScapeMonster>();
    public bool CanExit = true;
    public bool ExitAttempt = false;
    
    
    public User(){

        
       

        ScapeMonsters.Add(new Fuzzy(3));
   


    }

    public void AddItemToInventory(Item item)
    {
        _Inventory.Add(item.ItemId, item);
    }

    public void RemoveItemFromInventory(string itemId) 
    { 
        _Inventory.Remove(itemId);
    }

    public bool TryGetValueFromInventory(string itemId, out Item item)
    {
        return _Inventory.TryGetValue(itemId, out item);
    }

    public void DiscardScapeMonster()
    {
        if (ScapeMonsters.Count > 1)
        {
            foreach (ScapeMonster scapemonster in ScapeMonsters)
            {
                if (scapemonster.ScapeMonsterID == MonsterSelectedId)
                {
                    ScapeMonsters.Remove(scapemonster);
                    return;
                }

            }
        }
    }
        

    public void RemoveMove( string moveID)
    {
        foreach (ScapeMonster scapeMonster in ScapeMonsters)
        {
            foreach (ScapeMonsterMove move in scapeMonster.Moves)
            {
                if (move.Id == moveID)
                {

                    scapeMonster.Moves.Remove(move);
                    return;
                }
            }
        }

        
    }
    public void RenameScapeMonster(string ScapeMonsterId, string newName)
    {
        foreach (ScapeMonster monster in ScapeMonsters) 
        { 
        if (monster.ScapeMonsterID == ScapeMonsterId)
            {
                monster.ScapeMonsterName = newName;
                return;
            }
        
        }
    }


    public bool CanPassWater()
    {
        foreach (ScapeMonster scapeMonster in ScapeMonsters)
        {

            if (scapeMonster is Sealy)
            {
                return true;
            }

        }
        return false;
    }

    public bool InBattle { get; set; }= false;
    public bool IsTurn { get; set; }= false;

    public string ItemSelectedId { get; set; }
    public string MonsterSelectedId { get; set; }
    public Battle CurrentBattle { get; set; }
    public string UserId { get; set; }
    public int BattleCount { get; set; } = 0;
    public int GetUsersHighestLevelScapemonster()
    {
      
        int HighestLevel = 0;
        foreach (ScapeMonster ScapeMonster in ScapeMonsters)
        {
          
            int CurrentLevel = ScapeMonster.Level;
            if (CurrentLevel > HighestLevel )
            {
                HighestLevel = CurrentLevel;

            }

        }
        return HighestLevel;
                
    }
        
    public string UserName { get; set; }
    public string Password { get; set; }
    public int UserGold { get; set; }
    public (int x, int y) UserCoordinates { get; set; } 
    public string UserImage { get; set; } = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingdownblock.png";
}

