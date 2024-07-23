using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq.Expressions;
using System.Net.WebSockets;

namespace PokescapeServer
{
    public class Pokescape
    {
        //STEP 1
        //1. Find various images for the grid coordinate classes
        //2. Add more types of grid coordinate classes
        //3. Work out the best way to create the grid so that it is both random, but paths also connect //write comments on how you do this
        //
        //STEP 2 
        // Create websocket server
        public static async Task Main(string[] args)
        {
            // var grid = CreateGrid(20);
            //  LogGrid(grid);
            await WebsocketServer.Listen();

        }

    


} }