using System;
using System.Windows;

namespace POESKillTree
{
    [Serializable]
    public struct Vector2D
    {
        public static double DistanceLinePoint( Vector2D start, Vector2D end, Vector2D v )
        {
            Vector2D n;
            return DistanceLinePoint( start, end, v, out n );
        }
        public static implicit operator Point(Vector2D operand)
        {
            return new Point(operand.X,operand.Y);
        }
        public static implicit operator Vector2D(Point operand)
        {
            return new Vector2D(operand.X, operand.Y);
        }
        public static double DistanceLinePoint( Vector2D start, Vector2D end, Vector2D v, out Vector2D nearest )
        {
            Vector2D dir = v - start;
            Vector2D borderDir = ( end - start ).Normalized( );
            double dot = borderDir.Dot( dir );
            if ( dot < 0 )
            {
                nearest = start;
                return ( v - start ).Length;
            }
            if ( dot > ( end - start ).Length )
            {
                nearest = end;
                return ( v - end ).Length;
            }
            Vector2D tangent = borderDir * dot;
            nearest = start + tangent;
            Vector2D normal = dir - tangent;
            return normal.Length;
        }

        public static Vector2D? IntersectLineLine( Vector2D s1, Vector2D e1, Vector2D s2, Vector2D e2 )
        {
            Rect2D aabb1 = new Rect2D( s1, e1 );
            Rect2D aabb2 = new Rect2D( s2, e2 );
            if ( !aabb1.IntersectsWith( aabb2 ) ) return null;

            double x1 = s1.X;
            double x2 = e1.X;
            double x3 = s2.X;
            double x4 = e2.X;
            double y1 = s1.Y;
            double y2 = e1.Y;
            double y3 = s2.Y;
            double y4 = e2.Y;

            double denom = ( x1 - x2 ) * ( y3 - y4 ) - ( y1 - y2 ) * ( x3 - x4 );
            if ( Math.Abs( denom ) < 1e-4 ) return null;
            double px = ( ( x1 * y2 - y1 * x2 ) * ( x3 - x4 ) - ( x1 - x2 ) * ( x3 * y4 - y3 * x4 ) ) / denom;
            double py = ( ( x1 * y2 - y1 * x2 ) * ( y3 - y4 ) - ( y1 - y2 ) * ( x3 * y4 - y3 * x4 ) ) / denom;

            Vector2D p = new Vector2D( px, py );
            double l1 = ( p - s1 ).Length;
            double l2 = ( p - e1 ).Length;
            if ( l1 + l2 > ( s1 - e1 ).Length + 1e-3 ) return null;
            return p;
        }

        public double X, Y;

        public Vector2D( double x, double y )
        {
            X = x;
            Y = y;
        }

        public Vector2D( Point2D other )
        {
            X = other.X;
            Y = other.Y;
        }

        public override string ToString( )
        {
            return "(" + X + ", " + Y + ")";
        }

        //public Vector2D RandomRotate( double minAngle, double maxAngle, Random r )
        //{
        //    double angle = r.NextBetween( minAngle, maxAngle );
        //    return Rotate( angle );
        //}

        public static bool EpsilonEquals( Vector2D v1, Vector2D v2, double epsilon = 1e-5 )
        {
            return ( v1 - v2 ).Length < epsilon;
        }

        public string ToString( string format )
        {
            return "(" + X.ToString( format ) + ", " + Y.ToString( format ) + ")";
        }

        public Vector2D Rotate( double angle )
        {
            return new Vector2D(
                ( double )( Math.Cos( angle ) * X - Math.Sin( angle ) * Y ),
                ( double )( Math.Sin( angle ) * X + Math.Cos( angle ) * Y )
                );
        }

        public bool IsZero { get { return X == 0 && Y == 0; } }

        public double Length { get { return ( double )Math.Sqrt( X * X + Y * Y ); } }

        public double ManhattenLength { get { return Math.Abs( X ) + Math.Abs( Y ); } }

        public double LengthSqr { get { return X * X + Y * Y; } }

        public override int GetHashCode( )
        {
            return ( X.GetHashCode( ) << 16 ) ^ Y.GetHashCode( );
        }

        public static Vector2D operator +( Vector2D lhs, Vector2D rhs )
        {
            return new Vector2D( lhs.X + rhs.X, lhs.Y + rhs.Y );
        }

        public static Vector2D operator /(Vector2D lhs, Vector2D rhs)
        {
            return new Vector2D(lhs.X / rhs.X, lhs.Y / rhs.Y);
        }
        public static Vector2D operator *(Vector2D lhs, Vector2D rhs)
        {
            return new Vector2D(lhs.X * rhs.X, lhs.Y * rhs.Y);
        }
        public static Vector2D operator -( Vector2D lhs, Vector2D rhs )
        {
            return new Vector2D( lhs.X - rhs.X, lhs.Y - rhs.Y );
        }

        public static Vector2D operator *( Vector2D lhs, double f )
        {
            return new Vector2D( lhs.X * f, lhs.Y * f );
        }

        public static Vector2D operator /( Vector2D lhs, double f )
        {
            return new Vector2D( lhs.X / f, lhs.Y / f );
        }

        public static bool operator ==( Vector2D lhs, Vector2D rhs )
        {
            /*if ( ( object )lhs == null && ( object )rhs == null ) return true;
            if ( ( object )lhs == null || ( object )rhs == null ) return false;*/
            return lhs.X == rhs.X && lhs.Y == rhs.Y;
        }

        public static bool operator !=( Vector2D lhs, Vector2D rhs )
        {
            /*if ( ( object )lhs == null && ( object )rhs == null ) return false;
            if ( ( object )lhs == null || ( object )rhs == null ) return true;*/
            return lhs.X != rhs.X || lhs.Y != rhs.Y;
        }

        public override bool Equals( object obj )
        {
            return this == ( Vector2D )obj;
        }

        public Point2D ToContainingPoint( )
        {
            return new Point2D( ( int )Math.Floor( X ), ( int )Math.Floor( Y ) );
        }

        public Vector2D Normalized( )
        {
            if ( X == 0 && Y == 0 ) return new Vector2D( 0, 0 );
            return this / Length;
        }

        public double Dot( Vector2D p )
        {
            return p.X * X + p.Y * Y;
        }

        public bool IsFirstQuadrant { get { return X >= 0 && Y >= 0; } }

        public bool IsSecondQuadrant { get { return X <= 0 && Y >= 0; } }

        public bool IsThirdQuadrant { get { return X <= 0 && Y <= 0; } }

        public bool IsForthQuadrant { get { return X >= 0 && Y <= 0; } }
    }
}