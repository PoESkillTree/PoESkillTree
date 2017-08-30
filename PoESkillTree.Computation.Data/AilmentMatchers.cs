using System.Collections.Generic;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data
{
    public class AilmentMatchers : ReferencedMatchersBase<IAilmentBuilder>
    {
        private IAilmentBuilders Ailment { get; }

        public AilmentMatchers(IAilmentBuilders ailmentBuilders)
        {
            Ailment = ailmentBuilders;
        }

        protected override IEnumerable<ReferencedMatcherData<IAilmentBuilder>> CreateCollection() =>
            new ReferencedMatcherCollection<IAilmentBuilder>
            {
                // chance to x/x duration
                { "ignite", Ailment.Ignite },
                { "shock", Ailment.Shock },
                { "chill", Ailment.Chill },
                { "freeze", Ailment.Freeze },
                { "bleed", Ailment.Bleed },
                { "cause bleeding", Ailment.Bleed },
                { "poison", Ailment.Poison },
                // being/while/against x
                { "ignited", Ailment.Ignite },
                { "shocked", Ailment.Shock },
                { "chilled", Ailment.Chill },
                { "frozen", Ailment.Freeze },
                { "bleeding", Ailment.Bleed },
                { "poisoned", Ailment.Poison },
            };
    }
}