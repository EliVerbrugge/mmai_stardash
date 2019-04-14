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

        public static void moveToward(Unit unit, double x, double y, double range = 0, bool doDash = false)
        {
            if (unit.Moves == 0)
            {
                return;
            }

            if (sunCollision(unit, x, y))
            {
                var availableRoutePoints = AI.ROUTE_POINTS.Where(v => !sunCollision(unit, v.x, v.y)).ToArray();
                if (availableRoutePoints.Length == 0)
                {
                    Console.WriteLine("No available route points {0}", new Vector(unit.X, unit.Y));
                    return;
                }

                var bestRoutePoint = availableRoutePoints.MinByValue(v => Solver.distanceSquared(x - v.x, y - v.y));

                moveToward(unit, bestRoutePoint.x, bestRoutePoint.y, 0, doDash);
                if (hasMovesE2(unit))
                {
                    moveToward(unit, x, y, range, doDash);
                }
                return;
            }

            var dx = x - unit.X;
            var dy = y - unit.Y;
            var distance = Solver.distance(dx, dy);
            if (inRangeE2(distance, range))
            {
                return;
            }

            var netDistance = distance - range + ERROR2;
            if (doDash && netDistance > unit.Moves + unit.Job.Moves && unit.canDash(netDistance))
            {
                unit.Dash(unit.X + (dx / distance) * netDistance, unit.Y + (dy / distance) * netDistance);
            } else
            {
                var magnitude = Math.Min(unit.Moves - ERROR, netDistance);
                unit.Move(unit.X + (dx / distance) * magnitude, unit.Y + (dy / distance) * magnitude);
            }
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

            moveToward(miner, nearest.X, nearest.Y, miner.Job.Range + nearest.Radius, true);

            if (miner.distance(nearest) < miner.Job.Range + nearest.Radius && AI.GAME.CurrentTurn >= AI.GAME.OrbitsProtected)
            {
                miner.Mine(nearest);
            }
        }

        public static void transport(Unit transport, IEnumerable<Unit> units, string[] materials)
        {
            while (true)
            {
                if (transport.Acted || transport.remainingCapacity() == 0)
                {
                    return;
                }

                var miners = units.Where(u => u.Job.Title == "miner" && u.remainingCapacity() < u.Job.CarryLimit);
                var noCarries = !miners.Any();
                if (noCarries)
                {
                    miners = units.Where(u => u.Job.Title == "miner");
                }
                if (!miners.Any())
                {
                    return;
                }
                var nearest = miners.MinByValue(b => b.distance(transport));
                if (!hasMovesE2(transport) && !inRangeE1(transport.distance(nearest), transport.Job.Range))
                {
                    return;
                }

                moveToward(transport, nearest.X, nearest.Y, transport.Job.Range);

                if (noCarries)
                {
                    return;
                }

                if (transport.distance(nearest) < transport.Job.Range + .01)
                {
                    foreach (var material in materials)
                    {
                        transfer(transport, nearest, material);
                    }
                }
            }
        }

        public static void transfer(Unit transport, Unit other, string material)
        {
            if (transport.remainingCapacity() == 0)
            {
                return;
            }

            var hasMineral = false;
            switch (material)
            {
                case "mythicite":
                    hasMineral = other.Mythicite > 0;
                    break;
                case "legendarium":
                    hasMineral = other.Legendarium > 0;
                    break;
                case "rarium":
                    hasMineral = other.Rarium > 0;
                    break;
                case "genarium":
                    hasMineral = other.Genarium > 0;
                    break;
            }

            if (hasMineral)
            {
                transport.Transfer(other, -1, material);
            }
        }

        public static void attack(Unit attacker, IEnumerable<Unit> targets, bool missile=false)
        {
            if (attacker.Acted || attacker.Job.Damage == 0)
            {
                return;
            }
            var alive = targets.Where(t => t.Energy > 0);
            if (!alive.Any())
            {
                return;
            }

            var nearest = alive.MinByValue(t => t.distance(attacker));
            if (nearest == null)
            {
                return;
            }

            moveToward(attacker, nearest.X, nearest.Y, attacker.Job.Range + AI.GAME.ShipRadius);

            if (!missile && inRangeE1(attacker.distance(nearest), attacker.Job.Range + AI.GAME.ShipRadius))
            {
                attacker.Attack(nearest);
            }
            else if(missile && inRangeE1(attacker.distance(nearest), 300))
            {
                attacker.Attack(nearest);
            }
        }

        public static double distance(double dx, double dy)
        {
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double distanceSquared(double dx, double dy)
        {
            return dx * dx + dy * dy;
        }

        public static bool collision(double x1, double y1, double x2, double y2, double cx, double cy, double radius)
        {
            var v12 = new Vector(x2 - x1, y2 - y1);
            var v1C = new Vector(cx - x1, cy - y1);

            var radiusSquare = radius * radius;
            if (Solver.distanceSquared(cx - x1, cy - y1) <= radiusSquare)
            {
                return true;
            }
            if (Solver.distanceSquared(cx - x2, cy - y2) <= radiusSquare)
            {
                return true;
            }

            var unit12 = v12.unit();
            var scalarProjection = v1C.dot(unit12);

            if (scalarProjection <= 0 || scalarProjection >= v12.length())
            {
                return false;
            }

            var projected = unit12.scale(scalarProjection);
            return inRangeE1(Solver.distanceSquared(v1C.x - projected.x, v1C.y - projected.y), radiusSquare);
        }

        public static bool sunCollision(Unit unit, double x2, double y2)
        {
            return collision(unit.X, unit.Y, x2, y2, AI.SUN.X, AI.SUN.Y, AI.SUN.Radius + AI.GAME.ShipRadius + .01);
        }

        public static double ERROR = 0.01;
        public static double ERROR2 = 0.02;

        public static bool inRangeE1(double distance, double range)
        {
            return distance < range - ERROR;
        }

        public static bool inRangeE2(double distance, double range)
        {
            return distance < range - ERROR;
        }

        public static bool hasMovesE2(Unit unit)
        {
            return unit.Moves > ERROR2;
        }
    }
}
