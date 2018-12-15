using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Builders.Buffs
{
    public class BuffBuilders : IBuffBuilders
    {
        private readonly IStatFactory _statFactory;
        private readonly IReadOnlyList<BuffBuilderWithKeywords> _allBuffs;

        public BuffBuilders(IStatFactory statFactory, SkillDefinitions skills)
        {
            _statFactory = statFactory;
            Fortify = Create("Fortify");
            Maim = Create("Maim");
            Intimidate = Create("Intimidate");
            Taunt = Create("Taunt");
            Blind = Create("Blind");
            Onslaught = Create("Onslaught");
            UnholyMight = Create("UnholyMight");
            Phasing = Create("Phasing");
            ArcaneSurge = Create("ArcaneSurge");
            Tailwind = Create("Tailwind");
            CoveredInAsh = Create("CoveredInAsh");
            Innervation = Create("Innervation");
            Impale = Create("Impale");
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
                new BuffBuilderWithKeywords(ArcaneSurge),
                new BuffBuilderWithKeywords(Tailwind, Keyword.Aura),
                new BuffBuilderWithKeywords(CoveredInAsh),
                new BuffBuilderWithKeywords(Innervation),
                new BuffBuilderWithKeywords(Impale),
                new BuffBuilderWithKeywords(Conflux.Chilling),
                new BuffBuilderWithKeywords(Conflux.Elemental),
                new BuffBuilderWithKeywords(Conflux.Igniting),
                new BuffBuilderWithKeywords(Conflux.Shocking),
                // Generic buff effect increase (used for Buff())
                new BuffBuilderWithKeywords(Create("Buff")),
                // Aura effect increase (used for Aura())
                new BuffBuilderWithKeywords(Create("Aura"), Keyword.Aura),
            };
            var skillBuffBuilders = skills.Skills
                .Where(s => !s.IsSupport && s.ActiveSkill.ProvidesBuff)
                .Select(s => new BuffBuilderWithKeywords(Create(s.Id), s.ActiveSkill.Keywords));
            allBuffs.AddRange(skillBuffBuilders);
            _allBuffs = allBuffs;
        }

        private BuffBuilder Create(string buffIdentity) =>
            new BuffBuilder(_statFactory, CoreBuilder.Create(buffIdentity));

        public IBuffBuilder Fortify { get; }
        public IBuffBuilder Maim { get; }
        public IBuffBuilder Intimidate { get; }
        public IBuffBuilder Taunt { get; }
        public IBuffBuilder Blind { get; }
        public IBuffBuilder Onslaught { get; }
        public IBuffBuilder UnholyMight { get; }
        public IBuffBuilder Phasing { get; }
        public IBuffBuilder ArcaneSurge { get; }
        public IBuffBuilder Tailwind { get; }
        public IBuffBuilder CoveredInAsh { get; }
        public IBuffBuilder Innervation { get; }
        public IBuffBuilder Impale { get; }
        public IConfluxBuffBuilders Conflux { get; }

        public IStatBuilder Temporary(IStatBuilder gainedStat)
        {
            var statBuilder = gainedStat.WithCondition(new ValueConditionBuilder(BuildCondition));
            return MultiplyValueByEffectModifier(statBuilder, "Buff", new ModifierSourceEntityBuilder());

            IValue BuildCondition(BuildParameters parameters)
            {
                var stat = _statFactory.FromIdentity($"Is {parameters.ModifierSource} active?",
                    parameters.ModifierSourceEntity, typeof(bool), ExplicitRegistrationTypes.UserSpecifiedValue());
                return new StatValue(stat);
            }
        }

        public IStatBuilder Temporary<T>(IStatBuilder gainedStat, T condition) where T : struct, Enum
        {
            var statBuilder = gainedStat
                .WithCondition(new ValueConditionBuilder(ps => BuildTemporaryBuffCondition(condition, ps)));
            return MultiplyValueByEffectModifier(statBuilder, "Buff", new ModifierSourceEntityBuilder());
        }

        public IStatBuilder Temporary<T>(IBuffBuilder buff, T condition) where T : struct, Enum
        {
            return buff.On(new ModifierSourceEntityBuilder())
                .WithCondition(new ValueConditionBuilder(ps => BuildTemporaryBuffCondition(condition, ps)));
        }

        private IValue BuildTemporaryBuffCondition<T>(T condition, BuildParameters parameters) where T : struct, Enum
        {
            var stat = _statFactory.FromIdentity($"Current {parameters.ModifierSource} stage",
                parameters.ModifierSourceEntity, typeof(T), ExplicitRegistrationTypes.UserSpecifiedValue());
            return new ConditionalValue(c => (int) c.GetValue(stat).Single() == Enums.ToInt32(condition),
                $"{stat} == {condition}");
        }

        public IStatBuilder Buff(IStatBuilder gainedStat, params IEntityBuilder[] affectedEntites)
            => MultiplyValueByEffectModifier(gainedStat, "Buff", new CompositeEntityBuilder(affectedEntites));

        public IStatBuilder Aura(IStatBuilder gainedStat, params IEntityBuilder[] affectedEntites)
            => MultiplyValueByEffectModifier(gainedStat, "Aura", new CompositeEntityBuilder(affectedEntites));

        private IStatBuilder MultiplyValueByEffectModifier(
            IStatBuilder gainedStat, string buffIdentity, IEntityBuilder targetEntities)
        {
            var coreStatBuilder = new StatBuilderWithValueConverter(new StatBuilderAdapter(gainedStat),
                target => new ValueBuilderImpl(ps => Build(ps, target), _ => ps => Build(ps, target)),
                (l, r) => l.Multiply(r));
            return new StatBuilder(_statFactory, coreStatBuilder).For(targetEntities);

            IValue Build(BuildParameters ps, Entity target)
                => new StatValue(_statFactory.BuffEffect(ps.ModifierSourceEntity, target, buffIdentity));
        }

        public IBuffBuilderCollection Buffs(IEntityBuilder source = null, params IEntityBuilder[] targets)
        {
            var allEntitiesBuilder = EntityBuilder.AllEntities;
            var sourceEntity = source ?? allEntitiesBuilder;
            var targetEntity = targets.Any() ? new CompositeEntityBuilder(targets) : allEntitiesBuilder;
            return new BuffBuilderCollection(_statFactory, _allBuffs, sourceEntity, targetEntity);
        }

        public IStatBuilder CurseLimit { get; }

        private class ConfluxBuffBuilders : IConfluxBuffBuilders
        {
            public ConfluxBuffBuilders(IStatFactory statFactory)
            {
                Igniting = new BuffBuilder(statFactory, CoreBuilder.Create("IgnitingConflux"));
                Shocking = new BuffBuilder(statFactory, CoreBuilder.Create("ShockingConflux"));
                Chilling = new BuffBuilder(statFactory, CoreBuilder.Create("ChillingConflux"));
                Elemental = new BuffBuilder(statFactory, CoreBuilder.Create("ElementalConflux"));
            }

            public IBuffBuilder Igniting { get; }
            public IBuffBuilder Shocking { get; }
            public IBuffBuilder Chilling { get; }
            public IBuffBuilder Elemental { get; }
        }
    }
}