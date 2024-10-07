using Newtonsoft.Json;
using PokescapeServer;
using System.Net.WebSockets;




//ALL THE MAIN LOGIC IS DONE INSIDE THE GAME CLASS
//There is one game per user... i.e. if 10 users are connected there are 10 games


public class Game
{

    public enum GameModeType
    {
        Hard,
        Easy
    }

    public class SerialisedGame // we must have this as we cant serialise private variables. the other reason is so we can send up a simpler version of the game, without unncessary details
    {// thus this is abstraction!
        public string GameId;
        public string gameState;
        public User user;
        public Dictionary<string, Block> currentGrid; 
        public List<Dictionary<string, Block>> grids;
    }



    public int gridSize = GameConfig.VisibleGridWidth * GameConfig.VisibleGridWidth;
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
    private int currentGridCount = 1;

    static int chestCount = 0;
    static int KeyCount = 0;

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
        for (int i = 0; i < numberOfRooms; i++)
        {
            var newGrid = CreateGrid(numberOfRooms, i+1);

            grids.Add(newGrid);
        }
        currentGrid = grids.First();
        SetUserCoordinatesBasedOnGrid(currentGrid);

        await TestIfRoomsAreOk();

        await SendMessage("grid", currentGrid);
        await SendMessage("user", user);




    }
    public async Task TestIfRoomsAreOk()
    {
        Console.WriteLine($"Top Door Count {TopDoorCount}");
        Console.WriteLine($"Right Door Count {RightDoorCount}");
        Console.WriteLine($"Left Door Count {LeftDoorCount}");
        Console.WriteLine($"Bottom Door Count {BottomDoorCount}");
        Console.WriteLine($"Number Of Rooms {grids.Count}");
    }

    public async Task LoadGame(String gameString)
    {
        SerialisedGame newGame = JsonConvert.DeserializeObject<SerialisedGame>(gameString);
        this.user = newGame.user;

        this.grids = new List<Dictionary<(int, int), Block>>();

        
        foreach (var grid in newGame.grids)
        {
            Dictionary<(int, int), Block> gridDict = new Dictionary<(int, int), Block>();
            foreach (var kvp in grid)
            {
                var keyParts = kvp.Key.Split(',');
                var key = (int.Parse(keyParts[0]), int.Parse(keyParts[1]));
                gridDict[key] = kvp.Value;
            }
            this.grids.Add(gridDict);
        }

        this.currentGrid = new Dictionary<(int, int), Block>();
        foreach (var kvp in newGame.currentGrid)
        {
            var keyParts = kvp.Key.Split(',');
            var key = (int.Parse(keyParts[0]), int.Parse(keyParts[1]));
            this.currentGrid[key] = kvp.Value;
        }

        this.GameId = newGame.GameId;
        this.gameState = newGame.gameState;

        await SendMessage("grid", currentGrid);
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
        //Console.WriteLine("SENDING TO WEB PAGE: " + JsonConvert.SerializeObject(message));
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


        switch (message.MessageType.ToUpper())
        {
            case "MOVE_UP": //here based on the message type, we add messages to the messagesToSend list based on how we respond
                if (user.InBattle == false)
                {
                    await MoveUp();
                }
                break;
            case "MOVE_DOWN":
                if (user.InBattle == false)
                {
                    await MoveDown();
                }

                break;
            case "MOVE_LEFT":
                if (user.InBattle == false)
                {
                    await MoveLeft();
                }

                break;
            case "MOVE_RIGHT":
                if (user.InBattle == false)
                {
                    await MoveRight();
                }

                break;
            case "MONSTER_SELECTED_FOR_BATTLE":
                if (user.InBattle)
                {
                    await MonsterSelectedForBattle(message.Data);
                }
                else if (user.InBattle == false && user.ItemSelectedId != null)
                {
                    await ApplyItemToMonster(user.ItemSelectedId, message.Data);
                }
                break;
            case "ATTACK_MOVE":
                if (user.InBattle && user.IsTurn)
                {
                    await UserAttack(message.Data);
                }
                break;
            case "ITEM_SELECTED":
                {
                    await ItemSelected(message.Data);
                }
                break;
            case "SAVE_GAME":
                await SaveGameV1();

                break;
            case "LOAD_GAME":
                await LoadGame(message.Data);

                break;

        }

    }
    public async Task SaveGameV1()
    {
        SerialisedGame gameToSave = new SerialisedGame();
        gameToSave.grids = new List<Dictionary<string, Block>>();

        foreach (var grid in this.grids)
        {
            Dictionary<string, Block> gridDict = new Dictionary<string, Block>();
            foreach (var kvp in grid)
            {
                gridDict[$"{kvp.Key.x},{kvp.Key.y}"] = kvp.Value;
            }
            gameToSave.grids.Add(gridDict);
        }

        gameToSave.gameState = this.gameState;

        gameToSave.currentGrid = new Dictionary<string, Block>();
        foreach (var kvp in this.currentGrid)
        {
            gameToSave.currentGrid[$"{kvp.Key.x},{kvp.Key.y}"] = kvp.Value;
        }

        gameToSave.user = this.user;
        gameToSave.GameId = this.GameId;

        await SendMessage("save_game", JsonConvert.SerializeObject(gameToSave, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
    }

    public async Task SaveGame()
    {
       /* SerialisedGame gameToSave = new SerialisedGame();
        gameToSave.grids = new();
        gameToSave.gameState = this.gameState;
        gameToSave.currentGrid = new();
        gameToSave.user = this.user;
        gameToSave.GameId = this.GameId;

        foreach (var grid in this.grids)
        {
            var newGrid = new Dictionary<(int x, int y), Block>();

            foreach (var coordAndBlock in grid)
            {
                var newCoordAndBlock = coordAndBlock;
                newCoordAndBlock.Value.Image = null;
                newCoordAndBlock.Value.DefaultImage = null;

                newGrid.Add(newCoordAndBlock.Key, newCoordAndBlock.Value);
            }
            gameToSave.grids.Add(newGrid);
        }


        foreach (var coordAndBlock in this.currentGrid)
        {
            var newCoordAndBlock = coordAndBlock;
            newCoordAndBlock.Value.Image = null;
            newCoordAndBlock.Value.DefaultImage = null;

            gameToSave.currentGrid.Add(newCoordAndBlock.Key, newCoordAndBlock.Value);
        }

        await SendMessage("save_game", JsonConvert.SerializeObject(gameToSave, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
 */   }
    public async Task ItemSelected(string itemId)
    {
        user.ItemSelectedId = itemId;
    }
    public async Task ApplyItemToMonster(string itemId, string monsterId)
    {
        Item item = user.Inventory.First(x => x.ItemId == itemId);
        ScapeMonster scapemonster = user.ScapeMonsters.First(y => y.ScapeMonsterID == monsterId);
        scapemonster = item.UseItem(scapemonster);
        user.ScapeMonsters.RemoveAll(y => y.ScapeMonsterID == monsterId);
        user.ScapeMonsters.Add(scapemonster);
        user.Inventory.Remove(item);
        await SendMessage("user", user);

    }
    static bool IsEven(int number)
    {
        return number % 2 == 0;
    }
    public async Task BattleWon(Battle battle)
    {

        await SendMessage("battleDialog", $"You defeated {battle.OpponentScapeMonster.ScapeMonsterName}! They will be added to your collection.");
        Thread.Sleep(500);
        if (IsEven(battlecount))
        {
            battle.UserScapeMonster.Level += 1;
            await SendMessage("battleDialog", $"{battle.UserScapeMonster.ScapeMonsterName} levelled up to level {battle.UserScapeMonster.Level}!");
        }

        Thread.Sleep(2500);

        user.ScapeMonsters.Add(battle.OpponentScapeMonster);
        user.CurrentBattle = null;
        user.InBattle = false;
        await SendMessage("user", user);
        await SendMessage("hideBattle", $"");
    }
    public async Task BattleLost(Battle battle)
    {
        await SendMessage("battleDialog", $"You lost vs {battle.OpponentScapeMonster.ScapeMonsterName}!");
        Thread.Sleep(2500);
        user.CurrentBattle = null;
        user.InBattle = false;
        await SendMessage("user", user);
        await SendMessage("hideBattle", $"");
    }
    public async Task UserAttack(string moveId)
    {
        try
        {
            ScapeMonsterMove move = user.CurrentBattle.UserScapeMonster.GetMove(moveId);
            var currentOponent = user.CurrentBattle.OpponentScapeMonster;
            ScapeMonster opponent = move.PerformMove(currentOponent);

            if (opponent.Health <= 0)
            { opponent.Health = 0; }
            user.CurrentBattle.OpponentScapeMonster = opponent;


            await SendMessage("battleDialog", $"{user.CurrentBattle.UserScapeMonster.ScapeMonsterName} used move {move.MoveName}, and performed {move.MoveDamage} damage");
            await SendMessage("battle", user.CurrentBattle);
            Thread.Sleep(1500);

            if (opponent.Health <= 0)
            {
                await BattleWon(user.CurrentBattle);
                return;
            }

            ScapeMonster userMonster = user.CurrentBattle.UserScapeMonster;
            var opponentMove = opponent.GetRandomMove();
            ScapeMonster newUserMonster = opponentMove.PerformMove(userMonster);
            if (newUserMonster.Health <= 0)
            { newUserMonster.Health = 0; }
            user.CurrentBattle.UserScapeMonster = newUserMonster;

            await SendMessage("battleDialog", $"Opponent {user.CurrentBattle.OpponentScapeMonster.ScapeMonsterName} used move {opponentMove.MoveName}, and performed {opponentMove.MoveDamage} damage");
            await SendMessage("battle", user.CurrentBattle);
            if (newUserMonster.Health <= 0)
            {
                await BattleLost(user.CurrentBattle);
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());

        }


    }
    public async Task MonsterSelectedForBattle(string monsterId)
    {
        user.CurrentBattle.UserScapeMonster = user.ScapeMonsters.First(monster => monster.ScapeMonsterID == monsterId);
        await SendMessage("battleDialog", user.CurrentBattle.UserScapeMonster.ScapeMonsterName + " chosen!");
        await SendMessage("battle", user.CurrentBattle);
    }

    static int battlecount = 1;
    public async Task ScapeMonsterEncounter()
    {
        return;
        battlecount++;
      
        Random random = new Random();
        double num = random.NextDouble();
        if (num < GameConfig.ProbabilityOfScapemonster)
        {
            Battle newBattle = new Battle();
            int maxScapeMonsterLevel = user.GetUsersHighestLevelScapemonster();

            newBattle.OpponentScapeMonster = ScapeMonster.GetRandomScapeMonster(maxScapeMonsterLevel);
            newBattle.OpponentScapeMonster.Level = 5;
            ///TO DO SET SCAPEMONSTER LEVELL RANDOMLY
            //maxScapeMonsterLevel - 5;

            //

            user.CurrentBattle = newBattle;
            user.InBattle = true;
            await SendMessage("battleDialog", "Choose Your Scapemonster...");
            await SendMessage("newBattle", newBattle);
            user.IsTurn = true;
            // user.ScapeMonsters.Add(ScapeMonster.GetRandomScapeMonster());
        }
        return;
    }

    public bool SetUserCoordinatesBasedOnGrid(Dictionary<(int x, int y), Block> grid, Type entranceType = null)
    {
        try
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
                            return true;
                        }
                    }
                    // If entranceType is not provided, place on any passable block
                    else if (block.CanPass)
                    {
                        block.HasUser = true;
                        grid[(x, y)] = block;
                        user.UserCoordinates = (x, y);
                        return true;
                    }
                }
            }
            return false;

        }
        catch (Exception ex)
        {
            return false;
        }
        
        Console.WriteLine("COULD NOT SET USER COORDINATES!!! NO CORRESPONDING ENTRANCE FOUND");
    }

    public Dictionary<(int x, int y), Block> CreateGrid(int totalNumberOfGrids, int currentGridCount)
    {
        while (true) //NEED TO REMOVE
        {


            try
            {
                var room = GenerateRoom(totalNumberOfGrids, currentGridCount);
                var finalGrid = RoomToGrid(room);

                return finalGrid;


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        return null;
    }

    public Dictionary<(int x, int y), Block> AddWaterToRoom(Dictionary<(int x, int y), Block> room)
    {
        try
        {
            bool isWaterInRoom = new Random().NextDouble() < GameConfig.ProbabilityOfWater;
            if (!isWaterInRoom)
            {
                return room;
            }
            int roomWidth = room.Keys.Max(k => k.x) + 1;
            int roomHeight = room.Keys.Max(k => k.y) + 1;
            var pond = GenerateBasicPond(roomWidth, roomHeight);

            bool pondPlaced = false;
            int x = 0;
            int y = 0;
            while (!pondPlaced)
            {
                try
                {
                    x = new Random().Next(1, roomWidth - 1);
                    y = new Random().Next(1, roomHeight - 1);
                    pondPlaced = TryPlacePond(x, y, room, pond);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }


            }
            return PlacePond(x, y, room, pond);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
        return null;

    }

    public bool TryPlacePond(int x, int y, Dictionary<(int x, int y), Block> room, Dictionary<(int x, int y), Block> pond)
    {
        try
        {
            int numberOfOriginalWallBlocks = room.Count(block => block.Value is WallBlock);

            Dictionary<(int x, int y), Block> newRoom = room;
            foreach (var coordsAndBlock in pond)
            {


                if (room[(coordsAndBlock.Key.x + x, coordsAndBlock.Key.y + y)] is BlankBlock || room[(coordsAndBlock.Key.x + x, coordsAndBlock.Key.y + y)] == null)
                {
                    return false;
                }
            }
            int numberOfWallBlocksNow = newRoom.Count(block => block.Value is WallBlock);
            if (numberOfWallBlocksNow != numberOfOriginalWallBlocks)
            {
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            return false;
        }
    }
    public Dictionary<(int x, int y), Block> PlacePond(int x1, int y1, Dictionary<(int x, int y), Block> room, Dictionary<(int x, int y), Block> pondParam)
    {
        var pond = pondParam;
        Dictionary<(int x, int y), Block> newRoom = room;

        int roomWidth = room.Keys.Max(k => k.x) + 1;
        int roomHeight = room.Keys.Max(k => k.y) + 1;
        int pondWidth = pond.Keys.Max(k => k.x) + 1;
        int pondHeight = pond.Keys.Max(k => k.y) + 1;
        Random rnd = new();
        int randomX = rnd.Next(GameConfig.MinDecompositionOfPondCorners, pondWidth / 2 - 1);
        int randomY = rnd.Next(GameConfig.MinDecompositionOfPondCorners, pondHeight / 2 - 1);
        for (int x = 0; x < randomX; x++)
        {
            for (int y = 0; y < randomY; y++)
            {
                pond[(x, y)] = new StoneFloorBlock();
            }
        }



        //bottom right corner processing
        randomX = rnd.Next(GameConfig.MinDecompositionOfPondCorners, pondWidth / 2 - 1);
        randomY = rnd.Next(GameConfig.MinDecompositionOfPondCorners, pondHeight / 2 - 1);


        for (int x = pondWidth - randomX; x < pondWidth - 1; x++)
        {
            for (int y = 0; y < randomY; y++)
            {
                pond[(x, y)] = new StoneFloorBlock();
            }
        }



        //top right corner processing
        randomX = rnd.Next(GameConfig.MinDecompositionOfPondCorners, pondWidth / 2 - 1);
        randomY = rnd.Next(GameConfig.MinDecompositionOfPondCorners, pondHeight / 2 - 1);


        for (int x = pondWidth - randomX; x < pondWidth - 1; x++)
        {
            for (int y = pondHeight - 1; y > pondHeight - 1 - randomY; y--)
            {
                pond[(x, y)] = new StoneFloorBlock();
            }
        }




        //top left corner processing
        randomX = rnd.Next(GameConfig.MinDecompositionOfPondCorners, pondWidth / 2 - 1);
        randomY = rnd.Next(GameConfig.MinDecompositionOfPondCorners, pondHeight / 2 - 1);


        for (int x = 0; x < randomX; x++)
        {
            for (int y = pondHeight - randomY; y < pondHeight - 1; y++)
            {
                pond[(x, y)] = new StoneFloorBlock();
            }
        }




        foreach (var coordsAndBlock in pond)
        {
            newRoom[(coordsAndBlock.Key.x + x1, coordsAndBlock.Key.y + y1)] = pond[(coordsAndBlock.Key.x, coordsAndBlock.Key.y)];
        }

        return newRoom;
    }

    public Dictionary<(int x, int y), Block> GenerateRoomCornerStrat()
    {
        try
        {
            Dictionary<(int x, int y), Block> room = GenerateBasicRoom();
            int roomWidth = room.Keys.Max(k => k.x) + 1;
            int roomHeight = room.Keys.Max(k => k.y) + 1;




            Random rnd = new Random();

            int minDecomposition = GameConfig.MinDecompositionOfCorners;

            //bottom left corner processing
            int randomX = rnd.Next(minDecomposition, roomWidth / 2 - 1);
            int randomY = rnd.Next(minDecomposition, roomHeight / 2 - 1);
            int randomMax = Math.Max(randomX, randomY);

            for (int x = 0; x < randomX; x++)
            {
                for (int y = 0; y < randomY; y++)
                {
                    room[(x, y)] = new BlankBlock();
                }
            }

            for (int x = 0; x < randomX; x++)
            {
                for (int y = 0; y < randomY; y++)
                {
                    if (room[(x + 1, y)] is FloorBlock || room[(x, y + 1)] is FloorBlock)
                    {
                        room[(x, y)] = new StoneWallBlock();
                    }
                }
            }

            //bottom right corner processing
            randomX = rnd.Next(minDecomposition, roomWidth / 2 - 1);
            randomY = rnd.Next(minDecomposition, roomHeight / 2 - 1);
            randomMax = Math.Max(randomX, randomY);

            for (int x = roomWidth - randomX; x < roomWidth - 1; x++)
            {
                for (int y = 0; y < randomY; y++)
                {
                    room[(x, y)] = new BlankBlock();
                }
            }

            for (int x = roomWidth - randomX; x < roomWidth - 1; x++)
            {
                for (int y = 0; y < randomY; y++)
                {
                    if (room[(x - 1, y)] is FloorBlock || room[(x, y + 1)] is FloorBlock)
                    {
                        room[(x, y)] = new StoneWallBlock();
                    }
                }
            }

            //top right corner processing
            randomX = rnd.Next(minDecomposition, roomWidth / 2 - 1);
            randomY = rnd.Next(minDecomposition, roomHeight / 2 - 1);
            randomMax = Math.Max(randomX, randomY);

            for (int x = roomWidth - randomX; x < roomWidth - 1; x++)
            {
                for (int y = roomHeight - 1; y > roomHeight - 1 - randomY; y--)
                {
                    room[(x, y)] = new BlankBlock();
                }
            }



            for (int x = roomWidth - randomX; x < roomWidth - 1; x++)
            {
                for (int y = roomHeight - 1; y > roomHeight - 1 - randomY; y--)
                {
                    if (room[(x - 1, y)] is FloorBlock || room[(x, y - 1)] is FloorBlock)
                    {
                        room[(x, y)] = new StoneWallBlock();
                    }
                }
            }

            //top left corner processing
            randomX = rnd.Next(minDecomposition, roomWidth / 2 - 1);
            randomY = rnd.Next(minDecomposition, roomHeight / 2 - 1);
            randomMax = Math.Max(randomX, randomY);

            for (int x = 0; x < randomX; x++)
            {
                for (int y = roomHeight - randomY; y < roomHeight - 1; y++)
                {
                    room[(x, y)] = new BlankBlock();
                }
            }



            for (int x = 0; x < randomX; x++)
            {
                for (int y = roomHeight - randomY; y < roomHeight - 1; y++)
                {
                    if (room[(x + 1, y)] is FloorBlock || room[(x, y - 1)] is FloorBlock)
                    {
                        room[(x, y)] = new StoneWallBlock();
                    }
                }
            }
            room = AddWaterToRoom(room);

            //FINAL TOUCH
            for (int x = 0; x < roomWidth; x++)
            {
                for (int y = 0; y < roomHeight; y++)
                {
                    try
                    {
                        Block blockToLeft = room.TryGetValue((x - 1, y), out var leftBlock) ? leftBlock : null;
                        Block blockToRight = room.TryGetValue((x + 1, y), out var rightBlock) ? rightBlock : null;
                        Block blockUp = room.TryGetValue((x, y + 1), out var upBlock) ? upBlock : null;
                        Block blockDown = room.TryGetValue((x, y - 1), out var downBlock) ? downBlock : null;

                        if ((blockUp == null || blockUp is BlankBlock) && (blockDown == null || blockDown is BlankBlock))
                        {
                            room[(x, y)] = new BlankBlock();
                        }
                        if ((blockToLeft == null || blockToLeft is BlankBlock) && (blockToRight == null || blockToRight is BlankBlock))
                        {
                            room[(x, y)] = new BlankBlock();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }

            return room;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
        return null;

    }

    public Dictionary<(int x, int y), Block> GenerateRoomUsingRandomWalk()
    {
        // Generate the basic room first
        Dictionary<(int x, int y), Block> room = GenerateBasicRoom();

        Random rnd = new Random();
        int roomWidth = room.Keys.Max(k => k.x) + 1;
        int roomHeight = room.Keys.Max(k => k.y) + 1;

        // Determine the number of walls to generate
        int wallCount = rnd.Next(1, 1);

        // Choose a random starting position inside the room
        int startX = rnd.Next(1, roomWidth - 1);
        int startY = rnd.Next(1, roomHeight - 1);

        // Define the directions: (dx, dy) tuples for (up, down, left, right)
        List<(int dx, int dy)> directions = new List<(int dx, int dy)>
    {
        (0, -1), // up
        (0, 1),  // down
        (-1, 0), // left
        (1, 0)   // right
    };

        int currentX = startX;
        int currentY = startY;

        while (wallCount > 0)
        {
            // Randomly select a direction
            var direction = directions[rnd.Next(directions.Count)];
            int newX = currentX + direction.dx;
            int newY = currentY + direction.dy;

            // Ensure the new position is within the room bounds and is not on the outer wall
            if (newX > 0 && newX < roomWidth - 1 && newY > 0 && newY < roomHeight - 1)
            {
                var newPosition = (newX, newY);

                // If the selected position is a floor, convert it to a wall
                if (room[newPosition] is StoneFloorBlock)
                {
                    room[newPosition] = new StoneWallBlock();
                    wallCount--;
                }

                // Move to the new position
                currentX = newX;
                currentY = newY;
            }
        }

        return room;
    }

    public Dictionary<(int x, int y), Block> GenerateBasicRoom()
    {
        Random rnd = new Random();
        int minRoomWidth = GameConfig.MinRoomWidth;
        int maxRoomWidth = GameConfig.MaxRoomWidth;
        int minRoomHeight = GameConfig.MinRoomWidth;
        int maxRoomHeight = GameConfig.MaxRoomWidth;
        int roomWidth = rnd.Next(minRoomWidth, maxRoomWidth);
        int roomHeight = rnd.Next(minRoomHeight, maxRoomHeight);
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
        return room;
    }

    public Dictionary<(int x, int y), Block> GenerateBasicPond(int roomWidth, int roomHeight)
    {
        Random rnd = new Random();
        int minPondWidth = Math.Min(GameConfig.MinPondWidth, roomWidth / 2);
        int maxPondWidth = Math.Min(GameConfig.MaxPondWidth, roomWidth / 2);
        int minPondHeight = Math.Min(GameConfig.MinPondWidth, roomHeight / 2);
        int maxPondHeight = Math.Min(GameConfig.MaxPondWidth, roomHeight / 2);
        int pondWidth = rnd.Next(minPondWidth, maxPondWidth);
        int pondHeight = rnd.Next(minPondHeight, maxPondHeight);
        Dictionary<(int x, int y), Block> pond = new();
        for (int x = 0; x < pondWidth; x++)
        {
            for (int y = 0; y < pondHeight; y++)
            {

                pond.Add((x, y), new WaterBlock());


            }
        }
        return pond;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="minRoomWidth"></param>
    /// <param name="maxRoomWidth"></param>
    /// <param name="minRoomHeight"></param>
    /// <param name="maxRoomHeight"></param>
    /// <returns></returns>
    public Dictionary<(int x, int y), Block> GenerateRoom(int totalNumberOfRooms, int currentRoomCount)
    {
        try
        {

            
            Random random = new Random();
            
            int maximumNumberOfPossibleDoors = totalNumberOfRooms*2 - DoorCount - 2;
            int maxNumberOfDoorsInThisRoom = Math.Min(maximumNumberOfPossibleDoors, GameConfig.MaxDoorsInRoom);
            int numberofdoorsinthisroom = 0;
            if(currentRoomCount == totalNumberOfRooms || currentRoomCount == 1)
            {
                numberofdoorsinthisroom = 1;
            }
            else
            {

                if (maxNumberOfDoorsInThisRoom < GameConfig.MinDoorsInRoom)
                {
                    numberofdoorsinthisroom = maxNumberOfDoorsInThisRoom;
                }
                else
                {
                    int minDoors = GameConfig.MinDoorsInRoom;
                    if(currentRoomCount > 1)
                    {
                        minDoors =  Math.Max(2, GameConfig.MinDoorsInRoom);
                    }
                    
                    numberofdoorsinthisroom = random.Next(minDoors, maxNumberOfDoorsInThisRoom);
                }

            }



            Dictionary<(int x, int y), Block> room = GenerateRoomCornerStrat();


            var wallBlocksAndCoordinates = room.Where(x => x.Value is StoneWallBlock).ToList();

            while (numberofdoorsinthisroom > 0)
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
                if (blockLeft is WallBlock && blockRight is WallBlock && blockAbove == null && TopDoorCount <= BottomDoorCount && LeftDoorCount == RightDoorCount) // door upwards
                {
                    Entrance entrance = new TopEntrance();

                    entrance.EntranceId = TopDoorCount;

                    entrance.CorrespondingRoomId = DoorCount + 1;
                    if (TopDoorCount < BottomDoorCount)
                    {
                        entrance.CorrespondingRoomId = DoorCount - 1;
                    }
                    room.Remove((selectedCoords.x, selectedCoords.y));
                    room.Add((selectedCoords.x, selectedCoords.y), entrance);
                    TopDoorCount++;
                    numberofdoorsinthisroom--;
                    DoorCount++;
                }
                else if (blockLeft is WallBlock && blockRight is WallBlock && blockBelow == null && BottomDoorCount <= TopDoorCount && LeftDoorCount == RightDoorCount) // door below
                {

                    Entrance entrance = new BottomEntrance();

                    entrance.EntranceId = BottomDoorCount;
                    entrance.CorrespondingRoomId = DoorCount + 1;
                    if (TopDoorCount > BottomDoorCount)
                    {
                        entrance.CorrespondingRoomId = DoorCount - 1;
                    }
                    room.Remove((selectedCoords.x, selectedCoords.y));
                    room.Add((selectedCoords.x, selectedCoords.y), entrance);
                    BottomDoorCount++;
                    numberofdoorsinthisroom--;
                    DoorCount++;
                }
                else if (blockAbove is WallBlock && blockBelow is WallBlock && blockRight == null && RightDoorCount <= LeftDoorCount && TopDoorCount == BottomDoorCount) // door to right 
                {
                    Entrance entrance = new RightEntrance();

                    entrance.CorrespondingRoomId = DoorCount + 1;
                    if (RightDoorCount < LeftDoorCount)
                    {
                        entrance.CorrespondingRoomId = DoorCount - 1;
                    }
                    entrance.EntranceId = RightDoorCount;
                    room.Remove((selectedCoords.x, selectedCoords.y));
                    room.Add((selectedCoords.x, selectedCoords.y), entrance);
                    numberofdoorsinthisroom--;
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
                        entrance.CorrespondingRoomId = DoorCount - 1;
                    }
                    room.Remove((selectedCoords.x, selectedCoords.y));
                    room.Add((selectedCoords.x, selectedCoords.y), entrance);
                    numberofdoorsinthisroom--;
                    LeftDoorCount++;
                    DoorCount++;
                }
            }


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


    public Dictionary<(int x, int y), Block> RoomToGrid(Dictionary<(int x, int y), Block> room)
    {
        Dictionary<(int x, int y), Block> gridGenerated = new Dictionary<(int x, int y), Block>();
        try
        {
            int roomHeight = GetRoomHeight(room);
            int roomWidth = GetRoomWidth(room);

            int roomStartXPosition = (GameConfig.VisibleGridWidth / 2) - roomWidth / 2;
            int roomStartYPosition = (GameConfig.VisibleGridWidth / 2) - roomHeight / 2;

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
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
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
                    if (blockToLeftName == "blank" || blockAboveName == "blank" || blockToRightName == "blank" || blockBelowName == "blank" || blockToTopLeftName == "blank" || blockToTopRightName == "blank" || blockToBottomRightName == "blank" || blockToBottomLeftName == "blank")
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
            return currentGrid[(x, y + 1)].Name;
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
    public Block GetBlockAbove(int x, int y)
    {
        if (y > 0 && currentGrid.ContainsKey((x, y + 1)))
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

    public Block GetBlockToRight(int x, int y)
    {
        if (x < gridSize - 1 && y < gridSize - 1 && currentGrid.ContainsKey((x + 1, y)))
        {
            return currentGrid[(x + 1, y + 1)];
        }
        else { return null; }
    }

    public Block GetBlockToLeft(int x, int y)
    {
        if (x > 0 && y < gridSize - 1 && currentGrid.ContainsKey((x - 1, y)))
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



    public async Task<bool> CheckAndApplyItem()
    {
        var currentBlock = currentGrid[user.UserCoordinates];
        if (currentBlock.ContainsItem)
        {
            user.Inventory.Add(currentBlock.item);
            await SendMessage("battleDialog", $"You found an {currentBlock.item.Name}");
            currentGrid[user.UserCoordinates].item = null;
            currentGrid[user.UserCoordinates].ContainsItem = false;
            currentGrid[user.UserCoordinates].Image = currentGrid[user.UserCoordinates].DefaultImage;

            await SendMessage("grid", currentGrid);
            await SendMessage("user", user);
            return true;
        }
        else
        {
            return false;
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
            if (entrance.CorrespondingRoomId == null || entrance.CorrespondingRoomId == -1)
            {
                entrance.CorrespondingRoomId = currentGridCount++;
            }
            
            user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingupblock.png";
            WalkThroughDoor(grids, typeof(BottomEntrance),entrance.CorrespondingRoomId);
            //SetUserCoordinatesBasedOnGrid(currentGrid, typeof(BottomEntrance));
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
            currentBlock = currentGrid[user.UserCoordinates];
            if (await CheckAndApplyItem() == false)
            {
                await ScapeMonsterEncounter();
            }


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
            
            WalkThroughDoor(grids, typeof(TopEntrance), entrance.CorrespondingRoomId);
            //SetUserCoordinatesBasedOnGrid(currentGrid, typeof(TopEntrance));
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

            currentBlock = currentGrid[user.UserCoordinates];
            if (await CheckAndApplyItem() == false)
            {
                await ScapeMonsterEncounter();
            }



        }
        user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingdownblock.png";
        await SendMessage("user", user);

    }

    
    public async Task MoveLeft()
    {
        Block blockLeft = currentGrid[(user.UserCoordinates.x - 1, user.UserCoordinates.y)];
        Block currentBlock = currentGrid[user.UserCoordinates];
        if (currentBlock is LeftEntrance)
        {
            LeftEntrance entrance = (LeftEntrance)currentBlock;
            entrance.EntranceId = currentGridCount;
            if (entrance.CorrespondingRoomId == null || entrance.CorrespondingRoomId == -1)
            {
                entrance.CorrespondingRoomId = currentGridCount++;
            }
          
            user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingleftblock.png";
            WalkThroughDoor(grids, typeof(RightEntrance), entrance.CorrespondingRoomId);
            //SetUserCoordinatesBasedOnGrid(currentGrid, typeof(RightEntrance));
            await SendMessage("grid", currentGrid);
            await SendMessage("user", user);
            return;

        }
        if (blockLeft.CanPass == true)
        {
            var coords = user.UserCoordinates;
            coords.x -= 1;
            user.UserCoordinates = coords;
            currentBlock = currentGrid[user.UserCoordinates];
            if (await CheckAndApplyItem() == false)
            {
                await ScapeMonsterEncounter();
            }

        }
        user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingleftblock.png";
        await SendMessage("user", user);

    }
    public async Task MoveRight()
    {
        Block blockRight = currentGrid[(user.UserCoordinates.x + 1, user.UserCoordinates.y)];
        Block currentBlock = currentGrid[user.UserCoordinates];
        if (currentBlock is RightEntrance)
        {
            RightEntrance entrance = (RightEntrance)currentBlock;
            entrance.EntranceId = currentGridCount;
            if (entrance.CorrespondingRoomId == null || entrance.CorrespondingRoomId == -1)
            {
                entrance.CorrespondingRoomId = currentGridCount++;
            }
           
            user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingrightblock.png";
            //SetUserCoordinatesBasedOnGrid(currentGrid, typeof(LeftEntrance));
            WalkThroughDoor(grids, typeof(LeftEntrance), entrance.CorrespondingRoomId);
            await SendMessage("grid", currentGrid);
            await SendMessage("user", user);
            return;

        }
        if (blockRight.CanPass == true)
        {
            var coords = user.UserCoordinates;
            coords.x += 1; user.UserCoordinates = coords;
            user.UserCoordinates = coords;
            currentBlock = currentGrid[user.UserCoordinates];
            if (await CheckAndApplyItem() == false)
            {
                await ScapeMonsterEncounter();
            }

        }
        user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingrightblock.png";
        await SendMessage("user", user);
    }
    public void WalkThroughDoor(List<Dictionary<(int x, int y), Block>> grids, Type entranceType, int correspondingRoomId)
    {
        
        if(correspondingRoomId > grids.Count - 1)
        {
            correspondingRoomId = 0;
        }
        currentGrid = grids[correspondingRoomId];
       
        while (SetUserCoordinatesBasedOnGrid(currentGrid, entranceType) == false)
        {
            if (correspondingRoomId > grids.Count - 1)
            {
                correspondingRoomId = 0;
            }
            correspondingRoomId++;
            currentGrid = grids[correspondingRoomId];
        }

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

