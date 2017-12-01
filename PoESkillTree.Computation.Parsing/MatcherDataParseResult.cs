using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Parsing
{
    public class MatcherDataParseResult
    {
        public MatcherDataParseResult(IModifierResult modifierResult, IReadOnlyDictionary<string, string> groups)
        {
            ModifierResult = modifierResult;
            Groups = groups;
        }

        public IModifierResult ModifierResult { get; }

        public IReadOnlyDictionary<string, string> Groups { get; }
    }
}