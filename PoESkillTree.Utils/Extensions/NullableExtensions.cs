using System;
using System.Collections.Generic;
using System.Linq;

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


        public static IEnumerable<T?> SelectOnValues<T>(this IEnumerable<T?> values, Func<T, T> selector)
            where T: struct
        {
            return values.Select(v => Select(v, selector));
        }

        public static T? AggregateOnValues<T>(this IEnumerable<T?> values, Func<T, T, T> operation)
            where T: struct
        {
            T? result = null;
            foreach (var value in values.OfType<T>())
            {
                result = result.HasValue ? operation(result.Value, value) : value;
            }
            return result;
        }
    }
}