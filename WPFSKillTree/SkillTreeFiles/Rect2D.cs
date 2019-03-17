using System;
using System.Windows;

namespace PoESkillTree.SkillTreeFiles
{
    /// <summary>
    ///     Rectangle
    /// </summary>
    [Serializable]
    public struct Rect2D
    {
        public static implicit operator Rect(Rect2D o)
        {
            return new Rect(o.TopLeft, o.BottomRight);
        }

        public static implicit operator Rect2D(Rect o)
        {
            return new Rect2D(o.TopLeft, o.BottomRight);
        }

        private readonly Vector2D _Min, _Max;
        public Vector2D Min, Max;

        public double Left
        {
            get { return Min.X; }
        }

        public double Right
        {
            get { return Max.X; }
        }

        public double Top
        {
            get { return Min.Y; }
        }

        public double Bottom
        {
            get { return Max.Y; }
        }

        public double Width
        {
            get { return Max.X - Min.X; }
        }

        public double Height
        {
            get { return Max.Y - Min.Y; }
        }

        public Vector2D Center
        {
            get { return (Min + Max)/2; }
        }

        public Vector2D TopLeft
        {
            get { return new Vector2D(Left, Top); }
        }

        public Vector2D TopRight
        {
            get { return new Vector2D(Right, Top); }
        }

        public Vector2D BottomLeft
        {
            get { return new Vector2D(Left, Bottom); }
        }

        public Vector2D BottomRight
        {
            get { return new Vector2D(Right, Bottom); }
        }

        public Vector2D Size
        {
            get { return new Vector2D(Width, Height); }
        }

        // public Vector2D RandomPoint( Random r ) { return new Vector2D( r.NextBetween( Left, Right ), r.NextBetween( Top, Bottom ) ); }

        public Rect2D(Vector2D v)
        {
            Min = Max = v;
            _Min = _Max = v;
        }

        public Rect2D(Vector2D min, Vector2D max)
        {
            Min = new Vector2D(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y));
            Max = new Vector2D(Math.Max(min.X, max.X), Math.Max(min.Y, max.Y));
            _Min = Min;
            _Max = Max;
        }

        public Rect2D(double left, double top, double width, double height)
            : this(new Vector2D(left, top), new Vector2D(left + width, top + height))
        {
        }

        public bool Contains(Vector2D v)
        {
            return v.X >= Min.X && v.Y >= Min.Y && v.Y <= Max.Y && v.X <= Max.X;
        }

        public bool Contains(Rect2D v)
        {
            return v.Min.X >= Min.X && v.Min.Y >= Min.Y && v.Max.Y <= Max.Y && v.Max.X <= Max.X;
        }

        public bool IsPoint
        {
            get { return Min == Max; }
        }

        public bool IntersectsWith(Rect2D r)
        {
            return !(r.Max.X <= Min.X || r.Max.Y <= Min.Y || Max.Y <= r.Min.Y || Max.X <= r.Min.X);
        }

        public static bool operator ==(Rect2D lhs, Rect2D rhs)
        {
            return lhs.Min == rhs.Min && lhs.Max == rhs.Max;
        }

        public static bool operator !=(Rect2D lhs, Rect2D rhs)
        {
            return lhs.Min != rhs.Min || lhs.Max != rhs.Max;
        }

        public static Rect2D operator *(Rect2D lhs, double d)
        {
            return new Rect2D(lhs.Center - lhs.Size/2*d, lhs.Center + lhs.Size/2*d);
        }

        public override int GetHashCode()
        {
            return _Min.GetHashCode() ^ _Max.GetHashCode();
        }

        public Rect2D Assimilate(Vector2D v)
        {
            /*if ( v.X < Min.X ) Min.X = v.X;
            if ( v.Y < Min.Y ) Min.Y = v.Y;
            if ( v.X > Max.X ) Max.X = v.X;
            if ( v.Y > Max.Y ) Max.Y = v.Y;*/
            return new Rect2D(
                new Vector2D(
                    Math.Min(Left, v.X),
                    Math.Min(Top, v.Y)),
                new Vector2D(
                    Math.Max(Right, v.X),
                    Math.Max(Bottom, v.Y))
                );
        }

        public Rect2D Widen(double delta)
        {
            return new Rect2D(Left - delta, Top - delta, Width + delta*2, Height + delta*2);
        }

        public override bool Equals(object obj)
        {
            return this == (Rect2D) obj;
        }
    }
}
