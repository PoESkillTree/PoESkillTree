using System;
using System.Windows;

namespace POESKillTree.SkillTreeFiles
{
    public readonly struct Vector2D
    {
        public static implicit operator Point(Vector2D operand)
            => new Point(operand.X, operand.Y);

        public static implicit operator Vector2D(Point operand)
            => new Vector2D(operand.X, operand.Y);

        public readonly double X, Y;

        public Vector2D(double x, double y)
            => (X, Y) = (x, y);

        public override string ToString()
            => "(" + X + ", " + Y + ")";

        public double Length => Math.Sqrt(X*X + Y*Y);

        public override int GetHashCode()
            => (X.GetHashCode() << 16) ^ Y.GetHashCode();

        public static Vector2D operator +(Vector2D lhs, Vector2D rhs)
            => new Vector2D(lhs.X + rhs.X, lhs.Y + rhs.Y);

        public static Vector2D operator /(Vector2D lhs, Vector2D rhs)
            => new Vector2D(lhs.X/rhs.X, lhs.Y/rhs.Y);

        public static Vector2D operator *(Vector2D lhs, Vector2D rhs)
            => new Vector2D(lhs.X*rhs.X, lhs.Y*rhs.Y);

        public static Vector2D operator -(Vector2D lhs, Vector2D rhs)
            => new Vector2D(lhs.X - rhs.X, lhs.Y - rhs.Y);

        public static Vector2D operator *(Vector2D lhs, double f)
            => new Vector2D(lhs.X*f, lhs.Y*f);

        public static Vector2D operator /(Vector2D lhs, double f)
            => new Vector2D(lhs.X/f, lhs.Y/f);

        public override bool Equals(object obj)
            => obj is Vector2D other && Equals(other);

        private bool Equals(Vector2D other)
            => X.Equals(other.X) && Y.Equals(other.Y);
    }
}