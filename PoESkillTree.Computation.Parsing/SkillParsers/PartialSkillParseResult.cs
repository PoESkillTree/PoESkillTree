using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class PartialSkillParseResult
    {
        public PartialSkillParseResult(
            IEnumerable<Modifier> parsedModifiers, IEnumerable<UntranslatedStat> parsedStats)
            => (ParsedModifiers, ParsedStats) = (parsedModifiers, parsedStats);

        public void Deconstruct(
            out IEnumerable<Modifier> parsedModifiers, out IEnumerable<UntranslatedStat> parsedStats)
            => (parsedModifiers, parsedStats) = (ParsedModifiers, ParsedStats);

        public IEnumerable<Modifier> ParsedModifiers { get; }
        public IEnumerable<UntranslatedStat> ParsedStats { get; }
    }
}