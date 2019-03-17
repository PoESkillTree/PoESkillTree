using System;

namespace PoESkillTree.SkillTreeFiles
{
    [Serializable]
    public struct Point2D
    {
        public readonly int X;
        public readonly int Y;

        public Point2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Point2D(Point2D other)
        {
            X = other.X;
            Y = other.Y;
        }

        public float Length
        {
            get { return (float) Math.Sqrt(X*X + Y*Y); }
        }

        public float LengthSqr
        {
            get { return X*X + Y*Y; }
        }

        public Point2D Discretizise(int size)
        {
            int newX = X;
            int newY = Y;
            if (newX%size != 0)
                if (newX > 0) newX -= newX%size;
                else newX -= newX%size + size;
            if (newY%size != 0)
                if (newY > 0) newY -= newY%size;
                else newY -= newY%size + size;
            return new Point2D(newX, newY);
        }

        public override bool Equals(object obj)
        {
            return this == (Point2D) obj;
        }

        public override int GetHashCode()
        {
            return (X << 16) + Y;
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }

        public static Point2D operator +(Point2D lhs, Point2D rhs)
        {
            return new Point2D(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static Point2D operator -(Point2D lhs, Point2D rhs)
        {
            return new Point2D(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static Point2D operator *(Point2D lhs, int f)
        {
            return new Point2D(lhs.X*f, lhs.Y*f);
        }

        public static Point2D operator /(Point2D lhs, int f)
        {
            return new Point2D(lhs.X/f, lhs.Y/f);
        }

        public static bool operator ==(Point2D lhs, Point2D rhs)
        {
            /*if ( ( object )lhs == null && ( object )rhs == null ) return true;
            if ( ( object )lhs == null || ( object )rhs == null ) return false;*/
            return lhs.X == rhs.X && lhs.Y == rhs.Y;
        }

        public static bool operator !=(Point2D lhs, Point2D rhs)
        {
            /*if ( ( object )lhs == null && ( object )rhs == null ) return false;
            if ( ( object )lhs == null || ( object )rhs == null ) return true;*/
            return lhs.X != rhs.X || lhs.Y != rhs.Y;
        }

        public static implicit operator Vector2D(Point2D p)
        {
            return new Vector2D(p.X, p.Y);
        }
    }
}