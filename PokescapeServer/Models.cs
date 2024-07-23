using Newtonsoft.Json;
using PokescapeServer;
using System.Drawing;

public class Message
{
    public string MessageType { get; set; } // move_left, Moveright, Moveup, Movedown, battle, save, load, grid,
    public string Data { get; set; } //
    public string SocketId { get; set; }                          // public string Data { get; set; } // 
}
public class Game
{

    public enum GameModeType
    {
        Hard,
        Easy
    }
    public int gridSize = 121;
    public int virtualGridSize = 21;
    private string GameId;
    private string gameState;
    private GameModeType gameMode;
    private string seed;
    private User user;
    private Dictionary<(int x, int y), Block> grid;

    public Game()
    {
        user = new User();

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns>MESSAGES TO SEND</returns>
    public List<Message> HandleMessage(Message message)
    {
        var messagesToSend = new List<Message>();
        var gridMessage = new Message();
        message.MessageType = "grid";
        message.Data = JsonConvert.SerializeObject(grid);

        var playerMessage = new Message();
        message.MessageType = "player";
        message.Data = JsonConvert.SerializeObject(grid);

        switch (message.MessageType)
        {
            case "MOVE_UP":
                MoveUp();
                messagesToSend.Add(gridMessage);
                messagesToSend.Add(playerMessage);
                break;
            case "MOVE_DOWN":
                MoveDown();
                messagesToSend.Add(gridMessage);
                messagesToSend.Add(playerMessage);
                break;
            case "MOVE_LEFT":
                MoveLeft();
                messagesToSend.Add(gridMessage);
                messagesToSend.Add(playerMessage);
                break;
            case "MOVE_RIGHT":
                MoveRight();
                messagesToSend.Add(gridMessage);
                messagesToSend.Add(playerMessage);
                break;


        }
        Console.WriteLine(message.MessageType, message.Data);
        return messagesToSend;

    }

    public Dictionary<(int x, int y), Block> CreateGrid()
    {
        Dictionary<(int x, int y), Block> grid = new();
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Block block = new StoneBlock();
                block.SetCanPass(true);
                block.SetName("grass");
                grid.Add((x, y), block);

            }
        }
        this.grid = grid;
        return grid;

    }





    public static void LogGrid(Block[][] grid)
    {
        for (int x = 0; x < grid.Length; x++)
        {
            for (int y = 0; y < grid.Length; y++)
            {
                Console.WriteLine(grid[x][y].GetName);
            }
            Console.WriteLine("\n");
        }
    }

    public void MoveUp()
    {
        var coords = user.GetUserCoordinates();
        coords.y += 1;
        user.SetUserCoordinates(coords);
        user.userImage = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Characterfacingupblock.png";
    }
    public void MoveDown()
    {
        var coords = user.GetUserCoordinates();
        coords.y -= 1;
        user.SetUserCoordinates(coords);
        user.userImage = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Characterfacingdownblock.png";

    }
    public void MoveLeft()
    {
        var coords = user.GetUserCoordinates();
        coords.x -= 1;
        user.SetUserCoordinates(coords);
        user.userImage = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Characterfacingleftblock.png";

    }
    public void MoveRight()
    {
        var coords = user.GetUserCoordinates();
        coords.x += 1;
        user.SetUserCoordinates(coords);
        user.userImage = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Characterfacingrightblock.png";
    }
    public GameModeType GetGameMode() { return gameMode; }
    public void SetGameMode(GameModeType value) { gameMode = value; }

    public string GetSeed() { return seed; }
    public void SetSeed(string value) { seed = value; }

    public User GetUser() { return user; }
    public void SetUser(User value) { user = value; }
    public string GetGameState()
    {
        return gameState; 
    }
   
    private Dictionary<(int x, int y), Block> LoadGameFromFile()
    {
        return null;
    }

    private Dictionary<(int x, int y), Block> GetGridFromFile()
    {
        return grid;
    }
    public Dictionary<(int x, int y), Block> GetVisibleGrid()
    {
        Dictionary<(int x, int y), Block> visibleGrid = new();
        var coords = user.GetUserCoordinates();
        int xCount = 0; int yCount = 0;
        for (int x = coords.x - virtualGridSize / 2; x<= coords.x+ virtualGridSize / 2; x++)
        {
            for (int y = coords.y - virtualGridSize / 2; y <= coords.y + virtualGridSize / 2; y++)
            {
                visibleGrid[(xCount, yCount)] = grid[(x, y)];
                yCount++;
            }
            xCount++;
        } 
        return visibleGrid;
    }
    public Dictionary<(int x, int y), Block> GetGrid() { return grid; }
    public void SetGrid(Dictionary<(int x, int y), Block> value) { grid = value; }
}

