using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            case "MOVE_UP": //here based on the message type, we add messages to the messagesToSend list based on how we respond
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
                block.CanPass = true;
                block.Name = "grass"; //changed to setting like this for easier format
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
                Console.WriteLine(grid[x][y].Name);
            }
            Console.WriteLine("\n");
        }
    }

    public void MoveUp()
    {
        var coords = user.UserCoordinates;
        coords.y += 1;
        user.UserCoordinates = coords;
        //log what is in folder with ./ file path here

        var directoryPath = "./";
        var fullPath = System.IO.Path.GetFullPath(directoryPath);
        var folderName = new System.IO.DirectoryInfo(fullPath).Name;
        Console.WriteLine(folderName);

        user.UserImage = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Characterfacingupblock.png";
    }
    public void MoveDown()
    {
        var coords = user.UserCoordinates;
        coords.y -= 1;
        user.UserCoordinates = coords;
        user.UserImage = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Characterfacingdownblock.png";

    }
    public void MoveLeft()
    {
        var coords = user.UserCoordinates;
        coords.x -= 1;
        user.UserCoordinates = coords;
        user.UserImage = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Characterfacingleftblock.png";

    }
    public void MoveRight()
    {
        var coords = user.UserCoordinates;
        coords.x += 1; user.UserCoordinates = coords; 
        user.UserCoordinates = coords;
        user.UserImage = "C:\\Users\\henry\\source\\repos\\pokescape\\Image\\blockImages\\Characterfacingrightblock.png";
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
        var coords = user.UserCoordinates;
        int xCount = 0; int yCount = 0;
        for (int x = coords.x - virtualGridSize / 2; x <= coords.x + virtualGridSize / 2; x++)
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

