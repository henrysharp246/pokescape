//THESE ARE IMPORTANT V
const visibleGridSize = 22;
const gridSize = 50;
var blockPercentageWidth = 100 / visibleGridSize;



var discardMoveInProcess = false;
function showInventory() {
    $(".button").removeClass('selected');
    $(".inventoryButton").addClass('selected');
    $("#itemInventory").show();
    $("#scapemonsterInventory").hide();
}
function showScapeMonsters() {
    $(".button").removeClass('selected');
    $(".scapeMonstersButton").addClass('selected');
    $("#scapemonsterInventory").show();
    $("#itemInventory").hide();
}

function closeWelcomeMessage() {
    $("#welcomeMessage").hide();
}
let isFeedOpponentCalled = false; // Tracks if FeedOpponent was called
let itemToFeedOpponent = null;   // Stores the item for FeedOpponent


function exitBattle()
{
    
    const messageToSend = {
        "MessageType": "EXITBATTLE",
        "SocketId": socketId,
        "Data": ''
    };
    console.log('Sending message:', messageToSend);
    connection.send(JSON.stringify(messageToSend));
}
discardMoveInProcess = false;


function selectItem(itemId) {
    discardMoveInProcess = false;

    

    // Original logic for ITEM_SELECTED`
    showScapeMonsters();
    const messageToSend = {
        "MessageType": "ITEM_SELECTED",
        "Data": itemId,
        "SocketId": socketId
    };
    connection.send(JSON.stringify(messageToSend));
}

function FeedOpponent() {

    // Mark that FeedOpponent has been called


    // Wait for `selectItem` to provide the item
    // This could be a UI interaction or external trigger

    // Example of sending the message once the item is selected


    const messageToSend = {
        "MessageType": "FEED_OPPONENT",
     
        "SocketId": socketId
    };

        connection.send(JSON.stringify(messageToSend));
        
    

}

function populateInventory(inventoryList) {
    var cards = '';
    inventoryList.forEach(function (item) {
     
        var card = `
            <div onclick="selectItem(${item.ItemId})" class="pokescape-item-card card-${item.Name}">
                <div class="pokescape-item-name">
                    ${item.Name}
                </div>
                <div class="item-count" style="">
                   
                </div>
               
                <img class="pokescape-item-img" src="${item.Image}" />
             
            </div>
        `;
        cards += card;
    });

    // Inject the cards into the scapemonsterInventory container
    $('#itemInventory').html(cards);
}
function populateMonsterCards(monsterList) {
    var cards = '';
    monsterList.forEach(function (monster) {

        
        var hpPercentage = (monster.Health / monster.MaximumHealth) * 100;
        
        
        var attackPercentage = (monster.Level / 50) * 100;
       

        var card = `
            <div onclick="selectScapemonster(this)"
        data-monster='${JSON.stringify(monster)}' class="pokescape-monster-card card-${monster.ScapeMonsterName}">
                <div class="pokescape-monster-name">
                    ${monster.ScapeMonsterName}
                </div>
                <div class="type-badge" style="background-color: ${monster.IsBoss ? '#FF0000' : '#603082'};">
                   lv. ${monster.Level}
                </div>
                <img  class="pokescape-monster-img" src="${monster.OpponentImage}"  />
                <div class="pokescape-monster-hp-container">
                    <div class="row cont-1-row">
                        <div class="card-label">HP:</div>
                        <div class="card-label-2">${monster.Health.toFixed(2)}/${monster.MaximumHealth.toFixed(2)}</div>
                    </div>
                    <div class="pokescape-monster-hp-bar">
                        <div class="pokescape-monster-hp-bar-filled" style="width:${hpPercentage}%;">
                        </div>
                    </div>
                </div>
                <div class="pokescape-monster-attack-container">
                    <div class="row cont-1-row">
                        <div class="card-label">Attack:</div>
                        <div class="card-label-2">${monster.Damage.toFixed(2) }</div>
                    </div>
                    <div class="pokescape-monster-attack-bar">
                        <div class="pokescape-monster-attack-bar-filled" style="width:${attackPercentage}%;">
                        </div>
                    </div>
                </div>
            </div>
        `;
        cards += card;
    });

    // Inject the cards into the scapemonsterInventory container
    $('#scapemonsterInventory').html(cards);
}


