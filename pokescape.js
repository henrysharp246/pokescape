// JavaScript source code
function toggleInventory() {
    $("#inventoryBox").toggle();
}
function closeWelcomeMessage() {
    $("#welcomeMessage").hide();
}

var websocket; 
const url = 'ws://localhost:5000/ws';
const connection = new WebSocket(url);

connection.onopen = () => {
    console.log('WebSocket connection opened');
    
};

connection.onmessage = (event) => {
    
    console.log(1);
    const message = JSON.parse(event.data);
    console.log(message);
    switch (message.MessageType) {
        case 'grid':
            handleGrid(message.Data);
            break;
      
     
    }
};

connection.onerror = (error) => {
    console.error('WebSocket error:', error);
};

connection.onclose = () => {
    console.log('WebSocket connection closed');
};

function handleGrid(eventMessage) {

    console.log("2");
    console.log(eventMessage);
    var dict = JSON.parse(eventMessage);


    var allText = '';
    var openCol = '<div class="col">';
    var closeCol ='</div>'

    var gridSize = 10; 
    
    var count = 0;
    allText = allText + openCol;
    for (let key in dict) {
        count++;
        if (count == gridSize) {
            allText += closeCol;
            allText += openCol;
            count = 0;
        }

        if (dict.hasOwnProperty(key)) {
            let value = dict[key];
            /*  '<div class="col">'

            '</div>'
            */
            var xCoord = key.slice(1, -1)
            // Convert the key string back to a tuple (array in JS)
            let tuple = JSON.parse("[" + key.slice(1, -1) + "]");


            var blockToAdd = '<div class="block block-' + value.Name + '">' + '</div>'
            allText += blockToAdd;
         
        }
       
    }
    $("#grid").append(allText);
    //convert data from string to dictionary
    //iterate through every columnn

    //using js add a column in html to the id=grid
    //in each column add all the rows.
}

function moveLeft() {
    var messageToSend = {
        "MessageType":"MOVE_LEFT",
        "Data": ""
    };
    connection.send(JSON.stringify(messageToSend));

}
function moveRight() {

}
function moveUp(){

}
function moveDown() {

}
