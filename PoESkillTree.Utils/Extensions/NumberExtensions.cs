using System;

namespace PoESkillTree.Utils.Extensions
{
    public static class NumberExtensions
    {
        public static bool AlmostEquals(this float x, float y, double delta)
            => x == y || Math.Abs(x - y) < delta;

        public static bool AlmostEquals(this double x, double y, double delta)
            => x == y || Math.Abs(x - y) < delta;
    }
}