function discardScapeMonster() {

    const messageToSend = {
        "MessageType": "DISCARD_SCAPEMONSTER",
        "SocketId": socketId,
       
    };
    console.log('Sending message:', messageToSend);
    connection.send(JSON.stringify(messageToSend));
    $('.discardscapemonster').hide();
}

const url = 'ws://localhost:5000/ws';
const connection = new WebSocket(url);

let socketId = null;

connection.onopen = () => {
    console.log('WebSocket connection opened');
};

connection.onmessage = (event) => {
    const message = JSON.parse(event.data);
    console.log(message);
    switch (message.MessageType) {
        case 'grid':
            handleGrid(message.Data);
            break;
        case 'battleDialog':
            logToConsole(message.Data);
            break;
        case 'hideBattle':
            hideBattle();
            break;
        case 'monsterselectedforbattle':
           
            moveDefendant();
            moveDefendantRock();
            
            break;
        case 'save_game':
            SaveString("PokescapeGame",message.Data);
            break;
        case 'user':

            handleUser(message.Data);
            break;
        case 'login':
            console.log("logged in");
            socketId = message.Data;
            break;
        case 'moveResponse':
            handleMoveResponse(message.Data);
            break;
        case 'newBattle':

            $('.exitbattle').show();
            // Directly call playAudio() instead of using window.onload

            discardMoveInProcess = false;
            playAudio();
            setTimeout(function () {
                console.log("Waited for 1 seconds");
            }, 1000);
            // Handle the new battle logic as before
            handleNewBattle(message.Data);
            break;
        case 'battle':
            handleBattle(message.Data);
            break;
        default:
            console.warn('Unhandled message type:', message.MessageType);
    }
};
function hideBattle() {
  

    $('#battle-screen-2').hide();
    $('.exitbattle').hide();
    $('#defendant-info').html("");
    $('.defendant-row').hide();
    $('#move-controls').hide(); $('.defendant-options').hide();
    stopAudio();

}
function showBattleDialog(dialogToShow) {
    $('.defendant-options').hide();
    $('.defendant-options').html(dialogToShow);
    $('.defendant-options').show();
}

connection.onerror = (error) => {
    console.error('WebSocket error:', error);
};

connection.onclose = () => {
    console.log('WebSocket connection closed');
};  
function selectScapemonster(element) {
    // Retrieve the monster data from the data-monster attribute
    var monster = JSON.parse(element.getAttribute('data-monster'));
    $('.movebut').show();
    
    console.log('Selected Monster:', monster);


    addMoves(monster.Moves);
    $('.move-controls').show();

    const messageToSend = {
        "MessageType": "MONSTER_SELECTED",
        "SocketId": socketId,
        "Data": monster.ScapeMonsterID
    };
    console.log('Sending message:', messageToSend);
    connection.send(JSON.stringify(messageToSend));
    $('.discardscapemonster').show();
}

function hideMoves() {
    $('.move-controls').hide();
    $('.movebut').hide();
}



function discardMove() {
    discardMoveInProcess = true;

    console.log('discardMoveInProcess');


}

