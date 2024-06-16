Pokescape uses C# as the back end and server of the application. 

Pokescape adopts js for the front end / client.

Pokescape utilizes websockets for the front <-> back end communication.

 public static List<List<IBlock>> CreateGrid(int size)
        {
            List<List<IBlock>> grid = new List<List<IBlock>>();

            for (int x = 0; x < size; x++)
            {
                grid.Add(new List<IBlock>());
                for (int y = 0; y < size; y++)
                {
                    grid[x].Add(null); // Initialize with null
                }
            }

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        grid[0][0] = new StoneWallBlock();
                    }
                    else
                    {
                        IBlock previousElement = null;
                        if (y > 0)
                        {
                            previousElement = grid[x][y - 1];
                        }
                        if (previousElement == null && x > 0)
                        {
                            previousElement = grid[x - 1][y];
                        }
                        if (previousElement != null)
                        {
                            grid[x][y] = previousElement.CreateNeighborBlock();
                        }
                    }
                }
            }
