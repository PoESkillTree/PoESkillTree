using System;

namespace POESKillTree.Utils.Extensions
{
    public static class NumberExtensions
    {
        public static bool AlmostEquals(this float x, float y, float delta)
        {
            return Math.Abs(x - y) < delta;
        }

        public static bool AlmostEquals(this double x, double y, double delta)
        {
            return Math.Abs(x - y) < delta;
        }
    }
}