function saveGame() {
    const messageToSend = {
        "MessageType": "SAVE_GAME",
        "SocketId": socketId,
       
    };
    connection.send(JSON.stringify(messageToSend));
}
function loadGame() {
    UploadString(function (content) {
        const messageToSend = {
            "MessageType": "LOAD_GAME",
            "SocketId": socketId,
            "Data": content
        };
        connection.send(JSON.stringify(messageToSend));
    });

    
}
function SaveString(fileName, content) {
    // Create a Blob with the content
    var blob = new Blob([content], { type: 'text/plain' });

    // Create a link element
    var link = $('<a></a>');

    // Set the download attribute with the desired file name
    link.attr('download', fileName);

    // Create an object URL for the Blob
    var url = URL.createObjectURL(blob);

    // Set the href to the Blob URL
    link.attr('href', url);

    // Append the link to the body (required for Firefox)
    $('body').append(link);

    // Programmatically click the link to trigger the download
    link[0].click();

    // Remove the link after download
    link.remove();

    // Revoke the object URL to free up memory
    URL.revokeObjectURL(url);
}
function UploadString(callback) {
    // Create an input element of type file
    var input = $('<input type="file" accept=".txt" style="display: none;">');

    // Append the input to the body
    $('body').append(input);

    // Set up an event listener to handle file selection
    input.on('change', function (event) {
        var file = event.target.files[0];

        if (file) {
            var reader = new FileReader();

            // When the file is read, call the callback with the result
            reader.onload = function (e) {
                var content = e.target.result;
                callback(content);
            };

            // Read the file as a text string
            reader.readAsText(file);
        }

        // Clean up by removing the input element after the file is selected
        input.remove();
    });

    // Trigger the file input click event
    input.click();
}
function updateOpponent(monster) {
    var hpPercentage = (monster.Health / monster.MaximumHealth) * 100;
    var health = monster.Health;
    var attackPercentage = (monster.Level / 50) * 100;


    console.log("health", health, "max", monster.MaximumHealth);
    var opponentString = `

    <div class="pokescape-monster-name">
       ${monster.ScapeMonsterName}
    </div>
    <div class="type-badge ${monster.ScapeMonsterName}" style="background-color: #603082;">
        lv${monster.Level}
    </div>
    <div class="pokescape-monster-hp-container">
        <div class="row cont-1-row">
            <div class="card-label">HP:</div>
            <div class="card-label-2">${health.toFixed(2)}/${monster.MaximumHealth.toFixed(2)}</div>
        </div>
        <div class="pokescape-monster-hp-bar">
          <div class="pokescape-monster-hp-bar-filled" style="width:${hpPercentage}%;">
        </div>
    </div>
    <div class="pokescape-monster-attack-container">
        <div class="row cont-1-row">
            <div class="card-label">Attack:</div>
            <div class="card-label-2">${monster.Damage.toFixed(2) }</div>
        </div>
        <div class="pokescape-monster-attack-bar">
          <div class="pokescape-monster-attack-bar-filled" style="width:${attackPercentage}%;">
        </div>
  
</div>
`; $('#opponent-info').html(opponentString);

}
function updateDefendant(monster) {
    var hpPercentage = (monster.Health / monster.MaximumHealth) * 100;
    var attackPercentage = (monster.Level / 50) * 100;
    
    var opponentString = `

    <div class="pokescape-monster-name">
       ${monster.ScapeMonsterName}
    </div>
    <div class="type-badge ${monster.ScapeMonsterName}" style="background-color: #603082;">
                lv${monster.Level}
    </div>
    <div class="pokescape-monster-hp-container">
        <div class="row cont-1-row">
            <div class="card-label">HP:</div>
            <div class="card-label-2">${monster.Health.toFixed(2)}/${monster.MaximumHealth.toFixed(2)}</div>
        </div>
        <div class="pokescape-monster-hp-bar">
          <div class="pokescape-monster-hp-bar-filled" style="width:${hpPercentage}%;">
        </div>
    </div>
    <div class="pokescape-monster-attack-container">
        <div class="row cont-1-row">
            <div class="card-label">Attack:</div>
            <div class="card-label-2">${monster.Damage.toFixed(2) } </div>
        </div>
        <div class="pokescape-monster-attack-bar">
          <div class="pokescape-monster-attack-bar-filled" style="width:${attackPercentage}%;">
        </div>
  
</div>
`; $('#defendant-info').html(opponentString);
    
}
function handleBattle(battle) {
    var convertedBattle = JSON.parse(battle);
    console.log(convertedBattle);
    var monster = convertedBattle.UserScapeMonster;
    updateDefendant(monster);
    $('#defendant-image').attr("src", monster.TamedImage);
  
    updateOpponent(convertedBattle.OpponentScapeMonster);
    $('.defendant-row').show();
    addMoves(monster.Moves);
    $('.move-controls').show();




}

var x = document.getElementById("myAudio");

