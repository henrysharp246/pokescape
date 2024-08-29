﻿using Newtonsoft.Json;
using PokescapeServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using PokescapeServer;
using System.Net.Http.Headers;




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
            var newGrid = CreateGrid();

            grids.Add(newGrid);
        }
        currentGrid = grids.First();
        SetUserCoordinatesBasedOnGrid(currentGrid);



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


        switch (message.MessageType.ToUpper())
        {
            case "MOVE_UP": //here based on the message type, we add messages to the messagesToSend list based on how we respond
                if (user.InBattle == false) {
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
                } else if (user.InBattle == false && user.ItemSelectedId != null)
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


        }

    }

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
    public async Task BattleWon(Battle battle)
    {
        await SendMessage("battleDialog", $"You defeated {battle.OpponentScapeMonster.ScapeMonsterName}! They will be added to your collection.");
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
        catch (Exception ex) {
            Console.WriteLine(ex.ToString());

        }


    }
    public async Task MonsterSelectedForBattle(string monsterId)
    {
        user.CurrentBattle.UserScapeMonster = user.ScapeMonsters.First(monster => monster.ScapeMonsterID == monsterId);
        await SendMessage("battleDialog", user.CurrentBattle.UserScapeMonster.ScapeMonsterName + " chosen!");
        await SendMessage("battle", user.CurrentBattle);
    }

    public async Task ScapeMonsterEncounter()
    {
        return;
        Random random = new Random();
        double num = random.NextDouble();
        if (num < GameConfig.ProbabilityOfScapemonster)
        {
            Battle newBattle = new Battle();
            newBattle.OpponentScapeMonster = ScapeMonster.GetRandomScapeMonster();
            user.CurrentBattle = newBattle;
            user.InBattle = true;
            await SendMessage("battleDialog", "Choose Your Scapemonster...");
            await SendMessage("newBattle", newBattle);
            user.IsTurn = true;
            // user.ScapeMonsters.Add(ScapeMonster.GetRandomScapeMonster());
        }
        return;
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
        while (true) //NEED TO REMOVE
        {

        
            try
            {
                var room = GenerateRoom();
                var finalGrid = RoomToGrid(room);

                return finalGrid;


            }
            catch(Exception ex)
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
        catch(Exception ex)
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
            foreach (var coordsAndBlock in pond) {
              
                newRoom[(coordsAndBlock.Key.x + x, coordsAndBlock.Key.y + y)] = pond[(coordsAndBlock.Key.x, coordsAndBlock.Key.y)]; 
                if (room[(coordsAndBlock.Key.x + x, coordsAndBlock.Key.y + y)] is BlankBlock || room[(coordsAndBlock.Key.x + x, coordsAndBlock.Key.y + y)] == null)
                {
                    return false;
                }
            }
            int numberOfWallBlocksNow = newRoom.Count(block => block.Value is WallBlock);
            if(numberOfWallBlocksNow != numberOfOriginalWallBlocks )
            {
                return false;
            }
            return true;
        }
        catch (Exception ex) { 
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

            for (int x = roomWidth - randomX; x < roomWidth-1; x++)
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
                for (int y = roomHeight - 1; y > roomHeight-1 - randomY; y--)
                {
                    room[(x, y)] = new BlankBlock();
                }
            }



            for (int x = roomWidth - randomX; x < roomWidth - 1; x++)
            {
                for (int y = roomHeight - 1; y > roomHeight-1 - randomY; y--)
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
            for (int x=0; x<roomWidth; x++)
            {
                for(int y=0; y<roomHeight; y++)
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
        catch (Exception ex) {
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
        int minPondWidth = Math.Min(GameConfig.MinPondWidth, roomWidth/2);
        int maxPondWidth = Math.Min(GameConfig.MaxPondWidth, roomWidth/2);
        int minPondHeight = Math.Min(GameConfig.MinPondWidth, roomHeight/2);
        int maxPondHeight = Math.Min(GameConfig.MaxPondWidth, roomHeight/2);
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
    public Dictionary<(int x, int y), Block> GenerateRoom()
    {
        try
        {
          

            Random random = new Random();

            int numberofdoors = random.Next(GameConfig.MinDoorsInRoom, GameConfig.MaxDoorsInRoom);



            Dictionary<(int x, int y), Block> room = GenerateRoomCornerStrat();


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
        if (x > 0 && y < gridSize - 1 && currentGrid.ContainsKey((x - 1, y )))
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
            currentBlock = currentGrid[user.UserCoordinates];
            if (await CheckAndApplyItem() == false)
            {
                await ScapeMonsterEncounter();
            }

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

