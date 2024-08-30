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
        public Emralux()
        {
            this.MaximumHealth = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\EmraluxTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\EmraluxOpponent.png";
            this.ScapeMonsterName = "Emralux";
            this.Health = this.MaximumHealth;
            this.Moves = new List<ScapeMonsterMove>() { new QuickAttack() };
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
            this.Moves = new List<ScapeMonsterMove>() { new QuickAttack() };
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
            this.Moves = new List<ScapeMonsterMove>() { new RockSmash() };
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
            this.Moves = new List<ScapeMonsterMove>() { new QuickAttack() };
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
            this.Moves = new List<ScapeMonsterMove>() { new QuickAttack() };
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
            this.Moves = new List<ScapeMonsterMove>() { new QuickAttack() };
        }
    }

    public class Sealy : ScapeMonster
    {
        public Sealy()
        {
            this.MaximumHealth = (new Random()).Next(800, 1000);
            this.BaseDamage = (new Random()).Next(800, 1000);
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\SealyTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\SealyOpponent.png";
            this.ScapeMonsterName = "Monke";
            this.Health = this.MaximumHealth;
            this.Moves = new List<ScapeMonsterMove>() { new QuickAttack() };
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
            this.Moves = new List<ScapeMonsterMove>() {new QuickAttack()};
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
        public static ScapeMonster GetRandomScapeMonster()
        {
            Random random = new Random();
            int num = random.Next(0, 100);

            switch (num)
            {/*
                case < 25://uncommon
                    return new Fuzzy();
                  
                case < 42://fairly common
                    return new Monke();
                   
                case < 55:
                    return new Inferna();

                case < 65:
                    return new Sealy();
                case < 80:
                    return new Golem();
                case < 90:
                    return new Obsadite();
                case < 97: 
                    return new Emralux(); */
                default: return new Sealy();
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

        public ScapeMonster() {
            ScapeMonsterID = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
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
