using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace POESKillTree.Utils.Converter
{
    public class NumberLessStringComparer : IComparer<string>
    {
        private static readonly Regex Numberfilter = new Regex(@"[0-9\\.]+");

        public int Compare(string x, string y)
        {
            return String.CompareOrdinal(Numberfilter.Replace(x, ""), Numberfilter.Replace(y, ""));
        }
    }
}