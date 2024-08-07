﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace PokescapeServer
{
   
  
    static class WebsocketServer
    {
        static Dictionary<string, Game> socketIdsToGames = new();

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
            socketIdsToGames.Add(socketId, game);
            await game.StartGame();
            // Handle the WebSocket connection
            await HandleWebSocketConnection(webSocket);
        }
        private static async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024];

            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (result.MessageType != WebSocketMessageType.Close)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                

              
                Message requestFromFrontEnd = JsonConvert.DeserializeObject<Message>(message);
                await HandleMessage(webSocket, requestFromFrontEnd);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            webSocket.Dispose();
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
