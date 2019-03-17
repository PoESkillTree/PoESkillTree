using System;
using System.Text.RegularExpressions;

namespace PoESkillTree.Utils
{
    public static class RegexTools
    {
        public static bool IsValidRegex(string pattern)
        {
            if (pattern == null) return false;

            try
            {
                Regex.Match("", pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }
    }
}
