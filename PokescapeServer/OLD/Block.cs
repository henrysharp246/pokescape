using System;
using System.Collections.Generic;
using System.Linq;

namespace PokescapeServer.OLD
{
    public interface IBlock
    {
        string Name { get; set; }
        bool CanPass { get; set; }
        List<(Type block, decimal likelihood)> BlocksAllowedNextTo { get; set; }

        IBlock CreateNeighborBlock()
        {
            Random random = new Random();
            double randomVal = random.NextDouble(); // Generate a random number between 0 and 1
            decimal cumulative = 0;

            foreach (var blockInfo in BlocksAllowedNextTo)
            {
                cumulative += blockInfo.likelihood;
                if ((double)cumulative > randomVal)
                {
                    return (IBlock)Activator.CreateInstance(blockInfo.block);
                }
            }

            // In case none match (which shouldn't happen if likelihoods are well-defined), return the first one
            return (IBlock)Activator.CreateInstance(BlocksAllowedNextTo[0].block);
        }
    }

    public abstract class BlockBase : IBlock
    {
        public string Name { get; set; }
        public bool CanPass { get; set; }
        public abstract List<(Type block, decimal likelihood)> BlocksAllowedNextTo { get; set; }

      
    }

    public class WallBlock : BlockBase
    {
        public override List<(Type block, decimal likelihood)> BlocksAllowedNextTo { get; set; }

        public WallBlock()
        {
            CanPass = false;
        }
    }

    public class FloorBlock : BlockBase
    {
        public override List<(Type block, decimal likelihood)> BlocksAllowedNextTo { get; set; }

        public FloorBlock()
        {
            CanPass = false;
        }
    }

    public class StoneWallBlock : WallBlock
    {
        public StoneWallBlock()
        {
            this.Name = "STONE_WALL_BLOCK";
            this.BlocksAllowedNextTo = new List<(Type block, decimal likelihood)>()
            {
                (typeof(StoneWallBlock), 0.7m),
                (typeof(RedWallBlock), 0.1m)
            };
        }
    }

    public class RedWallBlock : WallBlock
    {
        public RedWallBlock()
        {
            this.Name = "RED_WALL_BLOCK";
            this.BlocksAllowedNextTo = new List<(Type block, decimal likelihood)>()
            {
                (typeof(StoneWallBlock), 0.7m),
                (typeof(LavaBlock), 0.1m)
            };
        }
    }

    public class RedFloorBlock : FloorBlock
    {
        public RedFloorBlock()
        {
            this.Name = "RED_FLOOR_BLOCK";
            this.BlocksAllowedNextTo = new List<(Type block, decimal likelihood)>()
            {
                (typeof(LavaBlock), 0.6m),
                (typeof(StoneFloorBlock), 0.2m)
            };
        }
    }

    public class StoneFloorBlock : FloorBlock
    {
        public StoneFloorBlock()
        {
            this.Name = "STONE_FLOOR_BLOCK";
            this.BlocksAllowedNextTo = new List<(Type block, decimal likelihood)>()
            {
                (typeof(StoneWallBlock), 0.1m),
                (typeof(WaterBlock), 0.1m),
                (typeof(StoneFloorBlock), 0.1m)
            };
        }
    }

    public class WaterBlock : FloorBlock
    {
        public WaterBlock()
        {
            this.Name = "WATER_BLOCK";
            this.BlocksAllowedNextTo = new List<(Type block, decimal likelihood)>()
            {
                (typeof(WaterBlock), 0.8m),
                (typeof(StoneFloorBlock), 0.2m)
            };
        }
    }

    public class LavaBlock : FloorBlock
    {
        public LavaBlock()
        {
            this.Name = "LAVA_BLOCK";
            this.BlocksAllowedNextTo = new List<(Type block, decimal likelihood)>(); // Initialize with empty list
        }
    }
}
