using System;

namespace PoESkillTree.Common.Utils
{
    public static class Funcs
    {
        public static T Identity<T>(T t) => t;

        public static Func<T1, T3> AndThen<T1, T2, T3>(this Func<T1, T2> @this, Func<T2, T3> next) =>
            t1 => next(@this(t1));

        public static Func<T1, T2, T4> AndThen<T1, T2, T3, T4>(this Func<T1, T2, T3> @this, Func<T3, T4> next) =>
            (t1, t2) => next(@this(t1, t2));
    }
}