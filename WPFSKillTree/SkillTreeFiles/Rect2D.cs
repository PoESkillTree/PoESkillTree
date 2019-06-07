using System;
using System.Windows;

namespace PoESkillTree.SkillTreeFiles
{
    public readonly struct Rect2D
    {
        public static implicit operator Rect(Rect2D o)
            => new Rect(o.TopLeft, o.BottomRight);

        public static implicit operator Rect2D(Rect o)
            => new Rect2D(o.TopLeft, o.BottomRight);

        public double Left => TopLeft.X;

        public double Right => BottomRight.X;

        public double Top => TopLeft.Y;

        public double Bottom => BottomRight.Y;

        public double Width => BottomRight.X - TopLeft.X;

        public double Height => BottomRight.Y - TopLeft.Y;

        public Vector2D TopLeft { get; }

        public Vector2D BottomRight { get; }

        public Vector2D Size => new Vector2D(Width, Height);

        public Rect2D(Vector2D min, Vector2D max)
        {
            TopLeft = new Vector2D(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y));
            BottomRight = new Vector2D(Math.Max(min.X, max.X), Math.Max(min.Y, max.Y));
        }

        public Rect2D(double left, double top, double width, double height)
            : this(new Vector2D(left, top), new Vector2D(left + width, top + height))
        {
        }

        public static Rect2D operator *(Rect2D rect, double factor)
        {
            var center = (rect.TopLeft + rect.BottomRight) / 2;
            return new Rect2D(center - rect.Size / 2 * factor, center + rect.Size / 2 * factor);
        }

        public override int GetHashCode()
            => TopLeft.GetHashCode() ^ BottomRight.GetHashCode();

        public override bool Equals(object obj)
            => obj is Rect2D other && Equals(other);

        private bool Equals(Rect2D other)
            => TopLeft.Equals(other.TopLeft) && BottomRight.Equals(other.BottomRight);
    }
}
