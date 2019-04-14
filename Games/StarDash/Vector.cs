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
    public struct Vector
    {
        public double x;
        public double y;

        public Vector(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            Vector o = (Vector)obj;
            return o.x == x && o.y == y;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("|{0},{1}|", x, y);
        }

        public double length()
        {
            return Solver.distance(x, y);
        }

        public double lengthSquared()
        {
            return Solver.distanceSquared(x, y);
        }

        public double dot(Vector other)
        {
            return x * other.x + y * other.y;
        }

        public Vector unit()
        {
            var l = length();
            return new Vector(x / l, y / l);
        }

        public Vector scale(double scalar)
        {
            return new Vector(x * scalar, y * scalar);
        }

        public double scalarProjection(Vector other)
        {
            return dot(other.unit());
        }

        public Vector project(Vector other)
        {
            var unitOther = other.unit();
            var scalar = dot(unitOther);
            return unitOther.scale(scalar);
        }

        public Vector rotate(Vector origin, double rads)
        {
            var cx = x - origin.x;
            var cy = y - origin.y;
            var s = Math.Sin(rads);
            var c = Math.Cos(rads);
            cx = cx * c - cy * s;
            cy = cx * s + cy * c;
            return new Vector(cx + origin.x, cy + origin.y);
        }
    }
}