using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PokescapeServer
{
    public class Emralux : ScapeMonster
    {
        public Emralux()
        {
            this.Health = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\EmraluxTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\EmraluxOpponent.png";
          
        }
    }

    public class Fuzzy : ScapeMonster
    {
        public Fuzzy()
        {
            this.Health = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\FuzzyTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\FuzzyOpponent.png";

        }
    }

    public class Golem : ScapeMonster
    {
        public Golem()
        {
            this.Health = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\GolemTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\GolemOpponent.png";

        }
    }
    public class Inferna : ScapeMonster
    {
        public Inferna()
        {
            this.Health = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\InfernaTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\InfernaOpponent.png";

        }
    }

    public class Kahuna : ScapeMonster
    {
        public Kahuna()
        {
            this.Health = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\KahunaTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\KahunaOpponent.png";

        }
    }

    public class Monke : ScapeMonster
    {
        public Monke()
        {
            this.Health = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\MonkeTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\MonkeOpponent.png";

        }
    }

    public class Obsadite : ScapeMonster
    {
        public Obsadite()
        {
            this.Health = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\ObsaditeTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\ObsaditeOpponent.png";

        }
    }


    public class ScapeMonster
    {
        public ScapeMonster() {
            Random random = new Random();
            int num = random.Next(0, 100);

            switch (num)
            {
                case < 25://uncommon
                    break;
                case < 45://fairly common
                    break;
                case < 60://most common
                    break;
                case < 75://uncommon
                    break;
                case < 90:
                    break;

            }
        }
        public class ScapeMonsterMove
        {
            public int MoveDamage { get; set; }
            public string MoveName { get; set; }
            public Type MoveType { get; set; }
        }

        public string TamedImage { get; set; }
        public string OpponentImage { get; set; }
        public string ScapeMonsterID { get; set; }
        public (int xCord, int yCord) ScapemonsterCoordinates { get; set; }
        public string ScapeMonsterName { get; set; }
        public double MaximumHealth { get; set; }
        public bool IsBoss { get; set; }
        public double Health { get; set; }
        public double BaseDamage { get; set; }
        public List<ScapeMonsterMove> Moves { get; set; } = new List<ScapeMonsterMove>();
        public int Level { get; set; }
    }
}
