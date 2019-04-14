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

        public static bool canDash(this Unit unit, double x, double y)
        {
            return unit.Energy > unit.dashCost(x, y);
        }

        public static bool canDash(this Unit unit, double distance)
        {
            return unit.Energy > unit.dashCost(distance);
        }
    }
}