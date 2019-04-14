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
    public static class Extensions
    {
        public static double distance(this Unit unit, Body body)
        {
            return Solver.distance(unit.X - body.X, unit.Y - body.Y);
        }
        public static double distance(this Unit unit, Unit unit2)
        {
            return Solver.distance(unit.X - unit2.X, unit.Y - unit2.Y);
        }
        public static double distance(this Body body, Body body2)
        {
            return Solver.distance(body.X - body2.X, body.Y - body2.Y);
        }
        public static double distance(this Body body, Unit unit)
        {
            return Solver.distance(body.X - unit.X, body.Y - unit.Y);
        }

        public static int remainingCapacity(this Unit unit)
        {
            return unit.Job.CarryLimit - unit.Mythicite - unit.Legendarium - unit.Rarium - unit.Genarium;
        }

        public static T MinByValue<T, K>(this IEnumerable<T> source, Func<T, K> selector)
        {
            var comparer = Comparer<K>.Default;

            var enumerator = source.GetEnumerator();
            enumerator.MoveNext();

            var min = enumerator.Current;
            var minV = selector(min);

            while (enumerator.MoveNext())
            {
                var s = enumerator.Current;
                var v = selector(s);
                if (comparer.Compare(v, minV) < 0)
                {
                    min = s;
                    minV = v;
                }
            }
            return min;
        }

        public static int dashCost(this Unit unit, double x, double y)
        {
            return unit.dashCost(Solver.distance(x - unit.X, y - unit.Y));
        }

        public static int dashCost(this Unit unit, double distance)
        {
            return (int)Math.Ceiling((distance + Solver.ERROR) / AI.GAME.DashDistance) * AI.GAME.DashCost;
        }

        public static bool canMoveAndDash(this Unit unit, double x, double y)
        {
            var distance = Solver.distance(x - unit.X, y - unit.Y);
            return unit.Energy > unit.dashCost(distance - unit.Moves - Solver.ERROR2);
        }

        public static bool canDash(this Unit unit, double x, double y)
        {
            return unit.Energy > unit.dashCost(x, y);
        }

        public static bool canDash(this Unit unit, double distance)
        {
            return unit.Energy > unit.dashCost(distance);
        }

        public static double getValue(this string material)
        {
            switch (material)
            {
                case "mythicite":
                    return 1000;
                case "legendarium":
                    return AI.GAME.LegendariumValue;
                case "rarium":
                    return AI.GAME.RariumValue;
                case "genarium":
                    return AI.GAME.GenariumValue;
            }
            return 0;
        }

        public static bool canMine(this Unit unit, Body body)
        {
            return unit.Job == AI.MINER && !unit.IsBusy && body.MaterialType != "none" && body.Amount > 0 && body.Owner != unit.Owner.Opponent && unit.remainingCapacity() > 0;
        }

        public static bool inMiningRangeThisTurn(this Unit unit, Body body)
        {
            return Solver.inRangeE2(unit.distance(body), unit.Job.Range + body.Radius + unit.Moves);
        }

        public static bool canTransferThisTurn(this Unit transport, Unit miner)
        {
            return transport.remainingCapacity() > 0 && Solver.inRangeE2(transport.distance(miner), transport.Job.Range + transport.Moves);
        }

        public static Vector next(this Body body, int turns)
        {
            return new Vector(body.X, body.Y).rotate(new Vector(AI.SUN.X, AI.SUN.Y), ((2 * Math.PI) / AI.GAME.TurnsToOrbit) * -turns);
        }
    }
}