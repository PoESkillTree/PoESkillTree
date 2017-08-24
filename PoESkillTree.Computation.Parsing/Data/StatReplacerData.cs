using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing.Data
{
    public class StatReplacerData
    {
        public string OriginalStatRegex { get; }

        public IReadOnlyList<string> Replacements { get; }

        public StatReplacerData(string originalStatRegex, IReadOnlyList<string> replacements)
        {
            OriginalStatRegex = originalStatRegex;
            Replacements = replacements;
        }
    }
}