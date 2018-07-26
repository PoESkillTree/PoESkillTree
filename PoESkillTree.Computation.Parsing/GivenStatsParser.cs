using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Parsing
{
    public class GivenStatsParser
    {
        public static IReadOnlyList<Modifier> Parse(IParser parser, IEnumerable<IGivenStats> givenStats)
        {
            var modifierSource = new ModifierSource.Global(new ModifierSource.Local.Given());
            var givenParser = new GivenStatsParser(parser, modifierSource);
            foreach (var given in givenStats)
            {
                givenParser.Parse(given);
            }
            return givenParser._modifiers;
        }

        private readonly IParser _parser;
        private readonly ModifierSource _modifierSource;

        private readonly List<Modifier> _modifiers = new List<Modifier>();

        private GivenStatsParser(IParser parser, ModifierSource modifierSource)
            => (_parser, _modifierSource) = (parser, modifierSource);

        private void Parse(IGivenStats givenStats)
        {
            foreach (var entity in givenStats.AffectedEntities)
            {
                foreach (var statLine in givenStats.GivenStatLines)
                {
                    Parse(entity, statLine);
                }
                foreach (var modifier in givenStats.GivenModifiers)
                {
                    Parse(entity, modifier);
                }
            }
        }

        private void Parse(Entity entity, string statLine)
        {
            var (success, _, mods) = _parser.Parse(statLine, _modifierSource, entity);
            if (!success)
                throw new ParseException("Failed to parse given stat " + statLine);
            _modifiers.AddRange(mods);
        }

        private void Parse(Entity entity, IIntermediateModifier modifier)
        {
            var mods = modifier.Build(_modifierSource, entity);
            _modifiers.AddRange(mods);
        }
    }
}