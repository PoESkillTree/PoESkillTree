using System;

namespace POESKillTree.Utils.Extensions
{
    public static class NumberExtensions
    {
        public static bool AlmostEquals(this float x, float y)
        {
            // float has 6 significant digits. 4 leaves a bit of room for accumulated errors.
            var epsilon = Math.Max(Math.Abs(x), Math.Abs(y)) * 1e-5F;
            return AlmostEquals(x, y, epsilon);
        }
        public static bool AlmostEquals(this float x, float y, float epsilon)
        {
            return Math.Abs(x - y) <= epsilon;
        }

        public static bool AlmostEquals(this double x, double y)
        {
            // double has 15 significant digits. 13 leaves a bit of room for accumulated errors.
            var epsilon = Math.Max(Math.Abs(x), Math.Abs(y)) * 1e-14;
            return AlmostEquals(x, y, epsilon);
        }
        public static bool AlmostEquals(this double x, double y, double epsilon)
        {
            return Math.Abs(x - y) <= epsilon;
        }
    }
}