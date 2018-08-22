using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Parsing
{
    public class GivenStatsParser : IParser<IEnumerable<IGivenStats>>
    {
        public static IReadOnlyList<Modifier> Parse(ICoreParser coreParser, IEnumerable<IGivenStats> givenStats)
        {
            var givenParser = new GivenStatsParser(coreParser);
            var parseResult = givenParser.Parse(givenStats);
            if (!parseResult.SuccessfullyParsed)
                throw new ParseException("Failed to parse given modifier lines " +
                                         parseResult.FailedLines.ToDelimitedString("\n"));
            return parseResult.Modifiers;
        }

        private static readonly ModifierSource ModifierSource =
            new ModifierSource.Global(new ModifierSource.Local.Given());

        private readonly ICoreParser _coreParser;

        private GivenStatsParser(ICoreParser coreParser)
            => _coreParser = coreParser;

        public ParseResult Parse(IEnumerable<IGivenStats> givenStats)
            => ParseResult.Aggregate(givenStats.SelectMany(Parse));

        private IEnumerable<ParseResult> Parse(IGivenStats givenStats)
        {
            foreach (var entity in givenStats.AffectedEntities)
            {
                foreach (var statLine in givenStats.GivenStatLines)
                {
                    yield return Parse(entity, statLine);
                }
                foreach (var modifier in givenStats.GivenModifiers)
                {
                    yield return Parse(entity, modifier);
                }
            }
        }

        private ParseResult Parse(Entity entity, string statLine)
            => _coreParser.Parse(statLine, ModifierSource, entity);

        private static ParseResult Parse(Entity entity, IIntermediateModifier modifier)
        {
            var mods = modifier.Build(ModifierSource, entity);
            return ParseResult.Success(mods);
        }
    }
}