public abstract class Item
{
    private string itemId;
    private bool isPortable;
    private bool isInventory;

    public string GetItemId() { return itemId; }
    public void SetItemId(string value) { itemId = value; }

    public bool GetIsPortable() { return isPortable; }
    public void SetIsPortable(bool value) { isPortable = value; }

    public bool GetIsInventory() { return isInventory; }
    public void SetIsInventory(bool value) { isInventory = value; }

    public virtual void UseItem() { }
}

public class Scrollitem : Item
{
    private string text;

    public string GetText() { return text; }
    public void SetText(string value) { text = value; }

    public override void UseItem()
    {
        Console.WriteLine(text);
    }
}

public class SerializableBlock
{
    public string blockId;
    public string name;
    public bool canPass;
    public bool canSpawn;
    public string image; 
}

    public class Block
{
    private string blockId;
    private string name;
    private bool canPass;
    private bool canSpawn;
    public string image;
    private int roomIndex;

    public string Serialize()
    {
        SerializableBlock block = new()
        {
            blockId = this.blockId,
            name = this.name,
            canPass = this.canPass,
            canSpawn = this.canSpawn,
            image = this.image,
        };
        return JsonConvert.SerializeObject(block);
    }
    public string GetBlockId() { return blockId; }
    public void SetBlockId(string value) { blockId = value; }
    //Set Image
    public void SetRoomIndex(int value) { roomIndex = value; } 
    public int GetRoomIndex() { return roomIndex; }
    public string GetName() { return name; }
    public void SetName(string value) { name = value; }

    public bool GetCanPass() { return canPass; }
    public void SetCanPass(bool value) { canPass = value; }

    public bool GetCanSpawn() { return canSpawn; }
    public void SetCanSpawn(bool value) { canSpawn = value; }
}

public class FloorBlock : Block // INHERITANCE
{
    private bool canPass = true;
    
    public new bool GetCanPass() { return canPass; }
    public new void SetCanPass(bool value) { canPass = value; }
}

public class StoneBlock : FloorBlock
{
    private bool containsItem;
    public StoneBlock() {

        image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Stonefloorblock.png";

    } 

    public bool getContainsItem() { return containsItem; }


}




public class Blank : Block
{
    private bool canPass = false;

    public new bool GetCanPass() { return canPass; }

    public Blank()
    {
        image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Blankblock.png";

    }

}


public class WallBlock : Block
{
    private bool canPass;

    public new bool GetCanPass() { return canPass; }
    public new void SetCanPass(bool value) { canPass = value; }

}

public class StoneWallBlock : WallBlock 
{
    private bool canPass = false;

    public new bool GetCanPass() { return canPass; }

    StoneWallBlock()
    {
        image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Wallblock.png";
    }

}

public class WaterBlock : FloorBlock
{
    // Can pass if user has correct scapemonster
    private bool canPass = true; //for now
    WaterBlock()
    {
        image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Waterblock.png";
        
    }
       
}


public class Entrance : WallBlock
{
    private bool canPass = true;

    public new bool GetCanPass() { return canPass; }
    


}

public class TopEntrance : Entrance
{
    private bool canPass = true;

    public new bool GetCanPass() { return canPass; }

    TopEntrance()
    {
        image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Topentrance.png";
    }
    private int correspondingBottomEntrance;
    public int getCorrespondingBottomEntrance()
    {
        return correspondingBottomEntrance;
    }

    public void setCorrespondingBottomEntrance(int value)
    {
        correspondingBottomEntrance = value;
    }

}

public class BottomEntrance : Entrance
{
    private bool canPass = true;

