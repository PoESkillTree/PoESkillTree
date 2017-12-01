using System.Diagnostics;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Parsing.Data
{
    [DebuggerDisplay("{" + nameof(Regex) + "}")]
    public class MatcherData
    {
        public string Regex { get; }

        public IModifierResult ModifierResult { get; }

        public string MatchSubstitution { get; }

        public MatcherData(string regex, IModifierResult modifierResult, string matchSubstitution = "")
        {
            Regex = regex;
            ModifierResult = modifierResult;
            MatchSubstitution = matchSubstitution;
        }
    }
}