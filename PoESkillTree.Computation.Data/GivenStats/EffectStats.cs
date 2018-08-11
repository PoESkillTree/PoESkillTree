using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data.GivenStats
{
    /// <summary>
    /// <see cref="IGivenStats"/> implementation that provides the stats applied when effects are active.
    /// </summary>
    public class EffectStats : UsesStatBuilders, IGivenStats
    {
        private readonly IModifierBuilder _modifierBuilder;
        private readonly Lazy<IReadOnlyList<IIntermediateModifier>> _lazyGivenStats;

        public EffectStats(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
            _lazyGivenStats = new Lazy<IReadOnlyList<IIntermediateModifier>>(() => CreateCollection().ToList());
        }

        public IReadOnlyList<Entity> AffectedEntities { get; } = Enums.GetValues<Entity>().ToList();

        public IReadOnlyList<string> GivenStatLines { get; } = new string[0];

        public IReadOnlyList<IIntermediateModifier> GivenModifiers => _lazyGivenStats.Value;

        private EffectStatCollection CreateCollection() => new EffectStatCollection(_modifierBuilder, ValueFactory)
        {
            // ailments
            { Ailment.Freeze, PercentReduce, Stat.AnimationSpeed, 100 },
            // buffs
            { Buff.Fortify, PercentReduce, Damage.Taken.WithHits, 20 },
            { Buff.Maim, PercentReduce, Stat.MovementSpeed, 30 },
            { Buff.Intimidate, PercentIncrease, Damage.Taken, 10 },
            { Buff.Onslaught, PercentIncrease, Stat.CastRate, 20 },
            { Buff.Onslaught, PercentIncrease, Stat.MovementSpeed, 20 },
            {
                Buff.UnholyMight,
                BaseAdd, Physical.Damage.WithHitsAndAilments.GainAs(Chaos.Damage.WithHitsAndAilments), 30
            },
            { Buff.ArcaneSurge, PercentMore, Damage.WithSkills(DamageSource.Spell), 10 },
            { Buff.ArcaneSurge, PercentIncrease, Stat.CastRate.With(DamageSource.Spell), 10 },
            { Buff.ArcaneSurge, BaseAdd, Mana.Regen.Percent, 0.5 },
            { Buff.Conflux.Igniting, BaseSet, Ailment.Ignite.Source(AnyDamageType), 1 },
            { Buff.Conflux.Shocking, BaseSet, Ailment.Shock.Source(AnyDamageType), 1 },
            { Buff.Conflux.Chilling, BaseSet, Ailment.Chill.Source(AnyDamageType), 1 },
            { Buff.Conflux.Elemental, BaseSet, Ailment.Ignite.Source(AnyDamageType), 1 },
            { Buff.Conflux.Elemental, BaseSet, Ailment.Shock.Source(AnyDamageType), 1 },
            { Buff.Conflux.Elemental, BaseSet, Ailment.Chill.Source(AnyDamageType), 1 },
            // other effects
            { Ground.Consecrated, BaseAdd, Life.Regen, 6 },
        };
    }
}