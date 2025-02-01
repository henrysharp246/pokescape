using Newtonsoft.Json;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Collections.Concurrent;

namespace PokescapeServer
{
    static class WebsocketServer
    {
        static ConcurrentDictionary<string, Game> socketIdsToGames = new();  // using concurrent version of the dictionary allows multiple clients to connect to same server

        public static async Task Listen()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/ws/");
            listener.Start();
            Console.WriteLine("WebSocket server started at ws://localhost:5000/ws/");

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                    WebSocket webSocket = wsContext.WebSocket;

                    await OnClientConnected(webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }
        
        private static async Task OnClientConnected(WebSocket webSocket)
        {
            Console.WriteLine("New client connected");
            var loginMessage = new Message();
            string socketId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            loginMessage.MessageType = "login";
            loginMessage.Data = socketId;
            await SendMessage(webSocket, loginMessage);
            var game = new Game(webSocket);
            socketIdsToGames[socketId] = game;
            await game.StartGame();
            // Handle the WebSocket connection
            await HandleWebSocketConnection(webSocket);
        }
        private static async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            byte[] buffer = new byte[4096]; // Adjust the buffer size as needed.

            while (webSocket.State == WebSocketState.Open)
            {
                var receivedData = new List<byte>(); // Reset for each message.
                WebSocketReceiveResult result;

                // Loop to ensure we receive all fragments for a message
                do
                {
                      result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    receivedData.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));
                } while (!result.EndOfMessage); // Continue until the full message is received.

                // Decode and process the message.
                string message = Encoding.UTF8.GetString(receivedData.ToArray());

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Deserialize the message and handle it
                    Message requestFromFrontEnd = JsonConvert.DeserializeObject<Message>(message);
                    await HandleMessage(webSocket, requestFromFrontEnd);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    // Handle closing
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    webSocket.Dispose();
                    break; // Exit the loop if WebSocket is closing
                }
            }
        }
        public static async Task HandleMessage(WebSocket webSocket, Message message)
        {
            Game gameForUser = socketIdsToGames[message.SocketId];
           await gameForUser.HandleMessage(message); 
           
        }
        private static string GetMessageType(string message)
        {
            // Assuming the message is in JSON format: { "type": "greeting", "payload": "Hello" }
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(message);
                if (json.RootElement.TryGetProperty("type", out var type))
                {
                    return type.GetString();
                }
            }
            catch
            {
                // Handle parsing error if necessary
            }
            return "unknown";
        }

        public static async Task SendMessage(this WebSocket webSocket, Message messageToSend)
        {
          
            string messageJson = JsonConvert.SerializeObject(messageToSend);
            byte[] responseBuffer = Encoding.UTF8.GetBytes(messageJson);
            await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static void SendMessageNoWait(this WebSocket webSocket, Message messageToSend)
        {
            string messageJson = JsonConvert.SerializeObject(messageToSend);
            byte[] responseBuffer = Encoding.UTF8.GetBytes(messageJson);
            webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
        }

    }

}
