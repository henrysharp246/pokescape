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
    console.log("Eventmessage:", user.userImage);
    $("#character-one").attr("src", user.userImage);
    
}
function handleGrid(eventMessage) {
    const dict = JSON.parse(eventMessage);
    let allText = '<div class="col">';
    const gridSize = 21;
    let count = 0;
    let percentageWidth = 100 / gridSize;
    $("#character-one").css("width", percentageWidth.toString() + "%");
    for (let key in dict) {
        if (dict.hasOwnProperty(key)) {
            if (count === gridSize) {
                allText += '</div><div class="col">';
                count = 0;
            }

            let block = dict[key];
      
            let blockToAdd = `
                <div style="width: ${percentageWidth}%; height: ${percentageWidth}%;"
                     class="block block-a">
                    <img src="${block.image}" />
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
            messageTpe = "MOVE_RIGHT";
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
