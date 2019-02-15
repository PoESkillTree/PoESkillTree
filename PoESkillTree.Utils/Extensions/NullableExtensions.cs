using System;
using System.Collections.Generic;

namespace PoESkillTree.Utils.Extensions
{
    public static class NullableExtensions
    {
        public static TOut? Select<TIn, TOut>(this TIn? nullable, Func<TIn, TOut> selector)
            where TIn: struct
            where TOut: struct
        {
            return nullable.HasValue ? selector(nullable.Value) : (TOut?) null;
        }

        public static T? AggregateOnValues<T>(this List<T?> values, Func<T, T, T> combiner, Func<T, T> selector = null)
            where T: struct
        {
            T? result = null;
            foreach (var nullableValue in values)
            {
                if (!(nullableValue is T value))
                    continue;

                if (selector != null)
                    value = selector(value);
                result = result.HasValue ? combiner(result.Value, value) : value;
            }
            return result;
        }
    }
}