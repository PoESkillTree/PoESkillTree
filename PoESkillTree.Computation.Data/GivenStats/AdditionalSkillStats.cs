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
        private readonly Lazy<IReadOnlyList<IIntermediateModifier>> _lazyGivenStats;

        public AdditionalSkillStats(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
            _lazyGivenStats = new Lazy<IReadOnlyList<IIntermediateModifier>>(() => CreateCollection().ToList());
        }

        private IMetaStatBuilders MetaStats => BuilderFactories.MetaStatBuilders;

        public IReadOnlyList<Entity> AffectedEntities { get; } = new[] { GameModel.Entity.Character };

        public IReadOnlyList<string> GivenStatLines { get; } = new string[0];

        public IReadOnlyList<IIntermediateModifier> GivenModifiers => _lazyGivenStats.Value;

        private GivenStatCollection CreateCollection() => new GivenStatCollection(_modifierBuilder, ValueFactory)
        {
            {
                TotalOverride, MetaStats.SkillNumberOfHitsPerCast, Projectile.Count.Value,
                IsMainSkill("Barrage", 1)
            },

            { TotalOverride, Skills.FromId("BloodstainedBanner").Reservation, 0, Flag.IsBannerPlanted },
            { TotalOverride, Skills.FromId("BloodstainedBanner").Buff.StackCount.Maximum, 50 },

            { TotalOverride, Fire.Invert.Damage, 0, IsMainSkill("ElementalHit", 0) },
            { TotalOverride, Cold.Invert.Damage, 0, IsMainSkill("ElementalHit", 1) },
            { TotalOverride, Lightning.Invert.Damage, 0, IsMainSkill("ElementalHit", 2) },

            {
                // Freezing Pulse's damage dissipates while traveling
                // 60 * Projectile.Speed is the range, Projectile.TravelDistance / range is the percentage traveled
                PercentLess, Damage,
                ValueFactory.LinearScale(Projectile.TravelDistance / (60 * Projectile.Speed.Value),
                    (0, 0), (1, 50)),
                IsMainSkill("FreezingPulse")
            },
            {
                // Freezing Pulse's additional chance to freeze dissipates while traveling
                BaseAdd, Ailment.Freeze.Chance,
                ValueFactory.LinearScale(Projectile.TravelDistance / (60 * Projectile.Speed.Value),
                    (0, 25), (0.25, 0)),
                IsMainSkill("FreezingPulse")
            },

            {
                TotalOverride, MetaStats.SkillNumberOfHitsPerCast, Projectile.Count.Value,
                IsMainSkill("IceSpear", 1)
            },
            {
                TotalOverride, MetaStats.SkillNumberOfHitsPerCast, Projectile.Count.Value,
                IsMainSkill("IceSpear", 3)
            },

            {
                TotalOverride, MetaStats.SkillNumberOfHitsPerCast, Projectile.Count.Value,
                IsMainSkill("LancingSteel", 2)
            },
            // With the Primary and all Secondary Projectiles hitting, the Impale chance has to be adjusted
            // to be averaged across all hits.
            // The Primary Projectile has 100% Impale chance and is thus not affected by additional Impale chance
            // Derivation of the BaseAdd and PercentLess modifiers: (chances are between 0 and 1)
            //     Total chance = primary chance * 1/hits + secondary chance * (hits-1)/hits
            // <=> Total = primary * 1/hits + secondary * (hits-1)/hits
            // <=> Total = 1/hits + secondary * (hits-1)/hits                       (primary chance is 1)
            // <=> Total = (1/hits * hits/(hits-1) + secondary) * (hits-1)/hits
            // <=> Total = (1/(hits-1) + secondary) * (hits-1)/hits
            // With x = 1/(hits-1) and y = (hits-1)/hits we get
            //     Total = (x + secondary) * y
            // With x = 1/(hits-1) as a BaseAdd modifier (after multiplying by 100 to get a 0 to 100 based value),
            // 1 - y = 1/hits as a PercentLess modifier (after multiplying by 100) and
            // secondary simply being the sum of all parsed Impale chance modifiers, Total is calculated correctly.
            {
                BaseAdd, Buff.Impale.Chance.WithCondition(Hit.On), 100 / (MetaStats.SkillNumberOfHitsPerCast.Value - 1),
                IsMainSkill("LancingSteel", 2)
            },
            {
                PercentLess, Buff.Impale.Chance.WithCondition(Hit.On), 100 / MetaStats.SkillNumberOfHitsPerCast.Value,
                IsMainSkill("LancingSteel", 2)
            },

            { TotalOverride, Skills.FromId("PuresteelBanner").Reservation, 0, Flag.IsBannerPlanted },
            { TotalOverride, Skills.FromId("PuresteelBanner").Buff.StackCount.Maximum, 50 },

            {
                // Reduce cast rate proportional to the time spent channeling
                PercentLess, Stat.CastRate,
                100 * (Stat.SkillStage.Maximum.Value - Stat.SkillStage.Value + 1) / Stat.SkillStage.Maximum.Value,
                IsMainSkill("ScourgeArrow").And(Stat.SkillStage.Value > 0)
            },

            {
                TotalOverride, MetaStats.SkillNumberOfHitsPerCast, Projectile.Count.Value,
                IsMainSkill("ShatteringSteel", 2)
            },

            { TotalOverride, Buff.ArcaneSurge.On(Self), 1, SkillIsActive("SupportArcaneSurge") },

            { TotalOverride, Buff.Innervation.On(Self), 1, SkillIsActive("SupportOnslaughtOnSlayingShockedEnemy") },
        };

        private IConditionBuilder IsMainSkill(string skillId, int skillPart)
            => IsMainSkill(skillId).And(Stat.MainSkillPart.Value.Eq(skillPart));

        private IConditionBuilder IsMainSkill(string skillId)
            => MetaStats.MainSkillId.Value.Eq(Skills.FromId(skillId).SkillId);

        private IConditionBuilder SkillIsActive(string skillId)
            => MetaStats.ActiveSkillItemSlot(skillId).IsSet;
    }
}