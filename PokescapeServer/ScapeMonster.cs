using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            this.Moves = new List<ScapeMonsterMove>() { new Gust(), new DragonOutrage(), new TailSmash() };
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
         
            this.Moves = new List<ScapeMonsterMove>() { new QuickAttack(), new Heal(), new PoisonDodge() };
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
            this.BaseDefence = GenerateHealth(1.3, 1.4) ;
            
            this.BaseDamage = GenerateBaseDamage(1, 1.1);
          
            this.TamedImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\GolemTamed.png";
            this.OpponentImage = $"{Pokescape.ImageFolderPath}\\scapeMonsters\\GolemOpponent.png";
            this.ScapeMonsterName = "Golem";
         
            this.Moves = new List<ScapeMonsterMove>() { new RockSmash(), new EpicBodySlam(), new Bind() };
            this.Damage = this.BaseDamage * Level;
            this.MaximumHealth = this.BaseDefence * Level;
            this.Health = this.MaximumHealth;
            Level = level;
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
            this.ScapeMonsterName = "Inferna";
   
            this.Moves = new List<ScapeMonsterMove>() { new Charge() , new Attract(), new FireBall()};
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
            this.Moves = new List<ScapeMonsterMove>() { new UnknownPower(), new Fortify(), new Epicknockout() };
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
          
            this.Moves = new List<ScapeMonsterMove>() { new Agility(), new MonkeySlash(), new Bind() };
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
            this.ScapeMonsterName = "Sealy";
            this.Moves = new List<ScapeMonsterMove>() { new TailSlap(), new Surf(), new AquaShield()  };
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
      
            this.Moves = new List<ScapeMonsterMove>() {new Earthquake(), new ObsidianSlam()};
            this.Damage = this.BaseDamage * Level;
            this.MaximumHealth = this.BaseDefence * Level;
            this.Health = this.MaximumHealth;
            Level = level;
        }
    }

    public class ScapeMonsterMove
    {
        static Dictionary<string, ScapeMonsterMove> MovesById = new();

        static int id_counter = 1000;
        public bool activeCooldown { get; set; } = false;

        public ScapeMonsterMove()
        {
//            Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            Id = (id_counter++).ToString();
            MovesById[Id] = this;
        }
        public readonly string Id;
        public string MoveDescription { get; set; }
        public int ResistanceMoveLife;
        public int repeatedDamageMoveLife;
        public bool active;
        public double ChanceOfMissing { get; set; } = -1;
        public double HealthMultiplierToAttacker { get; set; }
        public double ResistanceToAttack { get; set; } = 1;
        public double Damage { get; set; }
        public double MoveDamagePerRound { get; set; } = -1;
        public int MoveLife { get; set; }
        public string MoveName { get; set; }

        public string MoveType { get; set; }
        public double ChanceOfDodging { get; set; } = -1;
        public bool HasResistanceToRepeatedMove { get; set; }
        public bool HasMoveLife { get; set; }
        public int CurrentCooldown { get; set; }    
        public int Cooldown { get; set; }

        public bool HasCooldown { get; set; }
        public int AbsoluteCooldown { get; set; }
        public double MoveHeal { get; set; }

       
        public static ScapeMonsterMove GetMoveById(string id) 
        { 
            return MovesById[id];
        }

        readonly static Random random = new Random();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="opponent"></param>
        /// <returns> Opponent</returns>
        public static MoveResult PerformMove(ScapeMonsterMove move, ScapeMonster opponent, ScapeMonster attacker, List<ScapeMonsterMove> ActiveAttackerMoves, List<ScapeMonsterMove> ActiveOpponentMoves)
        {
            // plan for function
            // 1. calculate any healing from the move (including level and other adjustments)
            // 2. set the health variable on the move Result (optionally append to the description variable)

            // 3. calculate the base damage potentially to be inflicted by the move
            // 4. determine if the move hits
            // 5. apply multipliers for level and other vulnerabilites (fire against water type etc)
            // 6. apply resistances and shields
            // 7. determine damage from previous moves that are still in play (automatic damages)
            // 8. calculate and set the total damage on the MoveResult result that will be returned and set the description


            // 9. remove previous moves that have expired ?
            // 10. return the result


            MoveResult moveResult = new MoveResult();
            // always apply healing

            

            if (move.HasMoveLife == true)
            {
                
                ActiveAttackerMoves.Add(move);
            }
    
           
            
            double baseDamage = move.Damage;
            double damage = baseDamage * attacker.Level * 0.25;
            foreach (ScapeMonsterMove resistantMove in ActiveOpponentMoves)
            {
                
                if (resistantMove.ResistanceToAttack != 0 && (resistantMove.ResistanceMoveLife!= null && resistantMove.ResistanceMoveLife != 0))
                {
                    damage = damage-damage*resistantMove.ResistanceToAttack;
                    moveResult.description2 = $"{opponent.ScapeMonsterName} has {resistantMove.MoveName} active! They blocked {resistantMove.ResistanceToAttack*100}% of the attack!";
                    resistantMove.ResistanceMoveLife--;
                    break;
                }


            }

            if (move.MoveHeal > 0.0)
            {

                double healthBeforeHeal = attacker.Health; // apply healing
                double healthAfterHeal = attacker.Health + move.MoveHeal * attacker.MaximumHealth;

                if (healthAfterHeal > attacker.MaximumHealth)
                    healthAfterHeal = attacker.MaximumHealth;
                moveResult.hpChange = healthAfterHeal - healthBeforeHeal;
                moveResult.hpChange = Math.Round(moveResult.hpChange, 2);
                moveResult.type = "HealMove";
                moveResult.description = $"{attacker.ScapeMonsterName} used move {move.MoveName} and gained {moveResult.hpChange} hp!";
            }


           



            if (move.Damage != 0)
            {
                bool didMiss = random.NextDouble() < move.ChanceOfMissing;  // could make chance of missing dependent on attacker level
                bool didDodge = random.NextDouble() < opponent.ChanceOfDodging; // could make chance of dodging dependent on opponent level



                
                if (didMiss)
                {
                    moveResult.damage = 0.0;
                    moveResult.description = $"{attacker.ScapeMonsterName} used move {move.MoveName} but missed!";
                }
                else if (didDodge)
                {
                    moveResult.damage = 0.0;
                    moveResult.description = $"{attacker.ScapeMonsterName} used move {move.MoveName} but {opponent.ScapeMonsterName} dodged!";
                }
                else if (move.HealthMultiplierToAttacker > 0)
                {
                    if (move.HealthMultiplierToAttacker < 1)
                    {
                        moveResult.hpChange = attacker.MaximumHealth - attacker.MaximumHealth * move.HealthMultiplierToAttacker;


                        moveResult.description2 = $"{attacker.ScapeMonsterName} lost {moveResult.hpChange} hp!";
                        moveResult.damage = damage;
                        moveResult.description = $"{attacker.ScapeMonsterName} used move {move.MoveName} and dealt {damage} damage!";
                    }
                    else
                    {


                        moveResult.damage = damage;
                        moveResult.description = $"{attacker.ScapeMonsterName} used move {move.MoveName} and dealt {damage} damage!";
                        
                        moveResult.description2 = $"{attacker.ScapeMonsterName} used move {move.MoveName} and gained {moveResult.hpChange} hp!";
                    }
                }
                else
                {
                    moveResult.damage = damage;
                    moveResult.description = $"{attacker.ScapeMonsterName} used move {move.MoveName} and dealt {damage} damage!";
                }
            }
            else
            {
                moveResult.description = $"{attacker.ScapeMonsterName} used move {move.MoveName}!";
            }

            

            // still todo: apply move results from previous results, including shields etc 

            //apply damage to attacker first
            foreach (ScapeMonsterMove repeatedDamageMove in ActiveOpponentMoves)
            {
                if (repeatedDamageMove.MoveDamagePerRound != 0 && repeatedDamageMove.MoveDamagePerRound != null && repeatedDamageMove.repeatedDamageMoveLife != 0 && repeatedDamageMove.repeatedDamageMoveLife != null)
                {
                    double damage1 = repeatedDamageMove.MoveDamagePerRound * opponent.Damage;
                    moveResult.damage += damage1;
                    moveResult.description += Environment.NewLine + $"{opponent.ScapeMonsterName}'s {repeatedDamageMove.MoveName} dealt {damage1} damage!";
                    repeatedDamageMove.repeatedDamageMoveLife--;
                }
            }

            return moveResult;
            
        }


    }

    
        


    public class MoveResult
    {
        public double damage;
        public double hpChange;
        public string description = "";
        public string description2 = "";
        public double extradamage;
        public string type = "";

    }
    public class EpicBodySlam : ScapeMonsterMove
    {
        public EpicBodySlam()
        {
            this.Damage = 1.4;
            this.HasMoveLife = false;
            this.HealthMultiplierToAttacker = 0.9;
            this.MoveName = "Epic Body Slam";
            this.MoveType = "Attack";
        }

     



    }

    public class Bind : ScapeMonsterMove
    {
        public Bind()
        {
            this.HealthMultiplierToAttacker = 1.1;
            this.HasMoveLife = false;
            this.Cooldown = 3;
            this.MoveName = "Bind";
            this.MoveType = "Heal";
        }

        



    }
    
    public class Heal : ScapeMonsterMove
    {
        public Heal()
        {
            this.MoveHeal = 0.3;
            this.HasMoveLife = false;
            this.MoveName = "Heal";
            this.HealthMultiplierToAttacker = 0.0;
            this.MoveType = "Heal";
        }
    }

    public class PoisonDodge : ScapeMonsterMove
    {
    
        public PoisonDodge()
        {
            this.ResistanceToAttack = 1;
            this.ResistanceMoveLife = 1;
            this.MoveDamagePerRound = 0.2;
            this.Cooldown = 5;
            this.repeatedDamageMoveLife = 5;
            this.HasMoveLife = true;
            this.MoveName = "Poison Dodge";
            this.MoveType = "Heal";
        } 
    
    
    
    
    
    
    
    }


    public class QuickAttack : ScapeMonsterMove
    {
        public QuickAttack()
        {
            this.Damage = 1;
            this.MoveName = "Quick Attack";
            this.MoveDescription = "Standard attack";
            this.MoveType = "Attack";
        }
       
    }
    public class Epicdrain : ScapeMonsterMove
    {
        public Epicdrain()
        {
            
            this.Damage = 1;
            this.MoveName = "Epic drain";
            this.HealthMultiplierToAttacker = 1.15;
            this.MoveType = "Attack";
        }
     
    }

    public class RockSmash : ScapeMonsterMove
    {
        public RockSmash()
        {
            this.Damage = 1.2;
            this.MoveName = "Rock Smash";
            this.MoveDescription = "Smash";
        }
     
    }

    public class UltraRay : ScapeMonsterMove
    {
        public UltraRay()
        {
            this.Damage = 0;
            this.MoveName = "Ultra Ray";
            this.MoveLife = 2;
            this.MoveDamagePerRound = 2.5;
        }

    }

    public class Resist : ScapeMonsterMove
    {
        public Resist()
        {
            this.Damage = 0;
            this.MoveName = "Resist";
            this.MoveLife = 999999;
            this.Cooldown = 999999;
            this.ResistanceToAttack = 0.1;
        }

    }

    public class Hysteria : ScapeMonsterMove
    {
        public Hysteria()
        {
            this.Damage = 0.5;
            this.MoveName = "Hysteria";
            this.MoveLife = 99999999;
            this.MoveDamagePerRound = 0.04;
        }

    }
    public class Charge : ScapeMonsterMove 
    {
        public Charge()
        {
            this.Damage = 1.1;
            this.MoveName = "Charge";
           
            
        }

    }

    public class FireBall : ScapeMonsterMove
    {
        public FireBall()
        {
            this.Damage = 1.3;
            this.MoveName = "FireBall";
            this.ChanceOfMissing = 0.3;
        }
    }


    public class Attract : ScapeMonsterMove
    {
        public Attract()
        {
            
            this.MoveName = "Attract";
            this.ChanceOfMissing = 0.3;
            this.ResistanceToAttack = 0.25;
            this.ResistanceMoveLife = 2;
            this.Cooldown = 3;
        }
    }

    public class MonkeySlash : ScapeMonsterMove
    {
        public MonkeySlash()
        {
            this.Damage = 1.1;
            this.MoveName = "Monkey Slash";


        }

    }

    public class TailSlap : ScapeMonsterMove
    {
        public TailSlap()
        {
            this.Damage = 1.1;
            this.MoveName = "Tail Slap";
            

        }

    }
    public class Surf : ScapeMonsterMove
    {
        public Surf()
        {
            this.Damage = 1.25;
            this.MoveName = "Surf";
            this.ChanceOfMissing = 0.25;


        }

    }
    public class AquaShield : ScapeMonsterMove
    {
        public AquaShield()
        {
            this.ResistanceToAttack = 0.5;
            this.ResistanceMoveLife = 3;
            this.Cooldown = 3;
        }
    }


    public class Agility : ScapeMonsterMove
    {
        public Agility()
        {
            this.ChanceOfDodging = 0.6;
            this.MoveName = "Agility";
            this.Cooldown = 3;
            this.MoveLife = 3;

        }

    }

    public class Gust : ScapeMonsterMove
    {
        public Gust()
        {
            this.Damage = 1.1;
            this.MoveName = "Gust";
            

        }

    }

    public class DragonOutrage : ScapeMonsterMove
    {
        public DragonOutrage()
        {
            this.Damage = 1.4;
            this.HealthMultiplierToAttacker = 0.9;
            this.MoveName = "Dragon Outrage";
        }
    }
    public class TailSmash : ScapeMonsterMove
    {
        public TailSmash()
        {
            this.Damage = 0.8;
            this.ChanceOfDodging = 0.5;
            this.MoveName = "Tail Smash";
        }
    }

    public class Earthquake : ScapeMonsterMove
    {
        public Earthquake()
        {
            this.MoveDamagePerRound = 0.6;
            this.repeatedDamageMoveLife = 2;
            this.MoveName = "Earthquake";
            this.Cooldown = 3;
        }
    }
    public class ObsidianSlam : ScapeMonsterMove
    {
        public ObsidianSlam()
        {
            this.Damage = 2;

            this.MoveName = "Obsidian Slam";
            this.ChanceOfMissing = 0.5;
        }
    }

    public class UnknownPower : ScapeMonsterMove
    {
        public UnknownPower()
        {
            this.Damage = 1.3;

            this.MoveName = "Unknown Power";
            
        }
    }

    public class Fortify : ScapeMonsterMove
    {
        public Fortify()
        {
            this.HealthMultiplierToAttacker = 1.2;

            this.MoveName = "Fortify";
            this.Cooldown = 2;

        }
    }

    public class Epicknockout : ScapeMonsterMove
    {
        public Epicknockout()
        {
            this.Damage = 3;
            this.Cooldown = 99999;
            this.MoveName = "Epic knockout";
            this.HealthMultiplierToAttacker = 0.6;
        }
    }




    public class ScapeMonster
    {
        public static ScapeMonster GetRandomScapeMonster(User user)
        {
            int level = user.GetUsersHighestLevelScapemonster();
            Random random = new Random();
            int num = random.Next(0, 100);

            //return new Sealy(level);
            if (level <= 24)
            {

                switch (num)
                {
                    case < 25://uncommon

                        return new Fuzzy(level);

                    case < 42://fairly common
                        return new Monke(level);

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
            else
            {
                switch (num)
                {
                    case < 25://uncommon

                        
                        return new Kahuna(level);

                    case < 42://fairly common
                        return new Monke(level);

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
                    default: return new Fuzzy(level);
                }
            }
        }

        public void UpdateScapeMonsterStats(ScapeMonster scapemonster)
        {
            double health_percentage = scapemonster.Health / scapemonster.MaximumHealth;
            scapemonster.Damage = scapemonster.BaseDamage * scapemonster.Level;
            scapemonster.MaximumHealth = scapemonster.BaseDefence * scapemonster.Level;
            scapemonster.Health = scapemonster.MaximumHealth * health_percentage;
            return;

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

        public static int uniqueScapeMonsterId = 10000;

        public ScapeMonster(int level) {

            Level = level;
            ScapeMonsterID = (uniqueScapeMonsterId++).ToString();
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

        public static ScapeMonster Levelup(ScapeMonster scapemonster)
        {
            scapemonster.Level += 1;
            scapemonster.Damage = scapemonster.Level*scapemonster.BaseDamage;
            scapemonster.Health = scapemonster.Level*scapemonster.BaseDamage;
            return scapemonster;
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
        public double ChanceOfDodging = 0.1;

    }
}
