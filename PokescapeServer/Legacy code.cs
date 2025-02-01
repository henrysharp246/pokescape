using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
namespace PokescapeServer
{
    internal class LegacyCode
    {
        public Dictionary<(int x, int y), Block> CreateGridBlank()
        {
            Dictionary<(int x, int y), Block> grid = new();
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Block block = new StoneFloorBlock();

                    grid.Add((x, y), block);

                }
            }
            this.currentGrid = grid;
            return grid;

        }
        public Dictionary<(int x, int y), Block> CreateGridV2()
        {
            Dictionary<(int x, int y), Block> grid = new();
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    string blockToLeftName = GetBlockNameToLeft(x, y);
                    string blockToRightName = GetBlockNameToRight(x, y);
                    string blockAboveName = GetBlockNameAbove(x, y);
                    string blockBelowName = GetBlockNameBelow(x, y);
                    string blockToTopLeftName = GetBlockNameToTopLeft(x, y);
                    string blockToTopRightName = GetBlockNameToTopRight(x, y);
                    string blockToBottomLeftName = GetBlockNameToBottomLeft(x, y);
                    string blockToBottomRightName = GetBlockNameToBottomRight(x, y);


                    if (x < 55)
                    {
                        if (y < -105)
                        {

                            Block blankblock = new BlankBlock();
                            blankblock.CanPass = true;

                            grid.Add((x, y), blankblock);
                        }

                    }
                    else
                    {
                        Random random = new Random();
                        int rand = random.Next(0, 10);
                        if (blockToLeftName == "blank" || blockAboveName == "blank" || blockToRightName == "blank" || blockBelowName == "blank" || blockToTopLeftName == "blank" || blockToTopRightName == "blank" || blockToBottomRightName == "blank" || blockToBottomLeftName == "blank")
                        {
                            Block wallblock = new StoneWallBlock();
                            wallblock.CanPass = true;
                            grid.Add((x, y), wallblock);
                        }

                        if (blockToLeftName == "stonewallblock" || blockAboveName == "stonewallblock" || blockToRightName == "stonewallblock" || blockBelowName == "stonewallblock")
                        {
                            Block stonefloorblock = new StoneWallBlock();
                            grid.Add((x, y), stonefloorblock);
                        }




                    }
                }
            }
            this.currentGrid = grid;
            return grid;
        }
        public string GetBlockCoordsToLeft(int x, int y)
        {
            if (x > 0 && currentGrid.ContainsKey((x - 1, y)))
            {
                return currentGrid[(x - 1, y)].Name;
            }
            else { return "default"; } // Or any other default name you prefer
        }

        public string GetBlockToCoordsRight(int x, int y)
        {
            if (x < gridSize - 1 && currentGrid.ContainsKey((x + 1, y)))
            {
                return currentGrid[(x + 1, y)].Name;
            }
            else { return "default"; }
        }

        public string GetBlockCoordsAbove(int x, int y)
        {
            if (y < gridSize - 1 && currentGrid.ContainsKey((x, y + 1)))
            {
                return currentGrid[(x, y + 1)].Name;
            }
            else { return "default"; }
        }

        public Block GetBlockBelow(int x, int y)
        {
            if (y > 0 && currentGrid.ContainsKey((x, y - 1)))
            {
                return currentGrid[(x, y - 1)];
            }
            else { return null; }
        }
        public Block GetBlockAbove(int x, int y)
        {
            if (y > 0 && currentGrid.ContainsKey((x, y + 1)))
            {
                return currentGrid[(x, y - 1)];
            }
            else { return null; }
        }
        public Block GetBlockToTopLeft(int x, int y)
        {
            if (x > 0 && y < gridSize - 1 && currentGrid.ContainsKey((x - 1, y + 1)))
            {
                return currentGrid[(x - 1, y + 1)];
            }
            else { return null; }
        }

        public Block GetBlockToRight(int x, int y)
        {
            if (x < gridSize - 1 && y < gridSize - 1 && currentGrid.ContainsKey((x + 1, y)))
            {
                return currentGrid[(x + 1, y + 1)];
            }
            else { return null; }
        }

        public Block GetBlockToLeft(int x, int y)
        {
            if (x > 0 && y < gridSize - 1 && currentGrid.ContainsKey((x - 1, y)))
            {
                return currentGrid[(x - 1, y + 1)];
            }
            else { return null; }
        }

        public Block GetBlockToTopRight(int x, int y)
        {
            if (x < gridSize - 1 && y < gridSize - 1 && currentGrid.ContainsKey((x + 1, y + 1)))
            {
                return currentGrid[(x + 1, y + 1)];
            }
            else { return null; }
        }

        public Block GetBlockToBottomLeft(int x, int y)
        {
            if (x > 0 && y > 0 && currentGrid.ContainsKey((x - 1, y - 1)))
            {
                return currentGrid[(x - 1, y - 1)];
            }
            else { return null; }
        }

        public Block GetBlockToBottomRight(int x, int y)
        {
            if (x < gridSize - 1 && y > 0 && currentGrid.ContainsKey((x + 1, y - 1)))
            {
                return currentGrid[(x + 1, y - 1)];
            }
            else { return null; }
        }



        public string GetBlockNameToLeft(int x, int y)
        {
            if (x > 0 && currentGrid.ContainsKey((x - 1, y)))
            {
                return currentGrid[(x - 1, y)].Name;
            }
            else { return "default"; } // Or any other default name you prefer
        }

        public string GetBlockNameToRight(int x, int y)
        {
            if (x < gridSize - 1 && currentGrid.ContainsKey((x + 1, y)))
            {
                return currentGrid[(x + 1, y)].Name;
            }
            else { return "default"; }
        }

        public string GetBlockNameAbove(int x, int y)
        {
            if (y < gridSize - 1 && currentGrid.ContainsKey((x, y + 1)))
            {
                return currentGrid[(x, y + 1)].Name;
            }
            else { return "default"; }
        }

        public string GetBlockNameBelow(int x, int y)
        {
            if (y > 0 && currentGrid.ContainsKey((x, y - 1)))
            {
                return currentGrid[(x, y - 1)].Name;
            }
            else { return "default"; }
        }

        public string GetBlockNameToTopLeft(int x, int y)
        {
            if (x > 0 && y < gridSize - 1 && currentGrid.ContainsKey((x - 1, y + 1)))
            {
                return currentGrid[(x - 1, y + 1)].Name;
            }
            else { return "default"; }
        }

        public string GetBlockNameToTopRight(int x, int y)
        {
            if (x < gridSize - 1 && y < gridSize - 1 && currentGrid.ContainsKey((x + 1, y + 1)))
            {
                return currentGrid[(x + 1, y + 1)].Name;
            }
            else { return "default"; }
        }

        public string GetBlockNameToBottomLeft(int x, int y)
        {
            if (x > 0 && y > 0 && currentGrid.ContainsKey((x - 1, y - 1)))
            {
                return currentGrid[(x - 1, y - 1)].Name;
            }
            else { return "default"; }
        }

        public string GetBlockNameToBottomRight(int x, int y)
        {
            if (x < gridSize - 1 && y > 0 && currentGrid.ContainsKey((x + 1, y - 1)))
            {
                return currentGrid[(x + 1, y - 1)].Name;
            }
            else { return "default"; }
        }




        public static void LogGrid(Block[][] grid)
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


    }
}
*/