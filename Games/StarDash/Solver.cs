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
            var dx = x - unit.X;
            var dy = y - unit.Y;
            var distance = Solver.distance(dx, dy) - range;
            var magnitude = Math.Min(unit.Moves, distance);
            unit.Move(unit.X + (dx / distance) * magnitude, unit.Y + (dy / distance) * magnitude);
        }

        public static double distance(double dx, double dy)
        {
            return Math.Sqrt(dx * dx + dy + dy);
        }
    }
}