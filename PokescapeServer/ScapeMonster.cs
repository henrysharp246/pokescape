using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PokescapeServer
{
    public class Emralux : ScapeMonster
    {
        public Emralux(int level) : base(level)
        {
            this.BaseDefence = GenerateHealth(1.1, 1.2);
           
            this.BaseDamage = GenerateBaseDamage(1.1, 1.2);
          
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\EmraluxTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\EmraluxOpponent.png";
            this.ScapeMonsterName = "Emralux";
            this.Health = this.MaximumHealth;
            this.Moves = new List<ScapeMonsterMove>() { new QuickAttack() };
            this.Damage = this.BaseDamage * Level;
            this.MaximumHealth = this.BaseDefence * Level;
            this.Health = this.MaximumHealth;
            Level = level;

        }
    }

    public class Fuzzy : ScapeMonster
    {
        public Fuzzy(int level) : base(level)
        {
            this.BaseDefence = GenerateHealth(1, 1.1);
      
            this.BaseDamage = GenerateBaseDamage(1, 1.1);
          
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\FuzzyTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\FuzzyOpponent.png";
            this.ScapeMonsterName = "Fuzzy";
         
            this.Moves = new List<ScapeMonsterMove>() { new QuickAttack() };
            this.Damage = this.BaseDamage * Level;
            this.MaximumHealth = this.BaseDefence * Level;
            this.Health = this.MaximumHealth;
            Level = level;
        }
    }

    public class Golem : ScapeMonster
    {
        public Golem(int level) : base(level)
        {
            this.BaseDefence = GenerateHealth(1.3, 1.4);
            
            this.BaseDamage = GenerateBaseDamage(1, 1.1);
          
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\GolemTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\GolemOpponent.png";
            this.ScapeMonsterName = "Golem";
         
            this.Moves = new List<ScapeMonsterMove>() { new RockSmash() };
        }
    }
    public class Inferna : ScapeMonster
    {
        public Inferna(int level) : base(level)
        {
            this.BaseDefence = GenerateHealth(0.9, 1.1);
            
            this.BaseDamage = GenerateBaseDamage(1.1, 1.3);
          
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\InfernaTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\InfernaOpponent.png";
            this.ScapeMonsterName = "Fuzzy";
   
            this.Moves = new List<ScapeMonsterMove>() { new QuickAttack() };
            this.Damage = this.BaseDamage * Level;
            this.MaximumHealth = this.BaseDefence * Level;
            this.Health = this.MaximumHealth;
            Level = level;
        }
    }

    public class Kahuna : ScapeMonster
    {
        public Kahuna(int level) : base(level)
        {
            this.BaseDefence = GenerateHealth(1.4, 1.5);
     
            this.BaseDamage = GenerateBaseDamage(1.4, 1.5);
          
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\KahunaTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\KahunaOpponent.png";
            this.ScapeMonsterName = "Kahuna";
            this.Moves = new List<ScapeMonsterMove>() { new QuickAttack() };
            this.Damage = this.BaseDamage * Level;
            this.MaximumHealth = this.BaseDefence * Level;
            this.Health = this.MaximumHealth;
            Level = level;

        }
    }

    public class Monke : ScapeMonster
    {
        public Monke(int level):base(level)
        {
            this.BaseDefence = GenerateHealth(0.6, 0.8);
        
            this.BaseDamage = GenerateBaseDamage(1.4, 1.6);
          
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\MonkeTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\MonkeOpponent.png";
            this.ScapeMonsterName = "Monke";
          
            this.Moves = new List<ScapeMonsterMove>() { new QuickAttack() };
            this.Damage = this.BaseDamage * Level;
            this.MaximumHealth = this.BaseDefence * Level;
            this.Health = this.MaximumHealth;
            Level = level;

        }
    }

    public class Sealy : ScapeMonster
    {
        public Sealy(int level) : base(level)
        {
            this.BaseDefence = GenerateHealth(1, 1.1);
            this.BaseDamage = GenerateBaseDamage(1.1, 1.2);
          
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\SealyTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\SealyOpponent.png";
            this.ScapeMonsterName = "Monke";
            this.Moves = new List<ScapeMonsterMove>() { new QuickAttack() };
            this.Damage = this.BaseDamage * Level;
            this.MaximumHealth = this.BaseDefence * Level;
            this.Health = this.MaximumHealth;
            Level = level;
        }
    }

    public class Obsadite : ScapeMonster
    {
        public Obsadite(int level):base(level)
        {
            this.BaseDefence = GenerateHealth(1.3, 1.4);
          
            this.BaseDamage = GenerateBaseDamage(1, 1.2);
          
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\ObsaditeTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\ObsaditeOpponent.png";
            this.ScapeMonsterName = "Obsadite";
      
            this.Moves = new List<ScapeMonsterMove>() {new QuickAttack()};
            this.Damage = this.BaseDamage * Level;
            this.MaximumHealth = this.BaseDefence * Level;
            this.Health = this.MaximumHealth;
            Level = level;
        }
    }

    public class ScapeMonsterMove
    {
        
        public ScapeMonsterMove()
        {
            Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        }
        public string Id { get; set; }
        public string MoveDescription { get; set; }
        public int MoveDamage { get; set; }
        public int MoveDamagePerRound { get; set; }
        public int MoveLife { get; set; }
        public string MoveName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="opponent"></param>
        /// <returns> Opponent</returns>
        public virtual ScapeMonster PerformMove(ScapeMonster opponent)
        {
            return null;
        }
    }
    public class QuickAttack : ScapeMonsterMove
    {
        public QuickAttack()
        {
            this.MoveDamage = 100;
            this.MoveName = "Quick Attack";
            this.MoveDescription = "Standard attack";
        }
        public override ScapeMonster PerformMove(ScapeMonster opponent)
        {

            var newOpponentAfterDamage = opponent;
            newOpponentAfterDamage.Health -= MoveDamage;
            return newOpponentAfterDamage;
         
        }
    }
    public class Epicdrain : ScapeMonsterMove
    {
        public Epicdrain()
        {
            
            this.MoveDamage = 0;
            this.MoveName = "Epic drain";
            this.MoveDescription = "Standard attack";
            this.MoveLife = 100;
            this.MoveDamagePerRound = 1;
        }
        public override ScapeMonster PerformMove(ScapeMonster opponent)
        {

            var newOpponentAfterDamage = opponent;
            newOpponentAfterDamage.Health -= MoveDamage;
            return newOpponentAfterDamage;

        }
    }

    public class RockSmash : ScapeMonsterMove
    {
        public RockSmash()
        {
            this.MoveDamage = 10;
            this.MoveName = "Rock Smash";
            this.MoveDescription = "Smash";
        }
        public override ScapeMonster PerformMove(ScapeMonster opponent)
        {

            var newOpponentAfterDamage = opponent;
            newOpponentAfterDamage.Health -= MoveDamage;
            return newOpponentAfterDamage;

        }
    }
    public class ScapeMonster
    {
        public static ScapeMonster GetRandomScapeMonster(int level)
        {
            Random random = new Random();
            int num = random.Next(0, 100);
          
            
            
      

            switch (num)
            {
                case < 25://uncommon
                    
                    return new Fuzzy( level);
                  
                case < 42://fairly common
                    return new Monke( level);
                   
                case < 55:
                    return new Inferna(level);

                case < 65:
                    return new Sealy(level);
                case < 80:
                    return new Golem(level);
                case < 90:
                    return new Obsadite(level);
                case < 97: 
                    return new Emralux(level); 
                default: return new Sealy(level);
            }
        }

        public ScapeMonsterMove GetMove(string id)
        {
            return this.Moves.First(move => move.Id == id);
        }

        public ScapeMonsterMove GetRandomMove()
        {
            Random rnd = new Random();
            int num = rnd.Next(0, Moves.Count-1);
            return this.Moves[num];
           
        }

        public ScapeMonster(int level) {

            Level = level;
            ScapeMonsterID = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        }
      
        public double GenerateHealth(double min, double max)
        {
            var rand = new Random();
            double randomDouble = rand.NextDouble();

            Console.WriteLine(randomDouble);
            // Scale it to the range [min, max]
            double Health = (double)(min + randomDouble * (max - min));

            // Round to 2 decimal places
            Health = Math.Round(Health, 2);
            return Health;
        }

        public double GenerateBaseDamage(double min, double max)
        {
            var rand = new Random();
            double randomDouble = rand.NextDouble();

            Console.WriteLine(randomDouble);
            // Scale it to the range [min, max]
            double Attack = (double)(min + randomDouble * (max - min));

            // Round to 2 decimal places
            Attack = Math.Round(Attack, 2);
            return Attack;
        }

        
        public string TamedImage { get; set; }
        public string OpponentImage { get; set; }
        public string ScapeMonsterID { get; set; }
        public (int xCord, int yCord) ScapemonsterCoordinates { get; set; }
        public string ScapeMonsterName { get; set; }
        public double BaseDefence { get; set; }
        public double MaximumHealth { get; set; }
        public double Damage {  get; set; }
        public bool IsBoss { get; set; }
        public double Health { get; set; }
        public double BaseDamage { get; set; }
        public List<ScapeMonsterMove> Moves { get; set; } = new List<ScapeMonsterMove>();
        public int Level { get; set; }
    }
}
