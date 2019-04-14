// This is where you build your AI for the Stardash game.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// <<-- Creer-Merge: usings -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
// you can add additional using(s) here
// <<-- /Creer-Merge: usings -->>

namespace Joueur.cs.Games.Stardash
{
    /// <summary>
    /// This is where you build your AI for Stardash.
    /// </summary>
    public class AI : BaseAI
    {
        #region Properties
#pragma warning disable 0169 // the never assigned warnings between here are incorrect. We set it for you via reflection. So these will remove it from the Error List.
#pragma warning disable 0649
        /// <summary>
        /// This is the Game object itself. It contains all the information about the current game.
        /// </summary>
        public readonly Game Game;
        /// <summary>
        /// This is your AI's player. It contains all the information about your player's state.
        /// </summary>
        public readonly Player Player;
#pragma warning restore 0169
#pragma warning restore 0649

        // <<-- Creer-Merge: properties -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
        // you can add additional properties here for your AI to use
        // <<-- /Creer-Merge: properties -->>
        #endregion


        public static Game GAME;
        public static Player PLAYER;
        public static Player OPPONENT;
        public static Job CORVETTE;
        public static Job MISSILE_BOAT;
        public static Job MARTYR;
        public static Job TRANSPORT;
        public static Job MINER;
        public static Body MYTHICITE;
        public static Body SUN;
        public static Body BASE;
        public static List<Vector> ROUTE_POINTS;

        public static int spawnListIndex = 3;
        #region Methods
        /// <summary>
        /// This returns your AI's name to the game server. Just replace the string.
        /// </summary>
        /// <returns>Your AI's name</returns>
        public override string GetName()
        {
            // <<-- Creer-Merge: get-name -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            return "Prime_Directive_Violations"; // REPLACE THIS WITH YOUR TEAM NAME!
            // <<-- /Creer-Merge: get-name -->>
        }

        /// <summary>
        /// This is automatically called when the game first starts, once the Game and all GameObjects have been initialized, but before any players do anything.
        /// </summary>
        /// <remarks>
        /// This is a good place to initialize any variables you add to your AI or start tracking game objects.
        /// </remarks>
        public override void Start()
        {
            // <<-- Creer-Merge: start -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            base.Start();
            // <<-- /Creer-Merge: start -->>
            AI.GAME = this.Game;
            AI.PLAYER = this.Player;
            AI.OPPONENT = this.Player.Opponent;
            AI.CORVETTE = this.Game.Jobs[0];
            AI.MISSILE_BOAT = this.Game.Jobs[1];
            AI.MARTYR = this.Game.Jobs[2];
            AI.TRANSPORT = this.Game.Jobs[3];
            AI.MINER = this.Game.Jobs[4];
            AI.MYTHICITE = this.Game.Bodies.First(b => b.MaterialType == "mythicite");
            AI.SUN = this.Game.Bodies.First(b => b.BodyType == "sun");
            AI.ROUTE_POINTS = new List<Vector>();
            AI.BASE = this.Game.CurrentPlayer.HomeBase; ;
            var extend = AI.SUN.Radius + 32;
            for (double i = 0; i < Math.PI * 2; i += (Math.PI / 8.0))
            {
                AI.ROUTE_POINTS.Add(new Vector(Math.Cos(i) * extend + AI.SUN.X, Math.Sin(i) * extend + AI.SUN.Y));
            }
        }

        /// <summary>
        /// This is automatically called every time the game (or anything in it) updates.
        /// </summary>
        /// <remarks>
        /// If a function you call triggers an update, this will be called before that function returns.
        /// </remarks>
        public override void GameUpdated()
        {
            // <<-- Creer-Merge: game-updated -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            base.GameUpdated();
            // <<-- /Creer-Merge: game-updated -->>
        }

        /// <summary>
        /// This is automatically called when the game ends.
        /// </summary>
        /// <remarks>
        /// You can do any cleanup of you AI here, or do custom logging. After this function returns, the application will close.
        /// </remarks>
        /// <param name="won">True if your player won, false otherwise</param>
        /// <param name="reason">A string explaining why you won or lost</param>
        public override void Ended(bool won, string reason)
        {
            // <<-- Creer-Merge: ended -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            base.Ended(won, reason);
            // <<-- /Creer-Merge: ended -->>
        }