// Function to play audio from the beginning
function playAudio() {
    x.currentTime = 0; // Reset audio to the start
    x.play().then(() => {
        console.log("Audio started playing from the beginning.");
    }).catch(error => {
        console.error("Failed to play audio:", error);
    });
}

// Function to pause and stop the audio
function stopAudio() {
    x.pause(); // Pause the audio
    
    console.log("Audio stopped.");
}

function resumeAudio() {
    // If audio is paused and not at the start, resume from the current position
    if (x.paused && x.currentTime > 0 && x.currentTime < x.duration) {
        x.play().then(() => {
            console.log("Audio resumed from position:", x.currentTime);
        }).catch(error => {
            console.error("Failed to resume audio:", error);
        });
    } else {
        console.log("Audio is either not paused or already finished.");
    }
}


function addMoves(moves) {
    let htmlString = `<h4 class="moves-h">Moves/Attacks:</h4>`;

    // Check if moves array is empty
    if (moves.length === 0) {
        htmlString += `<p>No moves available.</p>`;
    } else {
        // Generate rows of moves dynamically
        for (let i = 0; i < moves.length; i += 2) {

            htmlString += `<div class="move-row">`;

            // First move button, always exists at index i
            htmlString += `<a onclick="selectMove(${moves[i]?.Id}, '${moves[i]?.moveType}')" class="move-button"> ${moves[i]?.MoveName || 'Empty'}</a>`;

            // Second move button, exists at index i + 1 or is disabled
            if (moves[i + 1]) {
                htmlString += `<a onclick="selectMove(${moves[i+1]?.Id}, '${moves[i+1]?.moveType}')" class="move-button">${moves[i+1]?.MoveName || 'Empty'}</a>`;
            }
            else {
                htmlString += `<a class="move-button" disabled>Empty</a>`;
            }

            htmlString += `</div>`;
        }
    }

    $('#move-controls').html(htmlString);

  
}
function selectMove(moveId, moveType) {

    if (discardMoveInProcess == true) {

        const messageToSend = {
            "MessageType": "DISCARD_MOVE",
            "SocketId": socketId,
            "Data": moveId
        };
        console.log('Sending message:', messageToSend);
        connection.send(JSON.stringify(messageToSend));

        
    }

    else {

        if (moveType = 'attack') { $('#explosion').show(); }

        


        setTimeout(function () {
            $('#explosion').hide();
            
        }, 80);

        const messageToSend = {
            "MessageType": "ATTACK_MOVE",
            "SocketId": socketId,
            "Data": moveId
        };
        console.log('Sending message:', messageToSend);
        connection.send(JSON.stringify(messageToSend));


    }

    

    
}
function handleNewBattle(battle) {
    var convertedBattle = JSON.parse(battle);
    console.log(convertedBattle);
    var monster = convertedBattle.OpponentScapeMonster;
    

    updateOpponent(monster);
    
    // Set opponent image
    $('#opponent-image').attr("src", monster.OpponentImage);
  //  $('#opponent-scapemonster-1').attr("onclick", "selectScapemonster("+")")
    $('#opponent-scapemonster-1').attr("data-monster", monster);
    // Show flashing animation
    $('#flashing-screen').show();

    // Hide the flashing screen after 2 seconds and show the battle screen
    setTimeout(function () {
        $('#flashing-screen').hide(); // Hide flashing screen
        $('#battle-screen-2').show();  // Show battle screen
       

    }, 1500);  // 2 seconds delay
    moveOpponent();
    moveOpponentRock();
    var usermonster = convertedBattle.UserScapeMonster;
    updateDefendant(usermonster);
   
}

let oPposition = -270;
let defPositionleft = -250;
let defPositionbottom = -200;
let oProckposition = 760;
let DefendantRockPosition = -690;
function logToConsole(textToLog) {
    $("#battle-text").append('<div class="battle-text-item">'+textToLog+'</span>');
}
function moveOpponent() {
    const opponent = $("#opponent-image"); // jQuery selector for opponent element
    const targetPosition = 40; // Target position from the right edge in pixels
    const speed = 1.5; // Speed in pixels per frame

    // Move towards the target position from the right
    if (oPposition < targetPosition) {
        oPposition += speed; // Increment position towards the target
        opponent.css("right", oPposition + "px"); // Update right position with jQuery
        requestAnimationFrame(moveOpponent); // Continue animation
    }
}

