using System.Diagnostics;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Parsing.Data
{
    [DebuggerDisplay("{" + nameof(Regex) + "}")]
    public class MatcherData
    {
        public string Regex { get; }

        public IModifierBuilder ModifierBuilder { get; }

        public string MatchSubstitution { get; }

        public MatcherData(string regex, IModifierBuilder modifierBuilder, string matchSubstitution = "")
        {
            Regex = regex;
            ModifierBuilder = modifierBuilder;
            MatchSubstitution = matchSubstitution;
        }
    }
}