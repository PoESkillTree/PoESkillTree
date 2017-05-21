using System;
using System.Collections.Generic;
using MoreLinq;

namespace POESKillTree.Utils
{
    public static class Util
    {
        /// <summary>
        /// Returns a name based on <paramref name="name"/> that is not contained in
        /// <paramref name="invalidNames"/>.
        /// </summary>
        public static string FindDistinctName(string name, IEnumerable<string> invalidNames)
        {
            var invalidSet = invalidNames.ToHashSet();
            if (!invalidSet.Contains(name))
                return name;
            var i = 1;
            while (invalidSet.Contains(name + $" ({i})"))
            {
                i++;
            }
            return name + $" ({i})";
        }

        public static T[] GetEnumValues<T>() where T: struct
        {
            return (T[]) Enum.GetValues(typeof(T));
        }

        public static void TriggerPackUriSchemeInitialization()
        {
            if (!UriParser.IsKnownScheme("pack"))
            {
                // Necessary for unit tests. Accessing PackUriHelper triggers static initialization.
                // Without it, creating resource URIs from unit tests would throw UriFormatExceptions.
                var _ = System.IO.Packaging.PackUriHelper.UriSchemePack;
            }
        }
    }
}