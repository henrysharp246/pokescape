using Newtonsoft.Json;
using PokescapeServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using PokescapeServer;



//ALL THE MAIN LOGIC IS DONE INSIDE THE GAME CLASS
//There is one game per user... i.e. if 10 users are connected there are 10 games

//When we send the user, it updates the grid shown in the game based on where the user is
public class Game
{

    public enum GameModeType
    {
        Hard,
        Easy
    }
    public int gridSize = 210;
    private string GameId;
    private string gameState;
    private GameModeType gameMode;
    private string seed;
    private User user;
    private Dictionary<(int x, int y), Block> grid;
    private WebSocket currentSocket;

    //WHEN THIS RUNS A NEW GAME HAS BEEN CREATED
    public Game(WebSocket webSocket) 

    {
        user = new User();
        currentSocket = webSocket;
        //When there is a new game we want to send them a new grid
    }

    //all [async Task] means here is that when we run this method,
    //we can wait for it to complete before moving to the next line, by using await StartGame
    public async Task StartGame()
    {
        grid = CreateGrid();
        await SendMessage("grid",grid);
        await SendMessage("user", user);



    }

    //THE BELOW 3 METHODS ARE THE SAME, JUST ACCEPT DIFFERENT TYPES TO MAKE PROGRAMMING EASIER
    public async Task SendMessage(string messageType, object messageData)
    {
        
        await SendMessage(messageType, JsonConvert.SerializeObject(messageData));

    }
    public async Task SendMessage(string messageType, string messageData)
    {
        Message message = new Message();
        message.MessageType = messageType;
        message.Data = messageData;
        await SendMessage(message);
        
    }
    public async Task SendMessage(Message message) 
    {
        Console.WriteLine("SENDING TO WEB PAGE: " + JsonConvert.SerializeObject(message));
        await currentSocket.SendMessage(message);
    }

    /// <summary>
    /// This Is called when the user has sent us something
    /// </summary>
    /// <param name="message"></param>
    /// <returns>MESSAGES TO SEND</returns>
    public async Task HandleMessage(Message message)
    {
        Console.WriteLine("RECEIVED FROM USER: " + JsonConvert.SerializeObject(message));

    
        switch (message.MessageType)
        {
            case "MOVE_UP": //here based on the message type, we add messages to the messagesToSend list based on how we respond
                await MoveUp();
           
                break;
            case "MOVE_DOWN":
                await MoveDown();
    
                break;
            case "MOVE_LEFT":
                await MoveLeft();

                break;
            case "MOVE_RIGHT":
                await MoveRight();
  
                break;


        }

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


 

    public async Task MoveUp()
    {
        var coords = user.UserCoordinates;
        coords.y += 1;
        user.UserCoordinates = coords;
        //log what is in folder with ./ file path here

        var directoryPath = "./";
        var fullPath = System.IO.Path.GetFullPath(directoryPath);
        var folderName = new System.IO.DirectoryInfo(fullPath).Name;
        Console.WriteLine(folderName);

        user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingupblock.png";
        await SendMessage("user", user);
    }
    public async Task MoveDown()
    {
        var coords = user.UserCoordinates;
        coords.y -= 1;
        user.UserCoordinates = coords;
        user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingdownblock.png";
        await SendMessage("user", user);

    }
    public async Task MoveLeft()
    {
        var coords = user.UserCoordinates;
        coords.x -= 1;
        user.UserCoordinates = coords;
        user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingleftblock.png";
        await SendMessage("user", user);

    }
    public async Task MoveRight()
    {
        var coords = user.UserCoordinates;
        coords.x += 1; user.UserCoordinates = coords; 
        user.UserCoordinates = coords;
        user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingrightblock.png";
        await SendMessage("user", user);
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
/**     public Dictionary<(int x, int y), Block> GetVisibleGrid()
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
    }**/
    
    public Dictionary<(int x, int y), Block> GetGrid() { return grid; }
    public void SetGrid(Dictionary<(int x, int y), Block> value) { grid = value; }
}