    public new bool GetCanPass() { return canPass; }

    BottomEntrance()
    {
        image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Bottomentrance.png";
    }
    private int correspondingTopEntrance;
    public int getCorrespondingTopEntrance()
    {
        return correspondingTopEntrance;
    }

    public void setCorrespondingTopEntrance(int value)
    {
        correspondingTopEntrance = value;
    }

}

public class RightEntrance : Entrance
{
    private bool canPass = true;

    public new bool GetCanPass() { return canPass; }

    RightEntrance()
    {
        image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Rightentrance.png";
    }
    private int correspondingLeftEntrance;
    public int getCorrespondingLeftEntrance()
    {
        return correspondingLeftEntrance;
    }

    public void setCorrespondingLeftEntrance(int value)
    {
        correspondingLeftEntrance = value;
    }

}

public class LeftEntrance : Entrance
{
    private bool canPass = true;
   
    public new bool GetCanPass() { return canPass; }

    LeftEntrance()
    {
        image = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Leftentrance.png";
    }
    private int correspondingRightEntrance;
    public int getCorrespondingRightEntrance()
    {
        return correspondingRightEntrance;
    }

    public void setCorrespondingRightEntrance(int value)
    {
        correspondingRightEntrance = value;
    }
}


public class User
{
    private List<Item> inventory;
    private string userId;
    private string userName;
    private string password;
    private int usergold;
    private (int x, int y) userCoordinates;
    public string userImage = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Characterfacingdownblock.png";

    public User()
    {
        SetUserCoordinates((60, 60));
    }
    public List<Item> GetInventory() { return inventory; }
    public void SetInventory(List<Item> value) { inventory = value; }

    public string GetUserId() { return userId; }
    public void SetUserId(string value) { userId = value; }

    public string GetUserName() { return userName; }
    public void SetUserName(string value) { userName = value; }

    public string GetPassword() { return password; }
    public void SetPassword(string value) { password = value; }


    public int GetUsergold() { return usergold; }
    public void SetUsergold(int value) { usergold = value; }

    public (int x, int y) GetUserCoordinates() { return userCoordinates; }
    public void SetUserCoordinates((int x, int y) value) { userCoordinates = value; }

  



}


public class ScapeMonster
{
    public class ScapeMonsterMove //COMPOSITION FILLED
    {
        private int moveDamage;
        private string moveName;
        private Type movetype;

        public int GetMoveDamage() { return moveDamage; }
        public void SetMoveDamage(int value) { moveDamage = value; }

        public string GetMoveName() { return moveName; }
        public void SetMoveName(string value) { moveName = value; }
    }

    private string scapeMonsterID;

    private (int xCord, int yCord) scapemonsterCoordinates;
    private string scapeMonsterName;
    private string scapeMonsterId;

    private double maximumHealth;
    private bool isBoss;
    private double health;
    private double damagePerHit;
    private List<ScapeMonsterMove> moves;
    private int level;
    

    public string GetScapeMonsterID() { return scapeMonsterID; }
    public void SetScapeMonsterID(string value) { scapeMonsterID = value; }

    public (int xCord, int yCord) GetScapemonsterCoordinates() { return scapemonsterCoordinates; }
    public void SetScapemonsterCoordinates((int xCord, int yCord) value) { scapemonsterCoordinates = value; }

    public string GetScapeMonsterName() { return scapeMonsterName; }
    public void SetScapeMonsterName(string value) { scapeMonsterName = value; }

   


    public double GetMaximumHealth() { return maximumHealth; }
    public void SetMaximumHealth(double value) { maximumHealth = value; }

    public bool GetIsBoss() { return isBoss; }
    public void SetIsBoss(bool value) { isBoss = value; }

    public double GetHealth() { return health; }
    public void SetHealth(double value) { health = value; }

    public double GetDamagePerHit() { return damagePerHit; }
    public void SetDamagePerHit(double value) { damagePerHit = value; }

    public List<ScapeMonsterMove> GetMoves() { return moves; }
    public void SetMoves(List<ScapeMonsterMove> value) { moves = value; }

    public int GetLevel() { return level; }
    public void SetLevel(int value) { level = value; }

    public ScapeMonster() { }
}

