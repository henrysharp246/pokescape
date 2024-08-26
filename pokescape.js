//THESE ARE IMPORTANT V
const visibleGridSize = 15;
const gridSize = 225;
var blockPercentageWidth = 100 / visibleGridSize;




function toggleInventory() {
    $("#inventoryBox").toggle();
}

function closeWelcomeMessage() {
    $("#welcomeMessage").hide();
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
        default:
            console.warn('Unhandled message type:', message.MessageType);
    }
};

connection.onerror = (error) => {
    console.error('WebSocket error:', error);
};

connection.onclose = () => {
    console.log('WebSocket connection closed');
};
function handleUser(eventMessage) {
    var user = JSON.parse(eventMessage);
    console.log("Eventmessage:", user.UserImage);
    $("#character-one").attr("src", user.UserImage);
    console.log("coords");
    console.log(user.UserCoordinates);
    handleUserGridPosition(user.UserCoordinates);
}


function handleUserGridPosition(userCoordinates) {
    const grid = $('#grid');
    const container = $('.grid-container');

    const gridWidth = grid.outerWidth();
    const gridHeight = grid.outerHeight();
    const containerWidth = container.outerWidth();
    const containerHeight = container.outerHeight();

    // Calculate the new position for the grid based on user coordinates
    let newLeft = /*-(userCoordinates.Item1 * gridWidth / 100) + (containerWidth / 2);*/ -(userCoordinates.Item1 * blockPercentageWidth)
    // Invert the direction for vertical movement
    let newTop = /*(userCoordinates.Item2 * gridHeight / 100) - (containerHeight / 2);*/ +(userCoordinates.Item2 * blockPercentageWidth)

    console.log("new top and left before adjustment");
    console.log("new top: " + newTop);
    console.log("new left: " + newLeft);

    grid.css({
        top: newTop + 'px',
        left: newLeft + 'px'
    });
}

function handleGrid(eventMessage) {
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
