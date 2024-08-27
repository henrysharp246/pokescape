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
    public int gridSize = GameConfig.VisibleGridWidth* GameConfig.VisibleGridWidth;
    public int gridWidth = GameConfig.VisibleGridWidth;
    private string GameId;
    private string gameState;
    private GameModeType gameMode;
    private string seed;

    private int LeftDoorCount = 0;
    private int RightDoorCount = 0;
    private int TopDoorCount = 0;
    private int BottomDoorCount = 0;

    private int DoorCount = 0;

    private User user;
    private Dictionary<(int x, int y), Block> currentGrid;
    private List<Dictionary<(int x, int y), Block>> grids;
    private WebSocket currentSocket;
    private int currentGridCount = 1 ;
    

    //WHEN THIS RUNS A NEW GAME HAS BEEN CREATED
    public Game(WebSocket webSocket) 

    {
        user = new User();
        currentSocket = webSocket;
        //When there is a new game we want to send them a new grid
        currentGrid = new Dictionary<(int x, int y), Block>();
    }

    //all [async Task] means here is that when we run this method,
    //we can wait for it to complete before moving to the next line, by using await StartGame
    public async Task StartGame()
    {

        var rand = new Random();
        grids = new();

        int numberOfRooms = rand.Next(GameConfig.MinRooms, GameConfig.MaxRooms);
        for(int i = 0; i < numberOfRooms; i++)
        {
            var newGrid = CreateGrid();
           
            grids.Add(newGrid);
        }
        currentGrid = grids.First();
        SetUserCoordinatesBasedOnGrid(currentGrid);



        await SendMessage("grid",currentGrid);
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

    public async Task ScapeMonsterEncounter()
    {
     

    }
    
    public void SetUserCoordinatesBasedOnGrid(Dictionary<(int x, int y), Block> grid, Type entranceType = null)
    {
        for (int x = 0; x < GameConfig.VisibleGridWidth; x++)
        {
            for (int y = 0; y < GameConfig.VisibleGridWidth; y++)
            {
                var block = grid[(x, y)];

                // Check if entranceType is provided and the block is of that exact type
                if (entranceType != null)
                {
                    if (block.GetType() == entranceType)
                    {
                        block.HasUser = true;
                        grid[(x, y)] = block;
                        user.UserCoordinates = (x, y);
                        return;
                    }
                }
                // If entranceType is not provided, place on any passable block
                else if (block.CanPass)
                {
                    block.HasUser = true;
                    grid[(x, y)] = block;
                    user.UserCoordinates = (x, y);
                    return;
                }
            }
        }

        Console.WriteLine("COULD NOT SET USER COORDINATES!!! NO CORRESPONDING ENTRANCE FOUND");
    }

    public Dictionary<(int x, int y), Block> CreateGrid()
    {
        var room = GenerateRoom();
        var finalGrid = RoomToGrid(room);

      
        return finalGrid;

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="minRoomWidth"></param>
    /// <param name="maxRoomWidth"></param>
    /// <param name="minRoomHeight"></param>
    /// <param name="maxRoomHeight"></param>
    /// <returns></returns>
    public Dictionary<(int x, int y), Block> GenerateRoom(int minRoomWidth = GameConfig.MinRoomWidth, int maxRoomWidth = GameConfig.MaxRoomWidth, int minRoomHeight = GameConfig.MinRoomWidth, int maxRoomHeight = GameConfig.MaxRoomWidth)
    {
        try
        {
            Random rnd = new Random();

            int roomWidth = rnd.Next(minRoomWidth, maxRoomWidth);
            int roomHeight = rnd.Next(minRoomHeight, maxRoomHeight);

            Random random = new Random();

            int numberofdoors = random.Next(GameConfig.MinDoorsInRoom, GameConfig.MaxDoorsInRoom);

            Dictionary<(int x, int y), Block> room = new();
            for (int x = 0; x < roomWidth; x++)
            {
                for (int y = 0; y < roomHeight; y++)
                {
                    Block blockToAdd;
                    if (x == 0 || y == roomHeight - 1 || x == roomWidth - 1 || y == 0)
                    {
                        blockToAdd = new StoneWallBlock();



                    }
                    else
                    {
                        blockToAdd = new StoneFloorBlock();
                    }
                    room.Add((x, y), blockToAdd);


                }
            }



            var wallBlocksAndCoordinates = room.Where(x => x.Value is StoneWallBlock).ToList();

            while (numberofdoors > 0)
            {
                int randomNum = random.Next(0, wallBlocksAndCoordinates.Count());
                var selectedBlockAndCoords = wallBlocksAndCoordinates[randomNum];
                var selectedBlock = selectedBlockAndCoords.Value;
                var selectedCoords = selectedBlockAndCoords.Key;
                Block blockAbove; Block blockBelow; Block blockLeft; Block blockRight;

                blockLeft = room.GetValueOrDefault((selectedCoords.x - 1, selectedCoords.y), null);
                blockRight = room.GetValueOrDefault((selectedCoords.x + 1, selectedCoords.y), null);
                blockAbove = room.GetValueOrDefault((selectedCoords.x, selectedCoords.y + 1), null);
                blockBelow = room.GetValueOrDefault((selectedCoords.x, selectedCoords.y - 1), null);

                
              

                //that we only add a top entrance when the number of top entrances >= number of the bottom entrances
                if ( blockLeft is WallBlock && blockRight is WallBlock && blockAbove == null && TopDoorCount <= BottomDoorCount && LeftDoorCount == RightDoorCount) // door upwards
                {
                    Entrance entrance = new TopEntrance();
                    
                    entrance.EntranceId = TopDoorCount;
                    
                    entrance.CorrespondingRoomId = DoorCount + 1;
                    if(TopDoorCount < BottomDoorCount)
                    {
                       entrance.CorrespondingRoomId = DoorCount-1;
                    }
                    room.Remove((selectedCoords.x, selectedCoords.y));
                    room.Add((selectedCoords.x, selectedCoords.y), entrance);
                    TopDoorCount++;
                    numberofdoors--;
                    DoorCount++;
                }
                else if (blockLeft is WallBlock && blockRight is WallBlock && blockBelow == null && BottomDoorCount <= TopDoorCount && LeftDoorCount == RightDoorCount) // door below
                {

                    Entrance entrance = new BottomEntrance();
                   
                    entrance.EntranceId = BottomDoorCount;
                    entrance.CorrespondingRoomId = DoorCount + 1;
                    if (TopDoorCount > BottomDoorCount)
                    {
                       entrance.CorrespondingRoomId = DoorCount-1;
                    }
                    room.Remove((selectedCoords.x, selectedCoords.y));
                    room.Add((selectedCoords.x, selectedCoords.y), entrance);
                    BottomDoorCount++;
                    numberofdoors--;
                    DoorCount++;
                }
                else if (blockAbove is WallBlock && blockBelow is WallBlock && blockRight == null && RightDoorCount <= LeftDoorCount && TopDoorCount == BottomDoorCount) // door to right 
                {
                    Entrance entrance = new RightEntrance();
                 
                    entrance.CorrespondingRoomId = DoorCount + 1;
                    if (RightDoorCount < LeftDoorCount)
                    {
                        entrance.CorrespondingRoomId = DoorCount-1;
                    }
                    entrance.EntranceId = RightDoorCount;
                    room.Remove((selectedCoords.x, selectedCoords.y));
                    room.Add((selectedCoords.x, selectedCoords.y), entrance);
                    numberofdoors--;
                    RightDoorCount++;
                    DoorCount++;
                }

                else if (blockAbove is WallBlock && blockBelow is WallBlock && blockLeft == null && LeftDoorCount <= RightDoorCount && TopDoorCount == BottomDoorCount) // door to left
                {
                    Entrance entrance = new LeftEntrance();
                    
                    entrance.CorrespondingRoomId = DoorCount + 1;
                    entrance.EntranceId = LeftDoorCount;
                    if (RightDoorCount > LeftDoorCount)
                    {
                        entrance.CorrespondingRoomId = DoorCount-1;
                    }
                    room.Remove((selectedCoords.x, selectedCoords.y));
                    room.Add((selectedCoords.x, selectedCoords.y), entrance);
                    numberofdoors--;
                    LeftDoorCount++;
                    DoorCount++;
                }
            }

            Console.WriteLine($"Room Generated has a width of {roomWidth} and a height of {roomHeight}");

            //for each loop and we find all of the entrance we have created.
            //link up ids


            return room;
        }
        catch (Exception ex)
        {

            Console.WriteLine(ex.Message);
            Console.Write(ex.StackTrace);
        }
        return null;
    }
 
    public int GetRoomWidth(Dictionary<(int x, int y), Block> room)
    {
        // Find the maximum x coordinate to determine the width of the room
        int maxX = room.Keys.Max(key => key.x);

        // The width is maxX + 1 because coordinates are 0-based
        return maxX + 1;
    }
    public int GetRoomHeight(Dictionary<(int x, int y), Block> room)
    {
        // Find the maximum y coordinate to determine the height of the room
        int maxY = room.Keys.Max(key => key.y);

        // The height is maxY + 1 because coordinates are 0-based
        return maxY + 1;
    }


    public Dictionary<(int x, int y), Block> RoomToGrid(Dictionary<(int x, int y), Block>  room)
    {
        Dictionary<(int x, int y), Block> gridGenerated = new Dictionary<(int x, int y), Block>();
        try
        {
            int roomHeight = GetRoomHeight(room);
            int roomWidth = GetRoomWidth(room);

            int roomStartXPosition = (GameConfig.VisibleGridWidth / 2) - roomWidth/2 ;
        int roomStartYPosition = (GameConfig.VisibleGridWidth / 2) - roomHeight/2;
    
            foreach (var coordAndBlock in room)
         {
            //translates the room by half of the grid width and height (places the room in the middle of the grid)
            gridGenerated[((roomStartXPosition + coordAndBlock.Key.x), (roomStartYPosition + coordAndBlock.Key.y))] = coordAndBlock.Value;
        
         }

        //we need to fill the rest of the grid with blank spaces


           // Console.WriteLine(gridGenerated);
        
            for (int x = 0; x < GameConfig.VisibleGridWidth; x++)
            {
                for (int y = 0; y < GameConfig.VisibleGridWidth; y++)
                {
                   // Console.WriteLine($"x{x} y{y}");
                    if (!gridGenerated.ContainsKey((x, y)))
                    {
                        Block block = new BlankBlock();

                        gridGenerated.Add((x, y), block);
                    }

                }
            }

            Console.WriteLine($"Room Start x Position Is {roomStartXPosition} Room Start y Position Is {roomStartYPosition}");

        }
        catch (Exception ex) { Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
        gridGenerated = SortDictionary(gridGenerated);


       
        return gridGenerated;
    }

    public static Dictionary<(int x, int y), Block> SortDictionary(Dictionary<(int x, int y), Block> dictionary)
    {
        return dictionary
            .OrderBy(kvp => kvp.Key.x)  // First sort by x
            .ThenBy(kvp => kvp.Key.y)    // Then sort by y
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public Dictionary<(int x, int y), Block> CreateGridV0()
    {
        int totalRoomSize = 0;

        while (totalRoomSize < gridSize)
        {

            //GENERATE ROOM

            //totalRoomSize += newRoomSize 

        }
        return null;

    }

    public Dictionary<(int x, int y), Block> CreateGridBlank()
    {
        Dictionary<(int x, int y), Block> grid = new();
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Block block = new StoneFloorBlock();
    
                grid.Add((x, y), block);

            }
        }
        this.currentGrid = grid;
        return grid;

    }
    public Dictionary<(int x, int y), Block> CreateGridV2()
    {
        Dictionary<(int x, int y), Block> grid = new();
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                string blockToLeftName = GetBlockNameToLeft(x, y);
                string blockToRightName = GetBlockNameToRight(x, y);
                string blockAboveName = GetBlockNameAbove(x, y);
                string blockBelowName = GetBlockNameBelow(x, y);
                string blockToTopLeftName = GetBlockNameToTopLeft(x, y);
                string blockToTopRightName = GetBlockNameToTopRight(x, y);
                string blockToBottomLeftName = GetBlockNameToBottomLeft(x, y);
                string blockToBottomRightName = GetBlockNameToBottomRight(x, y);


                if (x < 55)
                {
                    if (y < -105)
                    {
                       
                        Block blankblock = new BlankBlock();
                        blankblock.CanPass = true;

                        grid.Add((x, y), blankblock);
                    }

                }
                else
                {
                    Random random = new Random();
                    int rand = random.Next(0, 10);
                    if (blockToLeftName == "blank"|| blockAboveName == "blank" || blockToRightName == "blank" || blockBelowName == "blank" || blockToTopLeftName == "blank" || blockToTopRightName == "blank" || blockToBottomRightName == "blank" || blockToBottomLeftName == "blank"  )
                    {
                        Block wallblock = new StoneWallBlock();
                        wallblock.CanPass = true;
                        grid.Add((x, y), wallblock);
                    }

                    if (blockToLeftName == "stonewallblock" || blockAboveName == "stonewallblock" || blockToRightName == "stonewallblock" || blockBelowName == "stonewallblock") 
                    {
                        Block stonefloorblock = new StoneWallBlock();
                        grid.Add((x, y), stonefloorblock);
                    }


                
                    
                }
            }
        }
        this.currentGrid = grid;
        return grid;
    }
    public string GetBlockCoordsToLeft(int x, int y)
    {
        if (x > 0 && currentGrid.ContainsKey((x - 1, y)))
        {
            return currentGrid[(x - 1, y)].Name;
        }
        else { return "default"; } // Or any other default name you prefer
    }

    public string GetBlockToCoordsRight(int x, int y)
    {
        if (x < gridSize - 1 && currentGrid.ContainsKey((x + 1, y)))
        {
            return currentGrid[(x + 1, y)].Name;
        }
        else { return "default"; }
    }

    public string GetBlockCoordsAbove(int x, int y)
    {
        if (y < gridSize - 1 && currentGrid.ContainsKey((x, y + 1)))
        {
            return currentGrid[(x, y + 1)].Name ;
        }
        else { return "default"; }
    }

    public Block GetBlockBelow(int x, int y)
    {
        if (y > 0 && currentGrid.ContainsKey((x, y - 1)))
        {
            return currentGrid[(x, y - 1)];
        }
        else { return null; }
    }

    public Block GetBlockToTopLeft(int x, int y)
    {
        if (x > 0 && y < gridSize - 1 && currentGrid.ContainsKey((x - 1, y + 1)))
        {
            return currentGrid[(x - 1, y + 1)];
        }
        else { return null; }
    }

    public Block GetBlockToTopRight(int x, int y)
    {
        if (x < gridSize - 1 && y < gridSize - 1 && currentGrid.ContainsKey((x + 1, y + 1)))
        {
            return currentGrid[(x + 1, y + 1)];
        }
        else { return null; }
    }

    public Block GetBlockToBottomLeft(int x, int y)
    {
        if (x > 0 && y > 0 && currentGrid.ContainsKey((x - 1, y - 1)))
        {
            return currentGrid[(x - 1, y - 1)];
        }
        else { return null; }
    }

    public Block GetBlockToBottomRight(int x, int y)
    {
        if (x < gridSize - 1 && y > 0 && currentGrid.ContainsKey((x + 1, y - 1)))
        {
            return currentGrid[(x + 1, y - 1)];
        }
        else { return null; }
    }
    public string GetBlockNameToLeft(int x, int y)
    {
        if (x > 0 && currentGrid.ContainsKey((x - 1, y)))
        {
            return currentGrid[(x - 1, y)].Name;
        }
        else { return "default"; } // Or any other default name you prefer
    }

    public string GetBlockNameToRight(int x, int y)
    {
        if (x < gridSize - 1 && currentGrid.ContainsKey((x + 1, y)))
        {
            return currentGrid[(x + 1, y)].Name;
        }
        else { return "default"; }
    }

    public string GetBlockNameAbove(int x, int y)
    {
        if (y < gridSize - 1 && currentGrid.ContainsKey((x, y + 1)))
        {
            return currentGrid[(x, y + 1)].Name;
        }
        else { return "default"; }
    }

    public string GetBlockNameBelow(int x, int y)
    {
        if (y > 0 && currentGrid.ContainsKey((x, y - 1)))
        {
            return currentGrid[(x, y - 1)].Name;
        }
        else { return "default"; }
    }

    public string GetBlockNameToTopLeft(int x, int y)
    {
        if (x > 0 && y < gridSize - 1 && currentGrid.ContainsKey((x - 1, y + 1)))
        {
            return currentGrid[(x - 1, y + 1)].Name;
        }
        else { return "default"; }
    }

    public string GetBlockNameToTopRight(int x, int y)
    {
        if (x < gridSize - 1 && y < gridSize - 1 && currentGrid.ContainsKey((x + 1, y + 1)))
        {
            return currentGrid[(x + 1, y + 1)].Name;
        }
        else { return "default"; }
    }

    public string GetBlockNameToBottomLeft(int x, int y)
    {
        if (x > 0 && y > 0 && currentGrid.ContainsKey((x - 1, y - 1)))
        {
            return currentGrid[(x - 1, y - 1)].Name;
        }
        else { return "default"; }
    }

    public string GetBlockNameToBottomRight(int x, int y)
    {
        if (x < gridSize - 1 && y > 0 && currentGrid.ContainsKey((x + 1, y - 1)))
        {
            return currentGrid[(x + 1, y - 1)].Name;
        }
        else { return "default"; }
    }




    public static void LogGrid(Block[][] grid)
    {
        for (int x = 0; x < grid.Length; x++)
        {
            for (int y = 0; y < grid.Length; y++)
            {
                //Console.WriteLine(grid[x][y].Name);
            }
            //Console.WriteLine("\n");
        }
    }


 

    public async Task MoveUp()
    {
        
        Block blockabove = currentGrid[(user.UserCoordinates.x, user.UserCoordinates.y + 1)];
        Block currentBlock = currentGrid[user.UserCoordinates];
        if (currentBlock is TopEntrance)
        {
            TopEntrance entrance = (TopEntrance)currentBlock;
            entrance.EntranceId = currentGridCount;
            if(entrance.CorrespondingRoomId == null || entrance.CorrespondingRoomId == -1)
            {
                entrance.CorrespondingRoomId = currentGridCount++;
            }
            currentGrid = grids[entrance.CorrespondingRoomId];
            user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingupblock.png";
            SetUserCoordinatesBasedOnGrid(currentGrid, typeof (BottomEntrance));
            await SendMessage("grid", currentGrid);
            await SendMessage("user", user);

            return;
        }

        if (blockabove.CanPass == true)
        {
            var coords = user.UserCoordinates;
            coords.y += 1;
            user.UserCoordinates = coords;

            //log what is in folder with ./ file path here

            var directoryPath = "./";
            var fullPath = System.IO.Path.GetFullPath(directoryPath);
            var folderName = new System.IO.DirectoryInfo(fullPath).Name;
            Console.WriteLine(folderName);

           
        }
        user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingupblock.png";
        await SendMessage("user", user);
    }
    public async Task MoveDown()
    {
        Block blockbelow = currentGrid[(user.UserCoordinates.x, user.UserCoordinates.y - 1)];
        Block currentBlock = currentGrid[user.UserCoordinates];
        if (currentBlock is BottomEntrance)
        {
            BottomEntrance entrance = (BottomEntrance)currentBlock;
            entrance.EntranceId = currentGridCount;
            if (entrance.CorrespondingRoomId == null || entrance.CorrespondingRoomId == -1)
            {
                entrance.CorrespondingRoomId = currentGridCount++;
            }
            currentGrid = grids[entrance.CorrespondingRoomId];
            SetUserCoordinatesBasedOnGrid(currentGrid, typeof(TopEntrance));
            await SendMessage("grid", currentGrid);
             await SendMessage("user", user);
            user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingdownblock.png";
            return;

        }
        if (blockbelow.CanPass == true)
        {
            var coords = user.UserCoordinates;
            coords.y -= 1;
            user.UserCoordinates = coords;
        }
        user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingdownblock.png";
        await SendMessage("user", user);

    }
    public async Task MoveLeft()
    {
        Block blockabove = currentGrid[(user.UserCoordinates.x-1, user.UserCoordinates.y )];
        Block currentBlock = currentGrid[user.UserCoordinates];
        if (currentBlock is LeftEntrance)
        {
            LeftEntrance entrance = (LeftEntrance)currentBlock;
            entrance.EntranceId = currentGridCount;
            if (entrance.CorrespondingRoomId == null || entrance.CorrespondingRoomId == -1)
            {
                entrance.CorrespondingRoomId = currentGridCount++;
            }
            currentGrid = grids[entrance.CorrespondingRoomId];
             user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingleftblock.png";
            SetUserCoordinatesBasedOnGrid(currentGrid, typeof(RightEntrance));
            await SendMessage("grid", currentGrid);
            await SendMessage("user", user);
           return;

        }
        if (blockabove.CanPass == true)
        {
            var coords = user.UserCoordinates;
            coords.x -= 1;
            user.UserCoordinates = coords;
        }
        user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingleftblock.png";
        await SendMessage("user", user);

    }
    public async Task MoveRight()
    {
        Block blockabove = currentGrid[(user.UserCoordinates.x+1, user.UserCoordinates.y)];
        Block currentBlock = currentGrid[user.UserCoordinates];
        if (currentBlock is RightEntrance)
        {
            RightEntrance entrance = (RightEntrance)currentBlock;
            entrance.EntranceId = currentGridCount;
            if (entrance.CorrespondingRoomId == null || entrance.CorrespondingRoomId == -1)
            {
                entrance.CorrespondingRoomId = currentGridCount++;
            }
            currentGrid = grids[entrance.CorrespondingRoomId];
            user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingrightblock.png";
            SetUserCoordinatesBasedOnGrid(currentGrid, typeof(LeftEntrance));
            await SendMessage("grid", currentGrid);
            await SendMessage("user", user);
            return;

        }
        if (blockabove.CanPass == true)
        {
            var coords = user.UserCoordinates;
            coords.x += 1; user.UserCoordinates = coords;
            user.UserCoordinates = coords;
        }
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
        return currentGrid;
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
    
    public Dictionary<(int x, int y), Block> GetGrid() { return currentGrid; }
    public void SetGrid(Dictionary<(int x, int y), Block> value) { currentGrid = value; }
}