        /// <summary>
        /// This is called every time it is this AI.player's turn.
        /// </summary>
        /// <returns>Represents if you want to end your turn. True means end your turn, False means to keep your turn going and re-call this function.</returns>
        public bool RunTurn()
        {
            // <<-- Creer-Merge: runTurn -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
            Console.WriteLine("Turn #{0}", this.Game.CurrentTurn);
            Spawning();
            MinerLogic();
            TransportLogic();
            CorvetteLogic();
            return true;
            // <<-- /Creer-Merge: runTurn -->>
        }

        public void MinerLogic()
        {
            var miners = this.Player.Units.Where(u => u.Job == AI.MINER);
            var minersNearMythicite = miners.OrderBy(m => m.distance(AI.MYTHICITE));
            var mythiciteCount = Math.Max(miners.Count() / 3 - 1, 0);

            foreach (var miner in minersNearMythicite.Take(mythiciteCount))
            {
                Solver.mine(miner, new[] { AI.MYTHICITE });
                var baseBody = this.Game.CurrentPlayer.HomeBase;
                if (Extensions.remainingCapacity(miner) == 0)
                {
                    Solver.moveToward(miner, baseBody.X, baseBody.Y, baseBody.Radius, true);
                }
            }

            foreach (var miner in miners.Skip(mythiciteCount))
            {
                Solver.mine(miner, this.Game.Bodies);
                var baseBody = this.Game.CurrentPlayer.HomeBase;
                if (Extensions.remainingCapacity(miner) == 0)
                {
                    Solver.moveToward(miner, baseBody.X, baseBody.Y, baseBody.Radius, true);
                }
            }
        }
        public void TransportLogic()
        {
            string[] order = { "mythicite", "legendarium", "rarium", "genarium" };
            var transporters = this.Player.Units.Where(u => u.Job == AI.TRANSPORT);

            foreach (var transporter in transporters)
            {
                //Console.WriteLine(transporter.remainingCapacity());
                Solver.transport(transporter, this.Player.Units, order);

                var baseBody = this.Game.CurrentPlayer.HomeBase;
                if (transporter.remainingCapacity() == 0)
                {
                    Solver.moveToward(transporter, baseBody.X, baseBody.Y, baseBody.Radius, true);
                }
            }
        }
        public void CorvetteLogic()
        {
            foreach (var corvette in this.Player.Units.Where(u => u.Job == AI.CORVETTE))
            {
                Solver.attack(corvette, AI.OPPONENT.Units.Where(u => u.Job == AI.MINER));
                Solver.attack(corvette, AI.OPPONENT.Units);

            }
        }

        public void MissileBoatLogic()
        {
            foreach (var missileboat in this.Player.Units.Where(u => u.Job == AI.MISSILE_BOAT))
            {
                Solver.attack(missileboat, AI.OPPONENT.Units.Where(u => u.Job == AI.MINER), true);
                Solver.attack(missileboat, AI.OPPONENT.Units.Where(u => u.Job == AI.MINER), true);
                Solver.attack(missileboat, AI.OPPONENT.Units);

            }
        }

        public void Spawning()
        {
            var desiredUnits = new List<Job>
            {
                AI.MINER,
                AI.MINER,
                AI.MINER,
                AI.MISSILE_BOAT,
                AI.CORVETTE,
                AI.CORVETTE,
                AI.MINER,
                AI.MINER,
                AI.MINER,
                AI.TRANSPORT,
                AI.CORVETTE,
                AI.CORVETTE,
                AI.MINER,
                AI.MINER,
                AI.MINER,
                AI.CORVETTE,
                AI.CORVETTE,
            };
            while (Player.Money >= desiredUnits[spawnListIndex].UnitCost)
            {
                this.Player.HomeBase.Spawn(this.Player.HomeBase.X + this.Player.HomeBase.Radius, this.Player.HomeBase.Y, desiredUnits[spawnListIndex].Title);
                spawnListIndex++;
                if (spawnListIndex > desiredUnits.Count() - 1)
                {
                    spawnListIndex = 0;
                }
            }
        }
    }

    // <<-- Creer-Merge: methods -->> - Code you add between this comment and the end comment will be preserved between Creer re-runs.
    // you can add additional methods here for your AI to call
    // <<-- /Creer-Merge: methods -->>
    #endregion
}

