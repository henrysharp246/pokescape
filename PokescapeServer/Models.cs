public class Message
{
    public string MessageType { get; set; } // move_left, Moveright, Moveup, Movedown, battle, save, load, grid,
    public string Data { get; set; } //
    public string SocketId { get; set; }                          // public string Data { get; set; } // 
}
public class Game
{
    public void HandleMessage(Message message)
    {

    } 
    public enum GameModeType
    {
        Hard,
        Easy
    }
    private string Id;
    private string gameState;
    private GameModeType gameMode;
    private string seed;
    private User user;
    private Dictionary<(int x, int y), Block> grid;

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
    public Dictionary<(int x, int y), Block> GetGrid()
    {
        return null;
    }

    private Dictionary<(int x, int y), Block> LoadGameFromFile()
    {
        return null;
    }

    private Dictionary<(int x, int y), Block> GetGridFromFile()
    {
        return null;
    }
    public List<Dictionary<(int x, int y), Block>> GetGrid() { return grid; }
    public void SetGrid(List<Dictionary<(int x, int y), Block>> value) { grid = value; }
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

public class Block
{
    private string blockId;
    private string name;
    private bool canPass;
    private bool canSpawn;

    public string GetBlockId() { return blockId; }
    public void SetBlockId(string value) { blockId = value; }

    public string GetName() { return name; }
    public void SetName(string value) { name = value; }

    public bool GetCanPass() { return canPass; }
    public void SetCanPass(bool value) { canPass = value; }

    public bool GetCanSpawn() { return canSpawn; }
    public void SetCanSpawn(bool value) { canSpawn = value; }
}

public class FloorBlock : Block // INHERITANCE
{
    private bool canPass = false;

    public new bool GetCanPass() { return canPass; }
    public new void SetCanPass(bool value) { canPass = value; }
}

public class WallBlock : Block
{
    private bool canPass = true;

    public new bool GetCanPass() { return canPass; }
    public new void SetCanPass(bool value) { canPass = value; }
}

public class User
{
    private List<Item> inventory;
    private string userId;
    private string userName;
    private string password;
    private (int x, int y) userCoordinates;

    public List<Item> GetInventory() { return inventory; }
    public void SetInventory(List<Item> value) { inventory = value; }

    public string GetUserId() { return userId; }
    public void SetUserId(string value) { userId = value; }

    public string GetUserName() { return userName; }
    public void SetUserName(string value) { userName = value; }

    public string GetPassword() { return password; }
    public void SetPassword(string value) { password = value; }

    public (int x, int y) GetUserCoordinates() { return userCoordinates; }
    public void SetUserCoordinates((int x, int y) value) { userCoordinates = value; }
}

public class ScapeMonster
{
    public class ScapeMonsterMove //COMPOSITION FILLED
    {
        private int moveDamage;
        private string name;

        public int GetMoveDamage() { return moveDamage; }
        public void SetMoveDamage(int value) { moveDamage = value; }

        public string GetName() { return name; }
        public void SetName(string value) { name = value; }
    }

    private string scapeMonsterID;
    public enum ScapeMonsterType
    {
        Fire,
        Water,
        Rock
    }
    private (int xCord, int yCord) characterCoordinates;
    private string scapeMonsterName;
    private string scapeMonsterId;
    private ScapeMonsterType type;
    private double maximumHealth;
    private bool isBoss;
    private double health;
    private double damagePerHit;
    private List<ScapeMonsterMove> moves;
    private int level;

    public string GetScapeMonsterID() { return scapeMonsterID; }
    public void SetScapeMonsterID(string value) { scapeMonsterID = value; }

    public (int xCord, int yCord) GetCharacterCoordinates() { return characterCoordinates; }
    public void SetCharacterCoordinates((int xCord, int yCord) value) { characterCoordinates = value; }

    public string GetScapeMonsterName() { return scapeMonsterName; }
    public void SetScapeMonsterName(string value) { scapeMonsterName = value; }

    public string GetScapeMonsterId() { return scapeMonsterId; }
    public void SetScapeMonsterId(string value) { scapeMonsterId = value; }

    public ScapeMonsterType GetType() { return type; }
    public void SetType(ScapeMonsterType value) { type = value; }

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
