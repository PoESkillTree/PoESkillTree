using PoESkillTree.Computation.Parsing.Builders;

namespace PoESkillTree.Computation.Parsing.Data
{
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