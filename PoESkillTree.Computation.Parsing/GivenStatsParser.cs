using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Parsing
{
    public class GivenStatsParser
    {
        public static IReadOnlyList<Modifier> Parse(ICoreParser coreParser, IEnumerable<IGivenStats> givenStats)
        {
            var givenParser = new GivenStatsParser(coreParser);
            return givenStats.SelectMany(givenParser.ParseToModifiers).ToList();
        }

        public static IReadOnlyList<Modifier> Parse(ICoreParser coreParser, IGivenStats givenStats)
            => new GivenStatsParser(coreParser).ParseToModifiers(givenStats);

        private static readonly ModifierSource ModifierSource =
            new ModifierSource.Global(new ModifierSource.Local.Given());

        private readonly ICoreParser _coreParser;

        private GivenStatsParser(ICoreParser coreParser)
            => _coreParser = coreParser;

        private IReadOnlyList<Modifier> ParseToModifiers(IGivenStats givenStats)
        {
            var results = Parse(givenStats);
            var result = ParseResult.Aggregate(results);
            if (!result.SuccessfullyParsed)
                throw new ParseException("Failed to parse given modifier lines " +
                                         result.FailedLines.ToDelimitedString("\n"));
            return result.Modifiers;
        }

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