using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing
{
    public static class StringNormalizer
    {
        // This is infinitely faster than Regex.Replace
        public static string MergeWhiteSpace(string s)
        {
            var result = new List<char>(s.Length);
            var lastWasWhiteSpace = true;
            foreach (var c in s)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!lastWasWhiteSpace)
                    {
                        result.Add(' ');
                    }
                    lastWasWhiteSpace = true;
                }
                else
                {
                    result.Add(c);
                    lastWasWhiteSpace = false;
                }
            }
            return new string(result.ToArray());
        }
    }
}