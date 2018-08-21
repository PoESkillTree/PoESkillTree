using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Parsing
{
    public class GivenStatsParser
    {
        public static IReadOnlyList<Modifier> Parse(ICoreParser coreParser, IEnumerable<IGivenStats> givenStats)
        {
            var modifierSource = new ModifierSource.Global(new ModifierSource.Local.Given());
            var givenParser = new GivenStatsParser(coreParser, modifierSource);
            foreach (var given in givenStats)
            {
                givenParser.Parse(given);
            }
            return givenParser._modifiers;
        }

        private readonly ICoreParser _coreParser;
        private readonly ModifierSource _modifierSource;

        private readonly List<Modifier> _modifiers = new List<Modifier>();

        private GivenStatsParser(ICoreParser parser, ModifierSource modifierSource)
            => (_coreParser, _modifierSource) = (parser, modifierSource);

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
            var (success, _, _, mods) = _coreParser.Parse(statLine, _modifierSource, entity);
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