//THESE ARE IMPORTANT V
const visibleGridSize = 15;
const gridSize = 50;
var blockPercentageWidth = 100 / visibleGridSize;




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
function selectItem(itemId) {
    showScapeMonsters();
    const messageToSend = {
        "MessageType": "ITEM_SELECTED",
        "Data": itemId,
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
        var attackPercentage = (monster.BaseDamage / 1000) * 100;  // assuming max attack is 1300

        var card = `
            <div onclick="selectScapemonster(${monster.ScapeMonsterID})" class="pokescape-monster-card card-${monster.ScapeMonsterName}">
                <div class="pokescape-monster-name">
                    ${monster.ScapeMonsterName}
                </div>
                <div class="type-badge" style="background-color: ${monster.IsBoss ? '#FF0000' : '#603082'};">
                    ${monster.IsBoss ? 'Boss' : 'Mystical'}
                </div>
                <img class="pokescape-monster-img" src="${monster.OpponentImage}" />
                <div class="pokescape-monster-hp-container">
                    <div class="row cont-1-row">
                        <div class="card-label">HP:</div>
                        <div class="card-label-2">${hpPercentage.toFixed(0)}%</div>
                    </div>
                    <div class="pokescape-monster-hp-bar">
                        <div class="pokescape-monster-hp-bar-filled" style="width:${hpPercentage}%;">
                        </div>
                    </div>
                </div>
                <div class="pokescape-monster-attack-container">
                    <div class="row cont-1-row">
                        <div class="card-label">Attack:</div>
                        <div class="card-label-2">${monster.BaseDamage}/1000</div>
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
            showBattleDialog(message.Data);
            break;
        case 'hideBattle':
            hideBattle();
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
    $('#move-controls').hide();
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
function selectScapemonster(scapeMonsterId) {
    var messageType = "";
    const messageToSend = {
        "MessageType": "MONSTER_SELECTED_FOR_BATTLE",
        "SocketId": socketId,
        "Data": scapeMonsterId
    };
    console.log('Sending message:', messageToSend);
    connection.send(JSON.stringify(messageToSend));
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
    var attackPercentage = (monster.BaseDamage / 1000) * 100;  // assuming max attack is 1300
    var opponentString = `

    <div class="pokescape-monster-name">
       ${monster.ScapeMonsterName}
    </div>
    <div class="type-badge" style="background-color: #603082;">
        Mystical
    </div>
    <div class="pokescape-monster-hp-container">
        <div class="row cont-1-row">
            <div class="card-label">HP:</div>
            <div class="card-label-2">${hpPercentage.toFixed(0)}%</div>
        </div>
        <div class="pokescape-monster-hp-bar">
          <div class="pokescape-monster-hp-bar-filled" style="width:${hpPercentage}%;">
        </div>
    </div>
    <div class="pokescape-monster-attack-container">
        <div class="row cont-1-row">
            <div class="card-label">Attack:</div>
            <div class="card-label-2">${monster.BaseDamage}/1000</div>
        </div>
        <div class="pokescape-monster-attack-bar">
          <div class="pokescape-monster-attack-bar-filled" style="width:${attackPercentage}%;">
        </div>
  
</div>
`; $('#opponent-info').html(opponentString);
}
function updateDefendant(monster) {
    var hpPercentage = (monster.Health / monster.MaximumHealth) * 100;
    var attackPercentage = (monster.BaseDamage / 1000) * 100;  // assuming max attack is 1300
    var opponentString = `

    <div class="pokescape-monster-name">
       ${monster.ScapeMonsterName}
    </div>
    <div class="type-badge" style="background-color: #603082;">
        Mystical
    </div>
    <div class="pokescape-monster-hp-container">
        <div class="row cont-1-row">
            <div class="card-label">HP:</div>
            <div class="card-label-2">${hpPercentage.toFixed(0)}%</div>
        </div>
        <div class="pokescape-monster-hp-bar">
          <div class="pokescape-monster-hp-bar-filled" style="width:${hpPercentage}%;">
        </div>
    </div>
    <div class="pokescape-monster-attack-container">
        <div class="row cont-1-row">
            <div class="card-label">Attack:</div>
            <div class="card-label-2">${monster.BaseDamage}/1000</div>
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

function addMoves(moves) {
    let htmlString = `<h4>Moves/Attacks:</h4>`;

    // Check if moves array is empty
    if (moves.length === 0) {
        htmlString += `<p>No moves available.</p>`;
    } else {
        // Generate rows of moves dynamically
        for (let i = 0; i < moves.length; i += 2) {
            htmlString += `<div class="move-row">`;

            // First move button, always exists at index i
            htmlString += `<a onclick="attack(${moves[i]?.Id})" class="move-button">${moves[i]?.MoveName || 'No Move'}</a>`;

            // Second move button, exists at index i + 1 or is disabled
            if (moves[i + 1]) {
                htmlString += `<a class="move-button">${moves[i + 1].MoveName}</a>`;
            } else {
                htmlString += `<a class="move-button" disabled>No Move</a>`;
            }

            htmlString += `</div>`;
        }
    }

    $('#move-controls').html(htmlString);

  
}
function attack(moveId) {
    const messageToSend = {
        "MessageType": "ATTACK_MOVE",
        "SocketId": socketId,
        "Data": moveId
    };
    console.log('Sending message:', messageToSend);
    connection.send(JSON.stringify(messageToSend));
}
function handleNewBattle(battle) {
    var convertedBattle = JSON.parse(battle);
    console.log(convertedBattle);
    var monster = convertedBattle.OpponentScapeMonster;

    updateOpponent(monster);
    $('#opponent-image').attr("src", monster.OpponentImage);
    $('#battle-screen-2').show();


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
    let newtop2 = -(gridSize * blockWidth) + (visibleGridSize / 2 * blockWidth) + (userCoordinates.Item2 * blockWidth) ;/*- (visibleGridSize / 2 * blockWidth);*/ // Invert y-coordinate calculation for movement

    console.log("new bottom and left before adjustment");

    grid.css({
        top: newtop2 + 'px', // Use bottom for vertical positioning
        left: newleft2 + 'px'
    });

} function handleGrid(eventMessage) {
    const dict = JSON.parse(eventMessage);
    let allText = '<div class="col">';

    $("#character-one").css("width", blockPercentageWidth.toString() + "%");
    var yAdjustment = 100 + blockPercentageWidth;
    $("#character-one").css("transform", "translate(" + blockPercentageWidth.toString() + "% , -" + yAdjustment.toString() + "%)");
    // Create a 2D array to sort blocks based on x and y
    let blocks = [];

    for (let key in dict) {
        if (dict.hasOwnProperty(key)) {
            let block = dict[key];

            // Extract the x, y coordinates from the key
            let coordinates = key.replace(/[()]/g, '').split(',');
            let x = parseInt(coordinates[0].trim());
            let y = parseInt(coordinates[1].trim());

            // Ensure the array for row y exists
            if (!blocks[x]) {  // Note: We're correctly treating x as the row here
                blocks[x] = [];
            }

            // Place the block at the correct (x, y) position
            blocks[x][y] = block;  // Correctly assigning to blocks[x][y] (x is row, y is column)
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
    var messageType = "";
    switch (e.key) {
        case 'w':
            messageType = "MOVE_UP";
            break;
        case 'a':
            messageType = "MOVE_LEFT";
            break;
        case 's':
            messageType = "MOVE_DOWN";
            break;
        case 'd':  
            messageType = "MOVE_RIGHT";
            break;

    }
    const messageToSend = {
        "MessageType": messageType,
        "SocketId": socketId
    };
    console.log('Sending message:', messageToSend);
    connection.send(JSON.stringify(messageToSend));
});


 
function handleMoveResponse(data) {
    console.log('Move response:', data);
}
