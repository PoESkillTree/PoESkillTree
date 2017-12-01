using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Parsing
{
    public class MatcherDataParseResult
    {
        public MatcherDataParseResult(IModifierBuilder modifierBuilder, IReadOnlyDictionary<string, string> groups)
        {
            ModifierBuilder = modifierBuilder;
            Groups = groups;
        }

        public IModifierBuilder ModifierBuilder { get; }

        public IReadOnlyDictionary<string, string> Groups { get; }
    }
}