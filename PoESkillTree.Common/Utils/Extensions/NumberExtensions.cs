using System;

namespace PoESkillTree.Common.Utils.Extensions
{
    public static class NumberExtensions
    {
        public static bool AlmostEquals(this float x, float y, double delta)
        {
            return Math.Abs(x - y) < delta;
        }

        public static bool AlmostEquals(this double x, double y, double delta)
        {
            return Math.Abs(x - y) < delta;
        }
    }
}