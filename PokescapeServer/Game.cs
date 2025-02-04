using Newtonsoft.Json;
using PokescapeServer;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;


//ALL THE MAIN LOGIC IS DONE INSIDE THE GAME CLASS
//There is one game per user... i.e. if 10 users are connected there are 10 games

public class Game
{
    public int gridSize = GameConfig.VisibleGridWidth * GameConfig.VisibleGridWidth;
    public int gridWidth = GameConfig.VisibleGridWidth;
    private string GameId;
    private string gameState;
    private string seed;

    private User user;
    private Dictionary<(int x, int y), Block> currentGrid;
    private List<Dictionary<(int x, int y), Block>> grids;

    private WebSocket currentSocket;

    public static int ChestIndex = 1;
    public static int KeyIndex = 1;

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
            var newGrid = CreateGrid(grids);
            if (newGrid!=null)
                grids.Add(newGrid);
        }
        currentGrid = grids.First();
        SetUserCoordinatesBasedOnGrid(currentGrid);
        user.ScapeMonsters.Add(new Fuzzy(3));
        await SendMessage("grid", currentGrid);
        await SendMessage("user", user);
    }

    public async Task LoadGame(String gameString)
    {
        user.ScapeMonsters.Clear();
        var newGame = JsonConvert.DeserializeObject<PokescapeServer.SerializedGame>(gameString, SerializedGame.gameSerializerSettings);
        this.user = newGame.user;

        this.grids = new List<Dictionary<(int, int), Block>>();

        // create a lookup so we can connect up the entrances
        Dictionary<string, Entrance> entrances = new Dictionary<string, Entrance>();
        
        foreach (var grid in newGame.grids)
        {
            Dictionary<(int, int), Block> gridDict = new Dictionary<(int, int), Block>();
            foreach (var kvp in grid)
            {
                var keyParts = kvp.Key.Split(',');
                var key = (int.Parse(keyParts[0]), int.Parse(keyParts[1]));
                gridDict[key] = kvp.Value;

                if (kvp.Value is Entrance entrance)  // pattern recogn
                    entrances.Add(entrance.EntranceId, entrance);

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

        // now connect up the entrances
        // Now connect up the entrances
        foreach (var entrance in entrances.Values)
        {
            if (!string.IsNullOrEmpty(entrance.CorrespondingEntranceId) &&
                entrances.TryGetValue(entrance.CorrespondingEntranceId, out var correspondingEntrance))
            {
                entrance.CorrespondingEntrance = correspondingEntrance;
            }
            else
            {
                Console.WriteLine($"Cannot find corresponding entrance {entrance.CorrespondingEntranceId ?? "NULL"}");
                entrance.CorrespondingEntrance = entrances.Values.FirstOrDefault(); // Fallback
                entrance.CorrespondingEntranceId = entrance.CorrespondingEntrance.EntranceId; // Fallback
            }
        }


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
    /// This is called when the user has performed an action in the Game. The browser converts this 
    /// action to a message of a given type, that is handled here via the large switch statement.
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
            case "MONSTER_SELECTED":
                user.MonsterSelectedId = message.Data;
                if (user.InBattle)
                {
                    await MonsterSelectedForBattle(message.Data);
                }
                else 
                {
                    if (user.ItemSelectedId != null)
                    {
                        await ApplyItemToMonster(user.ItemSelectedId, message.Data);
                    }
                    await SendMessage("user", user);
                }
                break;
            case "ATTACK_MOVE":
                

                if (user.InBattle && user.IsTurn )
                {
                   if (user.CurrentBattle.UserScapeMonster.Health<=0)
                    {

                        await SendMessage("battleDialog", $"{user.CurrentBattle.UserScapeMonster.ScapeMonsterName} can't fight! Choose another ScapeMonster.");
                    }
                    else
                    {
                        await Attack(message.Data);
                    }
                    
                }
                break;
            case "RENAME":
            {
                    user.RenameScapeMonster(user.MonsterSelectedId, message.Data);
                    await SendMessage("user", user);
                }
                break;
            case "DISCARD_MOVE":

                    user.ItemSelectedId = null;
                
                    user.RemoveMove(message.Data);
                    await SendMessage("user", user);

                break;

            case "DISCARD_SCAPEMONSTER":
                {

                    if (user.MonsterSelectedId != null && user.InBattle == false)
                    {
                        user.DiscardScapeMonster();
                        await SendMessage("user", user);
                    }
                    user.MonsterSelectedId = null;
                    
                }
                break;

            case "ITEM_SELECTED":
                {
                    await ItemSelected(message.Data);
                }
                break;
            case "FEED_OPPONENT": 
                {
                    if (user.ScapeMonsters.Count == 3)
                    {
                        await SendMessage("battleDialog", $"You can't tame the {user.CurrentBattle.OpponentScapeMonster.ScapeMonsterName}! You already have 3 ScapeMonsters.");

                    }
                    else
                    {
                        await FeedOpponent(user.ItemSelectedId, user);
                    }
                }
                break;
            case "EXITBATTLE":
                {

                    if (!user.InBattle)
                    {
                        await SendMessage("battleDialog", $"You aren't in a battle!");
                        break;
                    }
                    if (!user.ExitAttempt)
                    {
                        if (user.CanExit = true)
                        {

                            ExitBattle(user.CurrentBattle);

                        }
                        else
                        {
                            await SendMessage("battleDialog", $"You can't escape!");
                        }
                    }
                    else
                    {
                        await SendMessage("battleDialog", $"You can't escape!");
                    }
                    user.ExitAttempt = true;
                }
                break;

            case "SAVE_GAME":
                if (!user.InBattle)
                {
                    await SaveGameV1();
                }
                break;
            case "LOAD_GAME":
                
                await LoadGame(message.Data);

                break;

        }

    }
    public async Task SaveGameV1()
    {
        var  gameToSave = new SerializedGame();
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

        await SendMessage("save_game", JsonConvert.SerializeObject(gameToSave, SerializedGame.gameSerializerSettings));
    }

    public async Task ItemSelected(string itemId)
    {
        user.ItemSelectedId = itemId;
    }

    public async Task ApplyItemToMonster(string itemId, string monsterId)
    {
        if (user.TryGetValueFromInventory(itemId, out Item item))
        {
            ScapeMonster scapemonster = user.ScapeMonsters.First(y => y.ScapeMonsterID == monsterId);
            ScapeMonster newscapemonster = item.UseItem(scapemonster);
            if (newscapemonster != null) // only applying the item if the item can be applied
            {
                scapemonster.UpdateScapeMonsterStats(scapemonster);
                user.ScapeMonsters.RemoveAll(y => y.ScapeMonsterID == monsterId);
                user.ScapeMonsters.Add(scapemonster);
                user.RemoveItemFromInventory(itemId);
                user.ItemSelectedId = null;
            }
        }
        await SendMessage("user", user);

    }
    public async Task FeedOpponent(string itemId, User user)
    {
        if (user.TryGetValueFromInventory(itemId, out Item item))
        {
            if (item is BerryItem)
            {
                ScapeMonster opponent = user.CurrentBattle.OpponentScapeMonster;
                if (item == null)
                {
                    throw new InvalidOperationException("Item not found in inventory.");
                }

                // Retrieve the opponent ScapeMonster
                user.RemoveItemFromInventory(itemId); // berry gets used in all cases
                // Calculate P_Base
                double P_Base = 1.0;

                // Calculate F_Level
                double F_Level = Math.Pow(0.95, Convert.ToDouble(opponent.Level));

                // Calculate F_Health
                double F_Health = 1.0 - (opponent.Health / opponent.MaximumHealth);

                // Ensure F_Health is within valid bounds
                F_Health = Math.Clamp(F_Health, 0.0, 1.0);

                // Calculate taming probability
                double P_Tame = P_Base * F_Level * F_Health;

                // Ensure P_Tame is within bounds (0 to 1)
                P_Tame = Math.Clamp(P_Tame, 0.0, 1.0);

                // Generate a random number and determine success
                Random random = new Random();
                double randomValue = random.NextDouble();
                bool isTamed = randomValue <= P_Tame;
                if (isTamed == true)
                {
                    await TameSuccess(user.CurrentBattle); // this will notify user they tamed the monster
                    return;
                }
                else
                {
                    await SendMessage("battleDialog", $"You fed the {user.CurrentBattle.OpponentScapeMonster.ScapeMonsterName}.");
                    await SendMessage("user", user);
                    return;
                }
            }
            else
            {
                await SendMessage("battleDialog", $"You can't feed the opponent that {item.Name}!");
            }
        }
       
    }

    static bool IsEven(int number)
    {
        return number % 2 == 0;
    }
    public async Task BattleWon(Battle battle)
    {
        await SendMessage("battleDialog", $"You defeated {battle.OpponentScapeMonster.ScapeMonsterName}!");
        Thread.Sleep(500);
        if (IsEven(battlecount))
        {

            for (int i = 0; i < user.ScapeMonsters.Count; i++)
            {
                if (user.ScapeMonsters[i].ScapeMonsterID == battle.UserScapeMonster.ScapeMonsterID)
                {
                    // Level up the user’s active monster
                    var updatedMonster = ScapeMonster.Levelup(battle.UserScapeMonster);

                    // Update the monster in the user’s collection
                    user.ScapeMonsters[i] = updatedMonster;

                    // Also update the monster currently associated with the battle
                    battle.UserScapeMonster = updatedMonster;

                    break; // We found and updated the correct monster, so we can exit the loop
                }
            }


            await SendMessage("battleDialog", $"{battle.UserScapeMonster.ScapeMonsterName} levelled up to level {battle.UserScapeMonster.Level}!");
        }

        Thread.Sleep(1500);

     
        user.CurrentBattle = null;
        user.InBattle = false;

        user.BattleCount++;
        if (user.BattleCount%3 == 0)
        {
            foreach(ScapeMonster scapemonster in user.ScapeMonsters)
            {
                scapemonster.Health = scapemonster.MaximumHealth;
            }
        }
        await SendMessage("user", user);
        await SendMessage("hideBattle", $"");
    }
    public async Task BattleLost(Battle battle)
    {
        await SendMessage("battleDialog", $"You lost vs {battle.OpponentScapeMonster.ScapeMonsterName}!");
        Thread.Sleep(2500);
        user.CurrentBattle = null;
        user.InBattle = false;
        user.BattleCount++;
        if (user.BattleCount % 3 == 0)
        {
            foreach (ScapeMonster scapemonster in user.ScapeMonsters)
            {
                scapemonster.Health = scapemonster.MaximumHealth;
            }
        }
        await SendMessage("user", user);
        await SendMessage("hideBattle", $"");
    }
    public async Task TameSuccess(Battle battle)
    {
        await SendMessage("battleDialog", $"You tamed {battle.OpponentScapeMonster.ScapeMonsterName}! They will be added to your collection.");
        Thread.Sleep(500);
        if (IsEven(battlecount))
        {
            battle.UserScapeMonster.Level += 1;
            await SendMessage("battleDialog", $"{battle.UserScapeMonster.ScapeMonsterName} levelled up to level {battle.UserScapeMonster.Level}!");
        }

        Thread.Sleep(1500);

        user.ScapeMonsters.Add(battle.OpponentScapeMonster);
        user.CurrentBattle = null;
        user.InBattle = false;
        await SendMessage("user", user);
        await SendMessage("hideBattle", $"");
    }

    public async Task ExitBattle(Battle battle)
    {
        Random random = new Random();
        int number = random.Next(1, 4);
        if (number != 1)
        {
            await SendMessage("battleDialog", $"You escaped!");
            Thread.Sleep(1000);
            user.CurrentBattle = null;
            user.InBattle = false;
            await SendMessage("user", user);
            await SendMessage("hideBattle", $"");
            return;
        }
        else
        {
            user.CanExit = false;
            await SendMessage("battleDialog", $"You can't escape!");
            return;
        }
    }

    public async Task Attack(string moveId)
    {
        Battle battle = user.CurrentBattle;
        try
        {
            if (user.CurrentBattle.UserCooldowns == null)
            {
                user.CurrentBattle.UserCooldowns = new Dictionary<ScapeMonsterMove, int>();
            }
            ScapeMonsterMove move = ScapeMonsterMove.GetMoveById(moveId);

            //ScapeMonsterMove activeCooldownMove = move; 

            //activeCooldownMove.activeCooldown = true;

            // to do : add a list of moves with active cooldown meaning the user cant play them, one list for user one to opponent

            //Check cooldowns

            if (user.CurrentBattle.UserCooldowns != null)
            {
                foreach (var kvp in user.CurrentBattle.UserCooldowns)
                {
                    if (kvp.Key == move)
                    {
                        if (kvp.Value != 0)
                        {
                            await SendMessage("battleDialog", $"You can't play that {move.MoveName} for another {kvp.Value} rounds.\nChoose another move.");
                            await Task.Delay(200);
                            return;
                        }
                    }
                }
            }

            
            if (move.Cooldown != 0)
            {
                user.CurrentBattle.UserCooldowns.Add(move, move.Cooldown+1);
            }

            

            var currentOpponent = user.CurrentBattle.OpponentScapeMonster;
            var userScapeMonster = user.CurrentBattle.UserScapeMonster;
            MoveResult moveResult = ScapeMonsterMove.PerformMove(move ,currentOpponent, userScapeMonster, user.CurrentBattle.ActiveUserMoves, user.CurrentBattle.ActiveOpponentMoves);

            currentOpponent.Health -= moveResult.damage;  // apply damage
            currentOpponent.Health -= moveResult.extradamage; // apply additional damage from special moves
            if (moveResult.hpChange != 0)
            {
                userScapeMonster.Health += moveResult.hpChange;
            }
            await SendMessage("user", user);

            // USER ATTACK

            if (currentOpponent.Health <= 0)
            { currentOpponent.Health = 0; }


            await SendMessage("battle", user.CurrentBattle);
            await SendMessage("battleDialog", moveResult.description);

            await Task.Delay(200);

            if (moveResult.description2 != "")
            { await SendMessage("battleDialog", moveResult.description2); }
            await Task.Delay(200);


            //            await SendMessage("battleDialog", $"{user.CurrentBattle.UserScapeMonster.ScapeMonsterName} used move {move.MoveName}, and performed {move.MoveDamage} damage");
            await SendMessage("battle", user.CurrentBattle);
            await Task.Delay(300);

            if (currentOpponent.Health <= 0)
            {
                await BattleWon(user.CurrentBattle);
                return;
            }

            //OPPONENT ATTACK
            
            var opponentMove = currentOpponent.GetRandomMove();

            MoveResult opponentMoveResult = ScapeMonsterMove.PerformMove(opponentMove ,userScapeMonster, currentOpponent, user.CurrentBattle.ActiveOpponentMoves, user.CurrentBattle.ActiveUserMoves); // Switched for the opponents attack

            // Check cooldowns
            if (user.CurrentBattle.OpponentCooldowns != null)
            {
                foreach (var kvp in user.CurrentBattle.OpponentCooldowns)
                {
                    if (kvp.Key == move)
                    {
                        if (kvp.Value != 0)
                        {
                            
                            await Task.Delay(50);
                            return;
                        }
                    }
                }
            }
           
            userScapeMonster.Health += -opponentMoveResult.damage;
            if (opponentMoveResult.hpChange != 0)
            {  
                currentOpponent.Health += moveResult.hpChange;
            }

            if (userScapeMonster.Health <= 0)
            { 
                userScapeMonster.Health = 0; 
            }
            await SendMessage("battleDialog", opponentMoveResult.description);
            await Task.Delay(300);
            if (opponentMoveResult.description2 != "")
            { await SendMessage("battleDialog", opponentMoveResult.description2); }
            
            await Task.Delay(300);
            if (user.CurrentBattle.UserCooldowns != null)
            {
                foreach (var key in user.CurrentBattle.UserCooldowns.Keys.ToList()) // Use ToList to avoid modifying the collection during iteration
                {
                    user.CurrentBattle.UserCooldowns[key]--;  // Decrease the value for the key

                    if (user.CurrentBattle.UserCooldowns[key] <= 0)
                    {
                        await SendMessage("battleDialog", $"The effects of your {key.MoveName} wore off.");
                        user.CurrentBattle.UserCooldowns.Remove(key); // Remove the key if its value is <= 0
                    }
                }
            }
            await Task.Delay(400);
            if (user.CurrentBattle.OpponentCooldowns != null)
            {
                foreach (var key in user.CurrentBattle.OpponentCooldowns.Keys.ToList()) // Use ToList to avoid modifying the collection during iteration
                {
                    user.CurrentBattle.OpponentCooldowns[key]--; // Decrease the value for the key

                    if (user.CurrentBattle.OpponentCooldowns[key] <= 0)
                    {
                        await SendMessage("battleDialog", $"The effects of the opponents {key.MoveName} wore off.");
                        user.CurrentBattle.OpponentCooldowns.Remove(key); // Remove the key if its value is <= 0
                    }
                }
            }

            await SendMessage("battle", user.CurrentBattle);
            if (userScapeMonster.Health <= 0)
            {
                userScapeMonster.Health = 0;
                await SendMessage("battleDialog", $"{userScapeMonster.ScapeMonsterName} fainted!");
                await SendMessage("user", user);

                if (user.ScapeMonsters.Exists(scapeMonster => scapeMonster.Health > 0))
                {
                    await SendMessage("battleDialog", $"Choose another ScapeMonster.");
                }
                else
                {
                    await BattleLost(user.CurrentBattle);
                    return;
                }
            }
            user.CanExit = true;
            user.ExitAttempt = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    public async Task MonsterSelectedForBattle(string monsterId)
    {
        //Check that the selected monster has health more than 0
        ScapeMonster selectedScapeMonster = user.ScapeMonsters.First(monster => monster.ScapeMonsterID == monsterId);
        if (selectedScapeMonster.Health >0)
        {
            user.CurrentBattle.UserScapeMonster = selectedScapeMonster;
            await SendMessage("battleDialog", user.CurrentBattle.UserScapeMonster.ScapeMonsterName + " chosen!");
            await SendMessage("monsterselectedforbattle", user.CurrentBattle.UserScapeMonster);
            Thread.Sleep(400);
            await SendMessage("battleDialog", $"What will {user.CurrentBattle.UserScapeMonster.ScapeMonsterName} do?");
            await SendMessage("battle", user.CurrentBattle);
        }

        else
        {
            await SendMessage("battleDialog", $"{selectedScapeMonster.ScapeMonsterName} can't fight! Choose another ScapeMonster.");
            return;
        }
    }

    static int battlecount = 1;
    public async Task ScapeMonsterEncounter()
    {
        battlecount++;
      
        Random random = new Random();
        int num = random.Next(1, 17);
        if (num < GameConfig.ProbabilityOfScapemonster)
        {
            Battle newBattle = new Battle();
            newBattle.OpponentScapeMonster = ScapeMonster.GetRandomScapeMonster(user);

            user.MonsterSelectedId = null;
            user.CurrentBattle = newBattle;
            user.InBattle = true;
            await SendMessage("battleDialog", "Choose Your Scapemonster...");
            await SendMessage("newBattle", newBattle);
            user.IsTurn = true;
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

    /// <summary>
    /// Calls Generate a room and then converts it to a ScreenLayout (grid)
    /// </summary>
    /// <param name="rooms"></param>
    /// <returns></returns>
    public Dictionary<(int x, int y), Block> CreateGrid(List<Dictionary<(int x, int y), Block>> rooms)
    {
        try
        {
            var room = GenerateRoom(rooms);
            var finalGrid = RoomToGrid(room);
            return finalGrid;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
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
            while (!pondPlaced) // todo: prevent infinite loop
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

            foreach (var coordsAndBlock in pond)
            {
                if (!room.TryGetValue((coordsAndBlock.Key.x + x, coordsAndBlock.Key.y + y), out var blk)  || blk is BlankBlock || blk is WallBlock)
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
        int randomX = rnd.Next(GameConfig.MinDecompositionOfPondCorners, Math.Max(GameConfig.MinDecompositionOfPondCorners, pondWidth / 2 - 1));
        int randomY = rnd.Next(GameConfig.MinDecompositionOfPondCorners, Math.Max(GameConfig.MinDecompositionOfPondCorners, pondHeight / 2 - 1));
        for (int x = 0; x < randomX; x++)
        {
            for (int y = 0; y < randomY; y++)
            {
                pond[(x, y)] = new StoneFloorBlock();
            }
        }

        //bottom right corner processing
        randomX = rnd.Next(GameConfig.MinDecompositionOfPondCorners, Math.Max(GameConfig.MinDecompositionOfPondCorners, pondWidth / 2 - 1));
        randomY = rnd.Next(GameConfig.MinDecompositionOfPondCorners, Math.Max(GameConfig.MinDecompositionOfPondCorners, pondHeight / 2 - 1));


        for (int x = pondWidth - randomX; x < pondWidth - 1; x++)
        {
            for (int y = 0; y < randomY; y++)
            {
                pond[(x, y)] = new StoneFloorBlock();
            }
        }
        //top right corner processing
        randomX = rnd.Next(GameConfig.MinDecompositionOfPondCorners, Math.Max(GameConfig.MinDecompositionOfPondCorners, pondWidth / 2 - 1));
        randomY = rnd.Next(GameConfig.MinDecompositionOfPondCorners, Math.Max(GameConfig.MinDecompositionOfPondCorners, pondHeight / 2 - 1));

        for (int x = pondWidth - randomX; x < pondWidth - 1; x++)
        {
            for (int y = pondHeight - 1; y > pondHeight - 1 - randomY; y--)
            {
                pond[(x, y)] = new StoneFloorBlock();
            }
        }

        //top left corner processing
        randomX = rnd.Next(GameConfig.MinDecompositionOfPondCorners, Math.Max(GameConfig.MinDecompositionOfPondCorners, pondWidth / 2 - 1));
        randomY = rnd.Next(GameConfig.MinDecompositionOfPondCorners, Math.Max(GameConfig.MinDecompositionOfPondCorners, pondHeight / 2 - 1));

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
            int randomX = rnd.Next(minDecomposition, Math.Max(minDecomposition, roomWidth / 2 - 1));
            int randomY = rnd.Next(minDecomposition, Math.Max(minDecomposition,roomHeight / 2 - 1));
            int randomMax = Math.Max(randomX, randomY);

            for (int x = 0; x < randomX; x++)
                for (int y = 0; y < randomY; y++)
                    room[(x, y)] = new BlankBlock();

            for (int x = 0; x < randomX; x++)
                for (int y = 0; y < randomY; y++)
                    if (room[(x + 1, y)] is FloorBlock || room[(x, y + 1)] is FloorBlock)
                        room[(x, y)] = new StoneWallBlock();

            //bottom right corner processing
            randomX = rnd.Next(minDecomposition, Math.Max(minDecomposition,roomWidth / 2 - 1));
            randomY = rnd.Next(minDecomposition, Math.Max(minDecomposition, roomHeight / 2 - 1));
            randomMax = Math.Max(randomX, randomY);

            for (int x = roomWidth - randomX; x < roomWidth - 1; x++)
                for (int y = 0; y < randomY; y++)
                    room[(x, y)] = new BlankBlock();

            for (int x = roomWidth - randomX; x < roomWidth - 1; x++)
                for (int y = 0; y < randomY; y++)
                    if (room[(x - 1, y)] is FloorBlock || room[(x, y + 1)] is FloorBlock)
                        room[(x, y)] = new StoneWallBlock();

            //top right corner processing
            randomX = rnd.Next(minDecomposition, Math.Max(minDecomposition, roomWidth / 2 - 1));
            randomY = rnd.Next(minDecomposition, Math.Max(minDecomposition, roomHeight / 2 - 1));
            randomMax = Math.Max(randomX, randomY);

            for (int x = roomWidth - randomX; x < roomWidth - 1; x++)
                for (int y = roomHeight - 1; y > roomHeight - 1 - randomY; y--)
                    room[(x, y)] = new BlankBlock();

            for (int x = roomWidth - randomX; x < roomWidth - 1; x++)
                for (int y = roomHeight - 1; y > roomHeight - 1 - randomY; y--)
                    if (room[(x - 1, y)] is FloorBlock || room[(x, y - 1)] is FloorBlock)
                        room[(x, y)] = new StoneWallBlock();

            //top left corner processing
            randomX = rnd.Next(minDecomposition, Math.Max(minDecomposition, roomWidth / 2 - 1));
            randomY = rnd.Next(minDecomposition, Math.Max(minDecomposition, roomHeight / 2 - 1));
            randomMax = Math.Max(randomX, randomY);

            for (int x = 0; x < randomX; x++)
                for (int y = roomHeight - randomY; y < roomHeight - 1; y++)
                    room[(x, y)] = new BlankBlock();

            for (int x = 0; x < randomX; x++)
                for (int y = roomHeight - randomY; y < roomHeight - 1; y++)
                    if (room[(x + 1, y)] is FloorBlock || room[(x, y - 1)] is FloorBlock)
                        room[(x, y)] = new StoneWallBlock();
            room = AddWaterToRoom(room);
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
/// Attempts to replace a StoneWallBlock with an EntranceBlock subject to accessibility and other constraints.
/// </summary>
/// <param name="room"></param>
/// <param name="correspondingEntrance"></param>
/// <returns></returns>
    public Entrance TryAddDoorToRoom(Dictionary<(int x, int y), Block> room, Entrance correspondingEntrance) // return null if not possible
    {
        var num_doors = room.Where(x => x.Value is Entrance).Count();
        if (num_doors >= GameConfig.MaxDoorsInRoom)
            return null;

        Random random = new Random();
        var wallBlocksAndCoordinates = room.Where(x => x.Value is StoneWallBlock).ToList();
        int count = 0;
        while (count++ < 500)
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

            bool relax = count > 50;

            // only add a top entrance when the number of top entrances >= number of the bottom entrances
            if (blockLeft is WallBlock && blockRight is WallBlock && (blockAbove == null || blockAbove is BlankBlock) && (blockBelow is StoneFloorBlock) && (correspondingEntrance==null  || correspondingEntrance is BottomEntrance) && blockBelow.ContainsItem == false) // door upwards
            {
                Entrance entrance = new TopEntrance() 
                { 
                    CorrespondingEntrance = correspondingEntrance,
                    CorrespondingEntranceId = correspondingEntrance?.EntranceId,
                    Coordinates = selectedCoords,
                };
                room[selectedCoords] = entrance;
                return entrance;
            }
            else if (blockLeft is WallBlock && blockRight is WallBlock && (blockBelow == null || blockBelow is BlankBlock) && (blockAbove is StoneFloorBlock) && (correspondingEntrance == null || correspondingEntrance is TopEntrance) && blockAbove.ContainsItem == false)// door below
            {
                Entrance entrance = new BottomEntrance()
                {
                    CorrespondingEntrance = correspondingEntrance,
                    CorrespondingEntranceId = correspondingEntrance?.EntranceId,
                    Coordinates = selectedCoords,
                };
                room[selectedCoords] = entrance;
                return entrance;
            }
            else if (blockAbove is WallBlock && blockBelow is WallBlock && (blockRight == null || blockRight is BlankBlock) && (blockLeft is StoneFloorBlock) && (correspondingEntrance == null || correspondingEntrance is LeftEntrance) && blockLeft.ContainsItem == false) // door to right 
            {
                Entrance entrance = new RightEntrance()
                {
                    CorrespondingEntrance = correspondingEntrance,
                    CorrespondingEntranceId = correspondingEntrance?.EntranceId,
                    Coordinates = selectedCoords,
                };
                room[selectedCoords] = entrance;
                return entrance;
            }
            else if (blockAbove is WallBlock && blockBelow is WallBlock && (blockLeft == null || blockLeft is BlankBlock) && (blockRight is StoneFloorBlock) && (correspondingEntrance == null || correspondingEntrance is RightEntrance) && blockRight.ContainsItem == false) // door to left
            {
                Entrance entrance = new LeftEntrance()
                {
                    CorrespondingEntrance = correspondingEntrance,
                    CorrespondingEntranceId = correspondingEntrance?.EntranceId,
                    Coordinates = selectedCoords,
                };
                room[selectedCoords] = entrance;
                return entrance;
            }
        }
        Console.WriteLine("Could not add door to room due to door placement issue");
        return null;
    }

    /// <summary>
    /// Generates a new room and attempts to connect it to one of the existing (already generated) rooms
    /// </summary>
    /// <param name="rooms"></param>
    /// <returns></returns>
    public Dictionary<(int x, int y), Block> GenerateRoom(List<Dictionary<(int x, int y), Block>> rooms)
    {
        try
        {
            Random random = new Random();
            int currentRoomCount = rooms.Count();
            Dictionary<(int x, int y), Block> room = GenerateRoomCornerStrat();
            if (currentRoomCount == 0) 
                return room;

            var wallBlocksAndCoordinates = room.Where(x => x.Value is StoneWallBlock).ToList();

            var first_entrance_in_this_room = TryAddDoorToRoom(room, null);
            first_entrance_in_this_room.RoomId = currentRoomCount;

            // todo: check if this fails (returns null)

            int count = 0;
            while (count++ < 500)
            {
                int randomNum = random.Next(0, rooms.Count());
                var previous_room = rooms[randomNum];

                var new_entrance_in_previous_room = TryAddDoorToRoom(previous_room, first_entrance_in_this_room);
                if (new_entrance_in_previous_room!=null)
                {
                    new_entrance_in_previous_room.RoomId = randomNum;
                    first_entrance_in_this_room.CorrespondingEntrance = new_entrance_in_previous_room;
                    first_entrance_in_this_room.CorrespondingEntranceId = new_entrance_in_previous_room.EntranceId;
                    return room;                
                }
            }
            Console.WriteLine("Failed to connect this room to any previous room");
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
        // Find the maximum x coordinate to determine the width of the roomfbattle
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

    /// <summary>
    /// This converts the current room (0,0)->(width,height) by centering it in the visible grid.
    /// Any overflow (if the room is bigger than the visible grid) will not be renderede by the browser/js client
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    public Dictionary<(int x, int y), Block> RoomToGrid(Dictionary<(int x, int y), Block> room)
    {
        Dictionary<(int x, int y), Block> result = new Dictionary<(int x, int y), Block>();
        try
        {
            int roomHeight = GetRoomHeight(room);
            int roomWidth = GetRoomWidth(room);

            int roomStartXPosition = (GameConfig.VisibleGridWidth / 2) - roomWidth / 2;
            int roomStartYPosition = (GameConfig.VisibleGridWidth / 2) - roomHeight / 2;

            foreach (var coordAndBlock in room)
            {
                //translates the room by half of the grid width and height (places the room in the middle of the grid)
                int new_x = (roomStartXPosition + coordAndBlock.Key.x);
                int new_y = (roomStartYPosition + coordAndBlock.Key.y);
                
                if (coordAndBlock.Value is Entrance entrance) // update the coordinates of the entrances from room coordinates to grid coordinates
                {
                    entrance.Coordinates = (new_x, new_y);
                }
                result[(new_x, new_y)] = coordAndBlock.Value;
            }

            // fill the rest of the grid with blank blocks
            for (int x = 0; x < GameConfig.VisibleGridWidth; x++)
            {
                for (int y = 0; y < GameConfig.VisibleGridWidth; y++)
                {
                    if (!result.ContainsKey((x, y)))
                    {
                        Block block = new BlankBlock();
                        result.Add((x, y), block);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

       
        var keys = result.Keys.ToList(); // Copy of the keys

        foreach (var key in keys)
        {
            int x = key.x;
            int y = key.y;

            // Get the current block
            Block currentBlock = result[(x, y)];

            // Check only StoneWallBlocks
            if (currentBlock is StoneWallBlock)
            {
                Console.WriteLine($"1 x {x} y {y} {currentBlock}");

                // Fetch neighbor blocks
                Block blockToLeft = result.ContainsKey((x - 1, y)) ? result[(x - 1, y)] : null;
                Block blockToRight = result.ContainsKey((x + 1, y)) ? result[(x + 1, y)] : null;
                Block blockUp = result.ContainsKey((x, y + 1)) ? result[(x, y + 1)] : null;
                Block blockDown = result.ContainsKey((x, y - 1)) ? result[(x, y - 1)] : null;

                // Check if the wall block touches a floor or water block
                if ((blockUp is StoneFloorBlock || blockUp is WaterBlock) ||
                    (blockDown is StoneFloorBlock || blockDown is WaterBlock) ||
                    (blockToLeft is StoneFloorBlock || blockToLeft is WaterBlock) ||
                    (blockToRight is StoneFloorBlock || blockToRight is WaterBlock))
                {
                    Console.WriteLine($"2 x {x} y {y} {currentBlock}");
                }
                else
                {
                    // Replace isolated wall blocks with BlankBlock
                    result[(x, y)] = new BlankBlock();
                }
            }
        }
        return result;
    }

    public async Task<bool> PickUpItem() // When user picks up item
    {
        var currentBlock = currentGrid[user.UserCoordinates];
        if (currentBlock.ContainsItem)
        {
            if (StartsWithVowel(currentBlock.item.Name))
            {
                await SendMessage("battleDialog", $"You found an {currentBlock.item.Name}!");
            }
            else
            {
                await SendMessage("battleDialog", $"You found a {currentBlock.item.Name}!");
            }
            user.AddItemToInventory(currentBlock.item);
                
            currentGrid[user.UserCoordinates].item = null;
            currentGrid[user.UserCoordinates].ContainsItem = false;
            currentGrid[user.UserCoordinates].Image = currentGrid[user.UserCoordinates].DefaultImage;

            await SendMessage("grid", currentGrid);
            await SendMessage("user", user);
            return true;
        }
        return false;
    }
    public async Task MoveUp()
    {
        Block currentBlock = currentGrid[user.UserCoordinates];
  
        if (currentBlock is TopEntrance)
        {
            TopEntrance entrance = (TopEntrance)currentBlock;
            user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingupblock.png";
            WalkThroughDoor(entrance);
            await SendMessage("grid", currentGrid);
            await SendMessage("user", user);
            return;
        }
        Block blockabove = currentGrid[(user.UserCoordinates.x, user.UserCoordinates.y + 1)];
        bool canPass = blockabove.CanPass || (blockabove is WaterBlock && user.CanPassWater());

        if (canPass == true)
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
            if (await PickUpItem() == false)
            {
                await ScapeMonsterEncounter();
            }
        }

        if (blockabove.item is ChestClosed)
        {
            int x = user.UserCoordinates.x;
            int y = user.UserCoordinates.y + 1;

            currentBlock = blockabove;
            await CheckChest(currentBlock, x, y);
        }
        user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingupblock.png";
        await SendMessage("user", user);
    }
    public async Task MoveDown()
    {
        
        Block currentBlock = currentGrid[user.UserCoordinates];
        if (currentBlock is BottomEntrance)
        {
            BottomEntrance entrance = (BottomEntrance)currentBlock;
            WalkThroughDoor(entrance);
            await SendMessage("grid", currentGrid);
            await SendMessage("user", user);
            user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingdownblock.png";
            return;

        }
        Block blockbelow = currentGrid[(user.UserCoordinates.x, user.UserCoordinates.y - 1)];

        bool canPass = blockbelow.CanPass || (blockbelow is WaterBlock && user.CanPassWater());

        if (canPass == true)
        {
            var coords = user.UserCoordinates;
            coords.y -= 1;
            user.UserCoordinates = coords;

            currentBlock = currentGrid[user.UserCoordinates];
            if (await PickUpItem() == false)
            {
                await ScapeMonsterEncounter();
            }
        }
        if (blockbelow.item is ChestClosed)
        {
            int x = user.UserCoordinates.x;
            int y = user.UserCoordinates.y-1;

            currentBlock = blockbelow;
            await CheckChest(currentBlock, x, y);
        }
        user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingdownblock.png";
        await SendMessage("user", user);
    }
    
    public async Task MoveLeft()
    {
      
        Block currentBlock = currentGrid[user.UserCoordinates];
        if (currentBlock is LeftEntrance)
        {
            LeftEntrance entrance = (LeftEntrance)currentBlock;
            WalkThroughDoor(entrance);
            user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingleftblock.png";
            await SendMessage("grid", currentGrid);
            await SendMessage("user", user);
            return;

        }
        Block blockLeft = currentGrid[(user.UserCoordinates.x - 1, user.UserCoordinates.y)];
        bool canPass = blockLeft.CanPass || (blockLeft is WaterBlock && user.CanPassWater());

        if (canPass == true)
        {
            var coords = user.UserCoordinates;
            coords.x -= 1;
            user.UserCoordinates = coords;
            currentBlock = currentGrid[user.UserCoordinates];
            if (await PickUpItem() == false)
            {
                await ScapeMonsterEncounter();
            }
        }
        if (blockLeft.item is ChestClosed)
        {
            int x = user.UserCoordinates.x - 1;
            int y = user.UserCoordinates.y;

            currentBlock = blockLeft;
            await CheckChest(currentBlock, x, y);
        }

        user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingleftblock.png";
        await SendMessage("user", user);

    }
    public async Task MoveRight()
    {
        Block currentBlock = currentGrid[user.UserCoordinates];

        if (currentBlock is RightEntrance)
        {
            RightEntrance entrance = (RightEntrance)currentBlock;
            WalkThroughDoor(entrance);

            user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingrightblock.png";
            //SetUserCoordinatesBasedOnGrid(currentGrid, typeof(LeftEntrance));
            await SendMessage("grid", currentGrid);
            await SendMessage("user", user);
            return;

        }
        Block blockRight = currentGrid[(user.UserCoordinates.x + 1, user.UserCoordinates.y)];
        
        bool canPass = blockRight.CanPass || (blockRight is WaterBlock && user.CanPassWater());

        if (canPass == true)
        {
            var coords = user.UserCoordinates;
            coords.x += 1; user.UserCoordinates = coords;
            user.UserCoordinates = coords;
            currentBlock = currentGrid[user.UserCoordinates];
            if (await PickUpItem() == false)
            {
                await ScapeMonsterEncounter();
            }

        }

        if (blockRight.item is ChestClosed)
        {
            int x = user.UserCoordinates.x + 1 ;
            int y = user.UserCoordinates.y;

            currentBlock = blockRight;
            await CheckChest(currentBlock, x, y);
        }

        user.UserImage = $"{Pokescape.ImageFolderPath}\\blockImages\\Characterfacingrightblock.png";
        await SendMessage("user", user);
    }
    public bool StartsWithVowel(string name)
    {
        char[] vowels = { 'A', 'E', 'I', 'O', 'U', 'a', 'e', 'i', 'o', 'u' };
        return vowels.Contains(name[0]);
    }
    public async Task CheckChest(Block block, int x, int y)
    {
        foreach (var kvp in user.Inventory)
        {
            if (kvp.Value is Key key)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(key.index);
                sb.Append(' ');
                if (key.index == block.item.index)
                {
                    Item item = Item.GetItemFromChest();
                    user.AddItemToInventory(item);
                    user.Inventory.Remove(kvp.Key);
                    currentGrid[(x, y)].item = null;
                    currentGrid[(x, y)].ContainsItem = false;
                    currentGrid[(x, y)].Image = $"{Pokescape.ImageFolderPath}\\blockImages\\Stonefloorblock.png";
                    currentGrid[(x, y)].CanPass = true;
                    await SendMessage("grid", currentGrid);
                    await SendMessage("user", user);

                    if (StartsWithVowel(kvp.Value.Name))
                    {
                        await SendMessage("battleDialog", $"You found an {item.Name} in the chest!");
                    }
                    else
                    {
                        await SendMessage("battleDialog", $"You found a {item.Name} in the chest!");
                    }
                    return;
                }
            }
        }
        await SendMessage("battleDialog", $"You don't have the right key!");
    }

    public void WalkThroughDoor( Entrance entrance)
    {
        Console.Write($"Room ID: {entrance.RoomId} ({user.UserCoordinates.x},{user.UserCoordinates.y})=>{entrance.CorrespondingEntrance.RoomId} ");
        try
        {
            currentGrid = grids[entrance.CorrespondingEntrance.RoomId];

            user.UserCoordinates = entrance.CorrespondingEntrance.Coordinates;
            Console.Write($"({user.UserCoordinates.x},{user.UserCoordinates.y}) ");

            var block = currentGrid[user.UserCoordinates];
            block.HasUser = true;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }

}

