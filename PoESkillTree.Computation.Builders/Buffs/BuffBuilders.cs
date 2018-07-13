using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Buffs
{
    public class BuffBuilders : IBuffBuilders
    {
        private readonly IStatFactory _statFactory;
        private readonly IReadOnlyList<BuffBuilderWithKeywords> _allBuffs;

        public BuffBuilders(
            IStatFactory statFactory, IEnumerable<(string identifier, IReadOnlyList<Keyword> keywords)> skillBuffs)
        {
            _statFactory = statFactory;
            Fortify = new BuffBuilder(statFactory, "Fortify");
            Maim = new BuffBuilder(statFactory, "Maim");
            Intimidate = new BuffBuilder(statFactory, "Intimidate");
            Taunt = new BuffBuilder(statFactory, "Taunt");
            Blind = new BuffBuilder(statFactory, "Blind");
            Onslaught = new BuffBuilder(statFactory, "Onslaught");
            UnholyMight = new BuffBuilder(statFactory, "UnholyMight");
            Phasing = new BuffBuilder(statFactory, "Phasing");
            Conflux = new ConfluxBuffBuilders(statFactory);
            CurseLimit = StatBuilderUtils.FromIdentity(statFactory, "CurseLimit", typeof(int));

            var allBuffs = new List<BuffBuilderWithKeywords>
            {
                new BuffBuilderWithKeywords(Fortify),
                new BuffBuilderWithKeywords(Maim),
                new BuffBuilderWithKeywords(Intimidate),
                new BuffBuilderWithKeywords(Taunt),
                new BuffBuilderWithKeywords(Blind),
                new BuffBuilderWithKeywords(Onslaught),
                new BuffBuilderWithKeywords(UnholyMight),
                new BuffBuilderWithKeywords(Phasing),
                new BuffBuilderWithKeywords(Conflux.Chilling),
                new BuffBuilderWithKeywords(Conflux.Elemental),
                new BuffBuilderWithKeywords(Conflux.Igniting),
                new BuffBuilderWithKeywords(Conflux.Shocking),
                // Generic buff effect increase
                new BuffBuilderWithKeywords(new BuffBuilder(statFactory, "Buff")),
                // Aura effect increase
                new BuffBuilderWithKeywords(new BuffBuilder(statFactory, "Aura"), Keyword.Aura),
            };
            var skillBuffBuilders = skillBuffs.Select(t =>
                new BuffBuilderWithKeywords(new BuffBuilder(statFactory, t.identifier), t.keywords));
            allBuffs.AddRange(skillBuffBuilders);
            _allBuffs = allBuffs;
        }

        public IBuffBuilder Fortify { get; }
        public IBuffBuilder Maim { get; }
        public IBuffBuilder Intimidate { get; }
        public IBuffBuilder Taunt { get; }
        public IBuffBuilder Blind { get; }
        public IBuffBuilder Onslaught { get; }
        public IBuffBuilder UnholyMight { get; }
        public IBuffBuilder Phasing { get; }
        public IConfluxBuffBuilders Conflux { get; }

        public IStatBuilder Temporary(IStatBuilder gainedStat)
        {
            return gainedStat.WithCondition(new ValueConditionBuilder(BuildCondition));

            IValue BuildCondition(BuildParameters parameters)
            {
                var stat = _statFactory.FromIdentity($"Is {parameters.ModifierSource} active?",
                    parameters.ModifierSourceEntity, typeof(bool), ExplicitRegistrationTypes.UserSpecifiedValue());
                return new StatValue(stat);
            }
        }

        public IStatBuilder Temporary<T>(IBuffBuilder buff, T condition) where T : struct, Enum
        {
            return buff.On(new ModifierSourceEntityBuilder()).WithCondition(new ValueConditionBuilder(BuildCondition));

            IValue BuildCondition(BuildParameters parameters)
            {
                var stat = _statFactory.FromIdentity($"Current {parameters.ModifierSource} stage",
                    parameters.ModifierSourceEntity, typeof(T), ExplicitRegistrationTypes.UserSpecifiedValue());
                return new ConditionalValue(c => (int) c.GetValue(stat).Single() == Enums.ToInt32(condition),
                    $"{stat} == {condition}");
            }
        }

        public IStatBuilder Buff(IStatBuilder gainedStat, params IEntityBuilder[] affectedEntites)
        {
            var statBuilder = gainedStat.For(new CompositeEntityBuilder(affectedEntites));
            return MultiplyValueByEffectModifier(statBuilder, "Buff");
        }

        public IStatBuilder Aura(IStatBuilder gainedStat, params IEntityBuilder[] affectedEntites)
        {
            var statBuilder = gainedStat.For(new CompositeEntityBuilder(affectedEntites));
            return MultiplyValueByEffectModifier(statBuilder, "Aura");
        }

        private IStatBuilder MultiplyValueByEffectModifier(IStatBuilder gainedStat, string buffIdentity)
        {
            var coreStatBuilder = new StatBuilderWithValueConverter(new StatBuilderAdapter(gainedStat),
                e => new StatValue(_statFactory.BuffEffect(e, buffIdentity)),
                (l, r) => l.Multiply(r));
            return new StatBuilder(_statFactory, coreStatBuilder);
        }

        public IBuffBuilderCollection Buffs(IEntityBuilder source = null, params IEntityBuilder[] targets)
        {
            IEntityBuilder allEntityBuilder = new EntityBuilder(Enums.GetValues<Entity>().ToArray());
            var sourceEntity = source ?? allEntityBuilder;
            var targetEntity = targets.Any() ? new CompositeEntityBuilder(targets) : allEntityBuilder;
            return new BuffBuilderCollection(_statFactory, _allBuffs, sourceEntity, targetEntity);
        }

        public IStatBuilder CurseLimit { get; }

        private class ConfluxBuffBuilders : IConfluxBuffBuilders
        {
            public ConfluxBuffBuilders(IStatFactory statFactory)
            {
                Igniting = new BuffBuilder(statFactory, "IgnitingConflux");
                Shocking = new BuffBuilder(statFactory, "ShockingConflux");
                Chilling = new BuffBuilder(statFactory, "ChillingConflux");
                Elemental = new BuffBuilder(statFactory, "ElementalConflux");
            }

            public IBuffBuilder Igniting { get; }
            public IBuffBuilder Shocking { get; }
            public IBuffBuilder Chilling { get; }
            public IBuffBuilder Elemental { get; }
        }
    }
}