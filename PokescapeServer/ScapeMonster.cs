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
            this.MaximumHealth = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\EmraluxTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\EmraluxOpponent.png";
            this.ScapeMonsterName = "Emralux";
            this.Health = this.MaximumHealth;
        }
    }

    public class Fuzzy : ScapeMonster
    {
        public Fuzzy()
        {
            this.MaximumHealth = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\FuzzyTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\FuzzyOpponent.png";
            this.ScapeMonsterName = "Fuzzy";
            this.Health = this.MaximumHealth;
        }
    }

    public class Golem : ScapeMonster
    {
        public Golem()
        {
            this.MaximumHealth = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\GolemTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\GolemOpponent.png";
            this.ScapeMonsterName = "Golem";
            this.Health = this.MaximumHealth;
        }
    }
    public class Inferna : ScapeMonster
    {
        public Inferna()
        {
            this.MaximumHealth = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\InfernaTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\InfernaOpponent.png";
            this.ScapeMonsterName = "Fuzzy";
            this.Health = this.MaximumHealth;
        }
    }

    public class Kahuna : ScapeMonster
    {
        public Kahuna()
        {
            this.MaximumHealth = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\KahunaTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\KahunaOpponent.png";
            this.ScapeMonsterName = "Kahuna";
            this.Health = this.MaximumHealth;
        }
    }

    public class Monke : ScapeMonster
    {
        public Monke()
        {
            this.MaximumHealth = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\MonkeTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\MonkeOpponent.png";
            this.ScapeMonsterName = "Monke";
            this.Health = this.MaximumHealth;
        }
    }

    public class Obsadite : ScapeMonster
    {
        public Obsadite()
        {
            this.MaximumHealth = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\ObsaditeTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\ObsaditeOpponent.png";
            this.ScapeMonsterName = "Obsadite";
            this.Health = this.MaximumHealth;
        }
    }


    public class ScapeMonster
    {
        public static ScapeMonster GetRandomScapeMonster()
        {
            Random random = new Random();
            int num = random.Next(0, 100);

            switch (num)
            {
                case < 25://uncommon
                    return new Fuzzy();
                  
                case < 45://fairly common
                    return new Golem();
                   
                case < 60://most common
                    return new Inferna();
                    
                case < 75://uncommon
                    return new Monke();
                case < 90:
                    return new Emralux();
                case < 100: 
                    return new Kahuna();
                default: return new Inferna();
            }
        }
        public ScapeMonster() {
          
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