function moveDefendant() {
    const defendant = $("#defendant-image");
    const targetPositionLeft = 0;
    const targetPositionBottom = 50;
    const speedLeft = 6;
    const speedBottom = 6;

   
    // Move left
    if (defPositionleft < targetPositionLeft) {
        defPositionleft += speedLeft;
        defendant.css("left", defPositionleft + "px");
        if (defPositionbottom < targetPositionBottom) {
            defPositionbottom += speedBottom;
            defendant.css("bottom", defPositionbottom + "px");
            
        }

        requestAnimationFrame(moveDefendant);
    }


   
}
function moveOpponentRock() {
    const opponentRock = $("#opponent-rock"); // jQuery selector for opponent element
    const targetPosition = -120; // Target position from the right edge in pixels
    const speed = 4.258; // Speed in pixels per frame

    // Move towards the target position from the right
    if (oProckposition > targetPosition) {
        oProckposition -= speed; // Increment position towards the target
        opponentRock.css("right", oProckposition + "px"); // Update right position with jQuery
        requestAnimationFrame(moveOpponentRock); // Continue animation
    }
}

function moveDefendantRock() {
    const DefendantRock = $("#defendant-rock"); // jQuery selector for opponent element
    const targetPosition = -220 // Target position from the right edge in pixels
    const speed = 11.19; // Speed in pixels per frame

    // Move towards the target position from the right
    if (DefendantRockPosition < targetPosition) {
        DefendantRockPosition += speed; // Increment position towards the target
        DefendantRock.css("left", DefendantRockPosition + "px"); // Update right position with jQuery
        requestAnimationFrame(moveDefendantRock); // Continue animation
    }
}


function handleUser(eventMessage) {
    var user = JSON.parse(eventMessage);
    console.log("Eventmessage:", user.UserImage);
    $("#character-one").attr("src", user.UserImage);
    console.log("coords");
    console.log(user.UserCoordinates);
    handleUserGridPosition(user.UserCoordinates);
    populateMonsterCards(user.ScapeMonsters);
    populateInventory(user.Inventory);
}

let renameinprogress = false;
function renameScapeMonster()
{

    // if pressed first time, rename in progress
    if (renameinprogress == false) {
        renameinprogress = true;
        $(".input-text").show();
    }

    else {
        renameinprogress = false;
        $(".input-text").hide();
        const newName = document.getElementById("scapemonsterNameInput").value;

        if (newName != "") {
            console.log("Renaming ScapeMonster to:", newName);
            


            const messageToSend = {
                "MessageType": "RENAME",
                "SocketId": socketId,
                "Data": newName,
            };
            console.log('Sending message:', messageToSend);
            connection.send(JSON.stringify(messageToSend));


        }

        document.getElementById("scapemonsterNameInput").value = "";

        
    }
   



}

function handleUserGridPosition(userCoordinates) {
    const grid = $('#grid');
    const container = $('.grid-container');

    const gridWidth = grid.outerWidth();
    const gridHeight = grid.outerHeight();
    const containerWidth = container.outerWidth();
    const containerHeight = container.outerHeight();

    // Calculate the new position for the grid based on user coordinates
    let blockWidth = $("img[class^='block-img']").first().width();
    console.log("blockWidth is " + blockWidth);

    // Calculate new left and top positions with inverted y logic for correct movement
    let newleft2 = -(userCoordinates.Item1 * blockWidth) + (visibleGridSize / 2 * blockWidth);
    let newtop2 = -(gridSize * blockWidth) + (visibleGridSize / 2 * blockWidth) + (userCoordinates.Item2 * blockWidth);

    console.log("new bottom and left before adjustment");

    // Apply a CSS transition so the grid animates its movement //change
    grid.css({ //change
        "transition": "top 0.2s ease-out, left 0.2s ease-out", //change
        "top": newtop2 + 'px', //change
        "left": newleft2 + 'px' //change
    }); //change
}

