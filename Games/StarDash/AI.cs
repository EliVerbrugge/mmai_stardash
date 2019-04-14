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
        public static List<Body> VALUE_ORDERED_ASTEROIDS;
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
            var extend = AI.SUN.Radius + 100;
            AI.VALUE_ORDERED_ASTEROIDS = this.Game.Bodies.Where(b => b.MaterialType != "none").OrderBy(b => b.MaterialType.getValue()).ToList();
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
            Console.WriteLine("Turn #{0} - {1}v{2} {3}v{4}", this.Game.CurrentTurn, this.Player.VictoryPoints, this.Player.Opponent.VictoryPoints, this.Player.Units.Count(u => u.Job == AI.MINER), this.Player.Opponent.Units.Count(u => u.Job == AI.MINER));
            Spawning();

            var miners = this.Player.Units.Where(u => u.Job == AI.MINER).ToArray();
            if (this.Player.VictoryPoints + this.Player.Units.Sum(m => m.Mythicite) > AI.GAME.MythiciteAmount / 2)
            {
                BringItIn();
            }

            if (miners.Length > 10)
            {
                MinerLogic(miners.Skip(2));
                OwnIt(miners[0], miners[1]);
            }
            else
            {
                MinerLogic(miners);
            }

            TransportLogic();
            CorvetteLogic();
            MissileBoatLogic();
            return true;
            // <<-- /Creer-Merge: runTurn -->>
        }

        public void BringItIn()
        {
            foreach (var unit in AI.PLAYER.Units.Where(u => u.Mythicite > 0))
            {
                Solver.moveToward(unit, AI.PLAYER.HomeBase.X, AI.PLAYER.HomeBase.Y, AI.PLAYER.HomeBase.Radius, true);
            }
        }

        public void MinerLogic(IEnumerable<Unit> miners)
        {
            foreach (var miner in miners)
            {
                if (miners.Count() > 15)
                {
                    Solver.mine(miner, new[] { AI.MYTHICITE }, true, true);
                }

                var predictAndDash = AI.GAME.CurrentTurn >= (AI.GAME.OrbitsProtected - 2);
                Solver.mine(miner, AI.GAME.Bodies.Where(b => b.MaterialType == "legendarium"), predictAndDash, predictAndDash);
                Solver.mine(miner, AI.GAME.Bodies.Where(b => b.MaterialType == "rarium"), predictAndDash, predictAndDash);
                Solver.mine(miner, AI.GAME.Bodies.Where(b => b.MaterialType == "genarium"), predictAndDash, predictAndDash);
                var baseBody = this.Game.CurrentPlayer.HomeBase;
                if (miner.remainingCapacity() == 0)
                {
                    Solver.moveToward(miner, baseBody.X, baseBody.Y, baseBody.Radius, true);
                }
            }
        }

        public void OwnIt(Unit miner1, Unit miner2)
        {
            var transports = this.Player.Units.Where(u => u.Job == AI.TRANSPORT).ToArray();
            var didMine = false;
            if (miner1.inMiningRangeThisTurn(AI.MYTHICITE) && !miner1.IsBusy)
            {
                if (miner1.remainingCapacity() > 0)
                {
                    Solver.mine(miner1, new[] { AI.MYTHICITE });
                    didMine = true;
                    if (miner2.Energy < 10)
                    {
                        Solver.moveToward(miner2, AI.PLAYER.HomeBase.X, AI.PLAYER.HomeBase.Y, 0, true);
                    }
                    else
                    {
                        Solver.moveAheadOf(miner2, AI.MYTHICITE, 2);
                    }
                }
                else
                {
                    var transport = transports.FirstOrDefault(t => t.canTransferThisTurn(miner1));
                    if (transport != null)
                    {
                        Solver.moveToward(transport, miner1.X, miner1.Y, transport.Job.Range);
                        Solver.transfer(transport, miner1, "mythicite", 1);
                        Solver.mine(miner1, new[] { AI.MYTHICITE });
                        didMine = true;
                        if (miner2.Energy < 10)
                        {
                            Solver.moveToward(miner2, AI.PLAYER.HomeBase.X, AI.PLAYER.HomeBase.Y, 0, true);
                        }
                        else
                        {
                            Solver.moveAheadOf(miner2, AI.MYTHICITE, 2);
                        }
                    }
                }
            }

            if (!didMine && miner2.inMiningRangeThisTurn(AI.MYTHICITE) && !miner2.IsBusy)
            {
                if (miner2.remainingCapacity() > 0)
                {
                    Solver.mine(miner2, new[] { AI.MYTHICITE });
                    if (miner1.Energy < 10)
                    {
                        Solver.moveToward(miner2, AI.PLAYER.HomeBase.X, AI.PLAYER.HomeBase.Y, 0, true);
                    }
                    else
                    {
                        Solver.moveAheadOf(miner1, AI.MYTHICITE, 2);
                    }
                }
                else
                {
                    var transport = transports.FirstOrDefault(t => t.canTransferThisTurn(miner2));
                    if (transport != null)
                    {
                        Solver.moveToward(transport, miner2.X, miner2.Y, transport.Job.Range);
                        Solver.transfer(transport, miner2, "mythicite", 1);
                        Solver.mine(miner2, new[] { AI.MYTHICITE });
                        if (miner1.Energy < 10)
                        {
                            Solver.moveToward(miner2, AI.PLAYER.HomeBase.X, AI.PLAYER.HomeBase.Y, 0, true);
                        }
                        else
                        {
                            Solver.moveAheadOf(miner1, AI.MYTHICITE, 2);
                        }
                    }
                }
            }
            else
            {
                Solver.mine(miner1, new[] { AI.MYTHICITE }, true, true);
                Solver.mine(miner2, new[] { AI.MYTHICITE }, true, true);
            }

            foreach (var transport in transports)
            {
                if (transport.remainingCapacity() == 0)
                {
                    Solver.moveToward(transport, AI.PLAYER.HomeBase.X, AI.PLAYER.HomeBase.Y, 0, true);
                }
                else
                {
                    Solver.moveAheadOf(transport, AI.MYTHICITE, 2);
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
                Solver.missileNearest(missileboat);
                Solver.moveAheadOf(missileboat, AI.MYTHICITE, 4);
            }
        }

        public void Spawning()
        {
            var desiredUnits = new List<Job>
            {
                AI.MINER,
                AI.MINER,
                AI.MINER,
                AI.MINER,
                AI.MINER,
                AI.MINER,
                AI.MINER,
                AI.MINER,
                AI.MINER,
                AI.MINER,
                AI.MINER,
                AI.MINER,
                AI.MINER,
                AI.TRANSPORT,
                AI.MINER,
            };
            while (Player.Money >= desiredUnits[spawnListIndex].UnitCost)
            {
                this.Player.HomeBase.Spawn(this.Player.HomeBase.X + this.Player.HomeBase.Radius, this.Player.HomeBase.Y, desiredUnits[spawnListIndex].Title);
                spawnListIndex++;
                if (spawnListIndex >= desiredUnits.Count)
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

