using System.Collections.Generic;
using System.Diagnostics;
using MoreLinq;

namespace PoESkillTree.Utils
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

        public static void OpenInBrowser(string url)
        {
            Process.Start(new ProcessStartInfo(url) {UseShellExecute = true});
        }
    }
}