using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.GameModel;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing
{
    public class GivenStatsParser
    {
        public static IReadOnlyList<Modifier> Parse(ICoreParser coreParser, IEnumerable<IGivenStats> givenStats)
            => ParseDeferred(coreParser, givenStats).Flatten().ToList();

        public static IEnumerable<IReadOnlyList<Modifier>> ParseDeferred(
            ICoreParser coreParser, IEnumerable<IGivenStats> givenStats)
        {
            var givenParser = new GivenStatsParser(coreParser);
            foreach (var parseResult in givenStats.SelectMany(g => givenParser.Parse(g)))
            {
                if (!parseResult.SuccessfullyParsed)
                    throw new ParseException("Failed to parse given modifier lines " +
                                             parseResult.FailedLines.ToDelimitedString("\n"));
                yield return parseResult.Modifiers;
            }
        }

        private static readonly ModifierSource ModifierSource =
            new ModifierSource.Global(new ModifierSource.Local.Given());

        private readonly ICoreParser _coreParser;

        private GivenStatsParser(ICoreParser coreParser)
            => _coreParser = coreParser;

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