function handleGrid(eventMessage) {
    const dict = JSON.parse(eventMessage);
    let allText = '<div class="col">';

    // Keep the character's width but remove any transform so it doesn't move //change
    $("#character-one").css("width", blockPercentageWidth.toString() + "%");

    // Remove or comment out the character transform lines so the character does not move //change
    var yAdjustment = 100 + blockPercentageWidth; //change
    $("#character-one").css("transform", "translate(" + blockPercentageWidth.toString() + "% , -" + yAdjustment.toString() + "%)"); //change

    // Create a 2D array to sort blocks based on x and y
    let blocks = [];

    for (let key in dict) {
        if (dict.hasOwnProperty(key)) {
            let block = dict[key];

            // Extract the x, y coordinates from the key
            let coordinates = key.replace(/[()]/g, '').split(',');
            let x = parseInt(coordinates[0].trim());
            let y = parseInt(coordinates[1].trim());

            // Ensure the array for row x exists
            if (!blocks[x]) {
                blocks[x] = [];
            }

            // Place the block at the correct (x, y) position
            blocks[x][y] = block;
        }
    }

    // Render the grid using flexbox with the sorted blocks
    for (let rowIndex in blocks) {
        let row = blocks[rowIndex];
        if (row) {
            for (let colIndex = row.length - 1; colIndex >= 0; colIndex--) {
                let block = row[colIndex];
                if (block) {
                    // Generate a class based on the proper x (row) and y (column) coordinates
                    let blockClass = `block${rowIndex}-${colIndex}`;

                    // Add the 'has-user' class if block.HasUser is true
                    if (block.HasUser) {
                        blockClass += ' has-user';
                    }

                    let blockToAdd = `
            <div style="width: ${blockPercentageWidth}%; height: ${blockPercentageWidth}%;"
                 class="block ${blockClass}">
                <img class="block-img" src="${block.Image}" />
            </div>`;
                    allText += blockToAdd;
                }
            }
            allText += '</div><div class="col">';
        }
    }

    allText += '</div>';
    $("#grid").html(allText);
}


function handleGridOld(eventMessage) {
    const dict = JSON.parse(eventMessage);
    let allText = '<div class="col">';
  
    let count = 0;
    
    $("#character-one").css("width", blockPercentageWidth.toString() + "%");
    for (let key in dict) {
        if (dict.hasOwnProperty(key)) {
            if (count === gridSize) {
                allText += '</div><div class="col">';
                count = 0;
            }

            let block = dict[key];
           
      
            let blockToAdd = `
                <div style="width: ${blockPercentageWidth}%; height: ${blockPercentageWidth}%;"
                     class="block block-a">
                    <img src="${block.Image}" />
                </div>`;
            allText += blockToAdd;
            count++;
        }
    }
    allText += '</div>';
    $("#grid").html(allText);
}

document.addEventListener('keydown', (e) => {
    if (!socketId) {
        console.error('Socket ID is not set. Cannot move player.');
        return;
    }

    if (renameinprogress == false) {


        let messageType = "";

        // Handle movement keys
        if (e.key === 'w' || e.key === 'ArrowUp') {
            messageType = "MOVE_UP";
        } else if (e.key === 'a' || e.key === 'ArrowLeft') {
            messageType = "MOVE_LEFT";
        } else if (e.key === 's' || e.key === 'ArrowDown') {
            messageType = "MOVE_DOWN";
        } else if (e.key === 'd' || e.key === 'ArrowRight') {
            messageType = "MOVE_RIGHT";
        }

        // Only send message if messageType is valid
        if (messageType) {
            const messageToSend = {
                "MessageType": messageType,
                "SocketId": socketId
            };
            console.log('Sending message:', messageToSend);

            // Check WebSocket connection before sending
            if (connection.readyState === WebSocket.OPEN) {
                connection.send(JSON.stringify(messageToSend));
            } else {
                console.error('WebSocket connection is not open.');
            }
        } else {
            console.error('Unrecognized key pressed:', e.key);
        }
    }
    else {
        console.log('Rename in progress');
    }
});



 
function handleMoveResponse(data) {
    console.log('Move response:', data);
}
