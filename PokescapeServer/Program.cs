using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq.Expressions;
using System.Net.WebSockets;
using PokescapeServer;


    public class Pokescape
    {

        public static readonly string ImageFolderPath = "./Image";
    //REPLACE THIS WITH YOURS :
    //"C:\\Users\\henry\\source\\repos\\pokescape\\Image

    //STEP 1s
    //1. Find various images for the grid coordinate classes
    //2. Add more types of grid coordinate classes
    //3. Work out the best way to create the grid so that it is both random, but paths also connect //write comments on how you do this
    //
    //STEP 2 
    // Create websocket server
    public static async Task Main(string[] args)
        {
          

            await WebsocketServer.Listen();

        }
   


} 