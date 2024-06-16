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

    
        public static Dictionary<(int x, int y), Block> CreateGridV1(int size)
        {
            Dictionary<(int x, int y), Block> grid = new();
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Block block = new Block();
                    block.CanPass = true;
                    block.Name = "grass";
                    grid.Add((x, y), block);

                }
            }
            return grid;
                  
        }



  

        public static void LogGrid(IBlock[][] grid)
        {
            for (int x = 0; x < grid.Length; x++)
            {
                for (int y = 0; y < grid.Length; y++)
                {
                    Console.WriteLine(grid[x][y].Name);
                }
                Console.WriteLine("\n");
            }
        }
} }