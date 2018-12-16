using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Data.GivenStats
{
    /// <summary>
    /// Additional modifiers that are required for skills to work as intended and can't be added through
    /// <see cref="GameModel.Skills.SkillDefinitionExtensions"/>.
    /// </summary>
    public class AdditionalSkillStats : UsesConditionBuilders, IGivenStats
    {
        private readonly IModifierBuilder _modifierBuilder;
        private readonly IMetaStatBuilders _stat;
        private readonly Lazy<IReadOnlyList<IIntermediateModifier>> _lazyGivenStats;

        public AdditionalSkillStats(
            IBuilderFactories builderFactories, IModifierBuilder modifierBuilder, IMetaStatBuilders metaStatBuilders)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
            _stat = metaStatBuilders;
            _lazyGivenStats = new Lazy<IReadOnlyList<IIntermediateModifier>>(() => CreateCollection().ToList());
        }

        public IReadOnlyList<Entity> AffectedEntities { get; } = new[] { GameModel.Entity.Character };

        public IReadOnlyList<string> GivenStatLines { get; } = new string[0];

        public IReadOnlyList<IIntermediateModifier> GivenModifiers => _lazyGivenStats.Value;

        private GivenStatCollection CreateCollection() => new GivenStatCollection(_modifierBuilder, ValueFactory)
        {
            { TotalOverride, _stat.SkillNumberOfHitsPerCast, Projectile.Count.Value, IsMainSkill("Barrage", 1) },
            { TotalOverride, Fire.Invert.Damage, 0, IsMainSkill("ElementalHit", 0) },
            { TotalOverride, Cold.Invert.Damage, 0, IsMainSkill("ElementalHit", 1) },
            { TotalOverride, Lightning.Invert.Damage, 0, IsMainSkill("ElementalHit", 2) },
            {
                // Reduce cast rate proportional to the time spent channeling
                PercentLess, Stat.CastRate,
                100 * (Stat.SkillStage.Maximum.Value - Stat.SkillStage.Value + 1) / Stat.SkillStage.Maximum.Value,
                IsMainSkill("ScourgeArrow").And(Stat.SkillStage.Value > 0)
            },
            {
                // Freezing Pulse's damage dissipates while traveling
                // 60 * Projectile.Speed is the range, Projectile.TravelDistance / range is the percentage traveled
                PercentLess, Damage,
                ValueFactory.LinearScale(Projectile.TravelDistance.Value / (60 * Projectile.Speed.Value),
                    (0, 0), (1, 50)),
                IsMainSkill("FreezingPulse")
            },
            {
                // Freezing Pulse's additional chance to freeze dissipates while traveling
                BaseAdd, Ailment.Freeze.Chance,
                ValueFactory.LinearScale(Projectile.TravelDistance.Value / (60 * Projectile.Speed.Value),
                    (0, 25), (0.25, 0)),
                IsMainSkill("FreezingPulse")
            },
            { TotalOverride, _stat.SkillNumberOfHitsPerCast, Projectile.Count.Value, IsMainSkill("IceSpear", 1) },
            { TotalOverride, _stat.SkillNumberOfHitsPerCast, Projectile.Count.Value, IsMainSkill("IceSpear", 3) },
            { TotalOverride, Buff.ArcaneSurge.On(Self), 1, SkillIsActive("SupportArcaneSurge") },
            { TotalOverride, Buff.Innervation.On(Self), 1, SkillIsActive("SupportOnslaughtOnSlayingShockedEnemy") },
        };

        private IConditionBuilder IsMainSkill(string skillId, int skillPart)
            => IsMainSkill(skillId).And(Stat.MainSkillPart.Value.Eq(skillPart));

        private IConditionBuilder IsMainSkill(string skillId)
            => _stat.MainSkillId.Value.Eq(Skills.FromId(skillId).SkillId);

        private IConditionBuilder SkillIsActive(string skillId)
            => _stat.ActiveSkillItemSlot(skillId).IsSet;
    }
}