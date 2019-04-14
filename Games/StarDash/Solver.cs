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
    public static class Solver
    {
        public static void mineInRange(Unit miner)
        {
            foreach (var body in AI.GAME.Bodies) // TODO: check range
            {
                if (body.MaterialType != "none" && miner.distance(body) <= miner.Job.Range)
                {
                    miner.Mine(body);
                }
            }
        }

        public static void moveToward(Unit unit, double x, double y, double range = 0)
        {
            if (unit.Moves == 0)
            {
                return;
            }

            var dx = x - unit.X;
            var dy = y - unit.Y;
            var distance = Solver.distance(dx, dy);
            if (distance < range - .01)
            {
                return;
            }

            var magnitude = Math.Min(unit.Moves - .01, distance - range + .01);
            unit.Move(unit.X + (dx / distance) * magnitude, unit.Y + (dy / distance) * magnitude);
        }

        public static void mine(Unit miner, IEnumerable<Body> bodies)
        {
            if (miner.Acted || miner.remainingCapacity() == 0)
            {
                return;
            }
            var nearest = bodies.Where(b => b.Amount > 0 && b.MaterialType != "none").MinByValue(b => b.distance(miner));
            if (nearest == null)
            {
                return;
            }

            moveToward(miner, nearest.X, nearest.Y, miner.Job.Range + nearest.Radius);

            if (miner.distance(nearest) < miner.Job.Range + nearest.Radius)
            {
                miner.Mine(nearest);
            }
        }

        public static void transport(Unit transport, IEnumerable<Body> miners, string[] order)
        {
            if(transport.Acted)
            {
                return;
            }

            if (transport.remainingCapacity() == transport.Job.CarryLimit)
            {
                //var nearest = miners.Where(b => b.remaining > 0 && b.MaterialType == mineral);
            }
        }

        public static void mine(Unit miner, IEnumerable<Body> bodies, string mineral)
        {
            if (miner.Acted || miner.remainingCapacity() == 0)
            {
                return;
            }
            var nearest = bodies.Where(b => b.Amount > 0 && b.MaterialType == mineral).MinByValue(b => b.distance(miner));
            if (nearest == null)
            {
                return;
            }

            moveToward(miner, nearest.X, nearest.Y, miner.Job.Range + nearest.Radius);

            if (miner.distance(nearest) < miner.Job.Range + nearest.Radius)
            {
                miner.Mine(nearest);
            }
        }

        public static void attack(Unit attacker, IEnumerable<Unit> targets)
        {
            if (attacker.Acted || attacker.Job.Damage == 0)
            {
                return;
            }
            var nearest = targets.Where(t => t.Energy > 0).MinByValue(t => t.distance(attacker));
            if (nearest == null)
            {
                return;
            }

            moveToward(attacker, nearest.X, nearest.Y, attacker.Job.Range + AI.GAME.ShipRadius);

            if (attacker.distance(nearest) < attacker.Job.Range + AI.GAME.ShipRadius)
            {
                attacker.Attack(nearest);
            }
        }

        public static double distance(double dx, double dy)
        {
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
