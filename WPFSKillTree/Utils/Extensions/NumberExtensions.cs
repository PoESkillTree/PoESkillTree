using System;

namespace POESKillTree.Utils.Extensions
{
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
    public static class NumberExtensions
    {
        public static bool AlmostEquals(this float x, float y, double delta)
        {
            return Math.Abs(x - y) < delta;
        }
        public static bool AlmostEquals(this SmallDec x, SmallDec y, double delta)
        {
            return SmallDec.Abs(x - y) < delta;
        }
        public static bool AlmostEquals(this double x, double y, double delta)
        {
            return Math.Abs(x - y) < delta;
        }
    }
}