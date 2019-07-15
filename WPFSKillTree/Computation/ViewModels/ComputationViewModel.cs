using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnumsNET;
using MoreLinq;
using PoESkillTree.Computation.Model;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Charges;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Effects;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.ViewModels
{
    public class ComputationViewModel : Notifier
    {
        private readonly ObservableCalculator _observableCalculator;
        private readonly CalculationNodeViewModelFactory _nodeFactory;

        public MainSkillSelectionViewModel MainSkillSelection { get; private set; }
        public ResultStatsViewModel OffensiveStats { get; }
        public ResultStatsViewModel DefensiveStats { get; }
        public ConfigurationStatsViewModel ConfigurationStats { get; private set; }
        public GainOnActionStatsViewModel GainOnActionStats { get; private set; }
        public SharedConfigurationViewModel SharedConfiguration { get; private set; }
        public IndependentResultStatsViewModel IndependentResultStats { get; private set; }

        private ComputationViewModel(ObservableCalculator observableCalculator, ComputationSchedulerProvider schedulers)
        {
            _observableCalculator = observableCalculator;
            var modifierNodeFactory = new ModifierNodeViewModelFactory(observableCalculator);
            _nodeFactory =
                new CalculationNodeViewModelFactory(modifierNodeFactory, observableCalculator, schedulers.Dispatcher);
            OffensiveStats = new ResultStatsViewModel(_nodeFactory);
            DefensiveStats = new ResultStatsViewModel(_nodeFactory);
        }

        private async Task InitializeAsync(
            SkillDefinitions skillDefinitions, IBuilderFactories f, ObservableSet<IReadOnlyList<Skill>> skills)
        {
            MainSkillSelection = MainSkillSelectionViewModel.Create(skillDefinitions, f, _nodeFactory, skills);

            InitializeOffensiveStats(f);
            InitializeDefensiveStats(f);

            ConfigurationStats = await ConfigurationStatsViewModel.CreateAsync(_observableCalculator, _nodeFactory);
            AddConfigurationStat(f.StatBuilders.Level, Entity.Enemy);

            GainOnActionStats = await GainOnActionStatsViewModel.CreateAsync(_observableCalculator, _nodeFactory);
            SharedConfiguration = SharedConfigurationViewModel.Create(_nodeFactory, f);
            IndependentResultStats =
                await IndependentResultStatsViewModel.CreateAsync(_observableCalculator, _nodeFactory);
        }

        private void InitializeOffensiveStats(IBuilderFactories f)
        {
            AddStats(OffensiveStats, f.MetaStatBuilders.SkillDpsWithHits);
            AddStats(OffensiveStats, f.MetaStatBuilders.SkillDpsWithDoTs);
            ForEachDamagingAilment(a => AddStats(OffensiveStats, f.MetaStatBuilders.AilmentDps(a)));

            AddStats(OffensiveStats, f.MetaStatBuilders.AverageHitDamage);
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.AverageDamage);
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.AverageDamagePerHit);
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.DamageWithNonCrits());
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.DamageWithCrits());
            ForEachDamageType(t => AddAvailableStats(OffensiveStats, f.MetaStatBuilders.DamageWithNonCrits(t)));
            ForEachDamageType(t => AddAvailableStats(OffensiveStats, f.MetaStatBuilders.DamageWithCrits(t)));
            ForEachDamageType(
                t => AddAvailableStats(OffensiveStats, f.MetaStatBuilders.EnemyResistanceAgainstCrits(t)));
            ForEachDamageType(
                t => AddAvailableStats(OffensiveStats, f.MetaStatBuilders.EnemyResistanceAgainstNonCrits(t)));
            AddStats(OffensiveStats,
                f.MetaStatBuilders.Damage(DamageType.Physical).WithSkills.With(DamageSource.Attack));
            AddStats(OffensiveStats,
                f.MetaStatBuilders.Damage(DamageType.Physical).WithSkills.With(DamageSource.Spell));
            AddStats(OffensiveStats,
                f.MetaStatBuilders.Damage(DamageType.Lightning).WithSkills.With(DamageSource.Attack));
            AddStats(OffensiveStats,
                f.MetaStatBuilders.Damage(DamageType.Lightning).WithSkills.With(DamageSource.Spell));
            ForEachDamageType(t => AddAvailableStats(OffensiveStats, f.MetaStatBuilders.Damage(t)));

            AddStats(OffensiveStats, f.StatBuilders.CastRate);
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.CastRate);
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.CastTime);
            AddAvailableStats(OffensiveStats, f.StatBuilders.BaseCastTime.With(AttackDamageHand.MainHand));
            AddAvailableStats(OffensiveStats, f.StatBuilders.HitRate);
            AddAvailableStats(OffensiveStats, f.StatBuilders.ActionSpeed);
            
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.EffectiveCritChance);
            AddStats(OffensiveStats, f.ActionBuilders.CriticalStrike.Chance);
            AddAvailableStats(OffensiveStats, f.ActionBuilders.CriticalStrike.Multiplier);

            AddStats(OffensiveStats, f.StatBuilders.ChanceToHit);
            AddAvailableStats(OffensiveStats, f.StatBuilders.Accuracy);

            ForEachDamagingAilment(
                a => AddAvailableStats(OffensiveStats, f.MetaStatBuilders.AilmentInstanceLifetimeDamage(a)));
            ForEachDamagingAilment(a => AddAvailableStats(OffensiveStats, f.MetaStatBuilders.AverageAilmentDamage(a)));
            ForEach<Ailment>(a => AddAvailableStats(OffensiveStats, f.MetaStatBuilders.AilmentEffectiveChance(a)));
            ForEach<Ailment>(
                a => AddAvailableStats(OffensiveStats, f.MetaStatBuilders.AilmentCombinedEffectiveChance(a)));
            ForEach<Ailment>(a => AddAvailableStats(OffensiveStats, f.EffectBuilders.Ailment.From(a).Chance));
            ForEach<Ailment>(a => AddAvailableStats(OffensiveStats, f.EffectBuilders.Ailment.From(a).Duration));
            ForEachDamagingAilment(
                a => AddAvailableStats(OffensiveStats, f.MetaStatBuilders.AilmentEffectiveInstances(a)));
            
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.SkillHitDamageSource);
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.SkillNumberOfHitsPerCast);
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.SkillUsesHand(AttackDamageHand.MainHand));
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.SkillUsesHand(AttackDamageHand.OffHand));

            AddAvailableStats(OffensiveStats, f.EffectBuilders.Stun.Chance);
            AddAvailableStats(OffensiveStats, f.EffectBuilders.Stun.Duration);
            AddAvailableStats(OffensiveStats, f.EffectBuilders.Stun.Threshold, Entity.Enemy);
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.EffectiveStunThreshold, Entity.Enemy);

            AddAvailableStats(OffensiveStats, f.StatBuilders.AreaOfEffect);
            AddAvailableStats(OffensiveStats, f.StatBuilders.Cooldown);
            AddAvailableStats(OffensiveStats, f.StatBuilders.CooldownRecoverySpeed);
            AddAvailableStats(OffensiveStats, f.StatBuilders.Duration);
            AddAvailableStats(OffensiveStats, f.StatBuilders.Range);
            AddAvailableStats(OffensiveStats, f.StatBuilders.Projectile.Count);
            AddAvailableStats(OffensiveStats, f.StatBuilders.Projectile.Fork);
            AddAvailableStats(OffensiveStats, f.StatBuilders.Projectile.Speed);
        }

        private void InitializeDefensiveStats(IBuilderFactories f)
        {
            AddStats(DefensiveStats, f.StatBuilders.Attribute.Strength);
            AddStats(DefensiveStats, f.StatBuilders.Attribute.Dexterity);
            AddStats(DefensiveStats, f.StatBuilders.Attribute.Intelligence);
            AddAvailableStats(DefensiveStats, f.StatBuilders.Requirements.Level);
            AddAvailableStats(DefensiveStats, f.StatBuilders.Requirements.Strength);
            AddAvailableStats(DefensiveStats, f.StatBuilders.Requirements.Dexterity);
            AddAvailableStats(DefensiveStats, f.StatBuilders.Requirements.Intelligence);

            AddStats(DefensiveStats, f.StatBuilders.Pool.From(Pool.Life));
            AddStats(DefensiveStats, f.StatBuilders.Pool.From(Pool.Life), nodeType: NodeType.Increase);
            AddStats(DefensiveStats, f.StatBuilders.Pool.From(Pool.Mana));
            AddStats(DefensiveStats, f.StatBuilders.Pool.From(Pool.EnergyShield));
            ForEach<Pool>(p => AddAvailableStats(DefensiveStats, f.StatBuilders.Pool.From(p).Cost));
            ForEach<Pool>(p => AddAvailableStats(DefensiveStats, f.StatBuilders.Pool.From(p).Regen));
            ForEach<Pool>(p => AddAvailableStats(DefensiveStats, f.StatBuilders.Pool.From(p).Regen.Percent));
            ForEach<Pool>(p => AddAvailableStats(DefensiveStats, f.StatBuilders.Pool.From(p).Reservation));
            ForEach<Pool>(p => AddAvailableStats(DefensiveStats, f.MetaStatBuilders.EffectiveRegen(p)));
            AddAvailableStats(DefensiveStats, f.StatBuilders.Pool.From(Pool.EnergyShield).Recharge);
            AddAvailableStats(DefensiveStats, f.StatBuilders.Pool.From(Pool.EnergyShield).Recharge.Start);
            AddAvailableStats(DefensiveStats, f.MetaStatBuilders.EffectiveRegen(Pool.EnergyShield));
            AddAvailableStats(DefensiveStats, f.MetaStatBuilders.EffectiveRecharge(Pool.EnergyShield));
            AddAvailableStats(DefensiveStats, f.MetaStatBuilders.RechargeStartDelay(Pool.EnergyShield));

            AddStats(DefensiveStats, f.StatBuilders.Armour);
            AddStats(DefensiveStats, f.StatBuilders.Evasion);
            AddStats(DefensiveStats, f.StatBuilders.Evasion.Chance);
            AddAvailableStats(DefensiveStats, f.StatBuilders.Evasion.ChanceAgainstMeleeAttacks);
            AddAvailableStats(DefensiveStats, f.StatBuilders.Evasion.ChanceAgainstProjectileAttacks);
            AddAvailableStats(DefensiveStats, f.StatBuilders.Accuracy, Entity.Enemy);
            AddStats(DefensiveStats, f.DamageTypeBuilders.AnyDamageType().Resistance);
            ForEachDamageType(t => AddAvailableStats(DefensiveStats, f.MetaStatBuilders.ResistanceAgainstHits(t)));
            ForEachDamageType(t => AddAvailableStats(DefensiveStats, f.MetaStatBuilders.MitigationAgainstHits(t)));
            ForEachDamageType(t => AddAvailableStats(DefensiveStats, f.MetaStatBuilders.MitigationAgainstDoTs(t)));
            ForEachDamageType(t => AddAvailableStats(DefensiveStats, f.MetaStatBuilders.MitigationAgainstDoTs(t)));
            ForEachDamageType(
                t => AddAvailableStats(DefensiveStats, f.DamageTypeBuilders.From(t).Damage.Taken.WithSkills));
            AddAvailableStats(DefensiveStats, f.ActionBuilders.CriticalStrike.ExtraDamageTaken);

            AddAvailableStats(DefensiveStats, f.StatBuilders.Dodge.AttackChance);
            AddAvailableStats(DefensiveStats, f.StatBuilders.Dodge.SpellChance);
            AddAvailableStats(DefensiveStats, f.ActionBuilders.Block.AttackChance);
            AddAvailableStats(DefensiveStats, f.ActionBuilders.Block.SpellChance);
            AddAvailableStats(DefensiveStats, f.ActionBuilders.Block.Recovery);
            AddAvailableStats(DefensiveStats, f.MetaStatBuilders.ChanceToAvoidMeleeAttacks);
            AddAvailableStats(DefensiveStats, f.MetaStatBuilders.ChanceToAvoidProjectileAttacks);
            AddAvailableStats(DefensiveStats, f.MetaStatBuilders.ChanceToAvoidSpells);
            AddAvailableStats(DefensiveStats, f.EffectBuilders.Stun.Avoidance);
            AddAvailableStats(DefensiveStats, f.EffectBuilders.Stun.Recovery);
            AddAvailableStats(DefensiveStats, f.EffectBuilders.Stun.ChanceToAvoidInterruptionWhileCasting);
            AddAvailableStats(DefensiveStats, f.MetaStatBuilders.StunAvoidanceWhileCasting);

            AddAvailableStats(DefensiveStats, f.StatBuilders.MovementSpeed);
            AddAvailableStats(DefensiveStats, f.StatBuilders.ItemQuantity);
            AddAvailableStats(DefensiveStats, f.StatBuilders.ItemRarity);

            AddAvailableStats(DefensiveStats, f.StatBuilders.PassivePoints);
            AddAvailableStats(DefensiveStats, f.StatBuilders.PassivePoints.Maximum);
            AddAvailableStats(DefensiveStats, f.StatBuilders.AscendancyPassivePoints);
            AddAvailableStats(DefensiveStats, f.StatBuilders.AscendancyPassivePoints.Maximum);
            AddAvailableStats(DefensiveStats, f.MetaStatBuilders.SelectedBandit);

            ForEach<ChargeType>(t => AddAvailableStats(DefensiveStats, f.ChargeTypeBuilders.From(t).Amount));
            ForEach<ChargeType>(t => AddAvailableStats(DefensiveStats, f.ChargeTypeBuilders.From(t).Amount.Maximum));
        }

        private static void ForEachDamageType(Action<DamageType> action)
            => Enums.GetValues<DamageType>().Where(t => t != DamageType.RandomElement).ForEach(action);

        private static void ForEachDamagingAilment(Action<Ailment> action)
            => AilmentConstants.DamagingAilments.ForEach(action);

        private static void ForEach<T>(Action<T> action) where T : struct, Enum
            => Enums.GetValues<T>().ForEach(action);

        private static void AddAvailableStats(
            ResultStatsViewModel viewModel, IStatBuilder statBuilder, Entity entity = Entity.Character)
        {
            foreach (var stat in statBuilder.BuildToStats(entity))
            {
                viewModel.AddAvailableStat(stat);
            }
        }

        private static void AddStats(
            ResultStatsViewModel viewModel,
            IStatBuilder statBuilder, Entity entity = Entity.Character,
            NodeType nodeType = NodeType.Total)
        {
            foreach (var stat in statBuilder.BuildToStats(entity))
            {
                viewModel.AddStat(stat, nodeType);
            }
        }

        private void AddConfigurationStat(IStatBuilder statBuilder, Entity entity = Entity.Character)
        {
            foreach (var stat in statBuilder.BuildToStats(entity))
            {
                ConfigurationStats.AddPinned(stat);
            }
        }

        public static async Task<ComputationViewModel> CreateAsync(
            GameData gameData, IBuilderFactories builderFactories,
            ObservableCalculator observableCalculator, ComputationSchedulerProvider schedulers,
            ObservableSet<IReadOnlyList<Skill>> skills)
        {
            var skillDefinitions = await gameData.Skills;
            var vm = new ComputationViewModel(observableCalculator, schedulers);
            await vm.InitializeAsync(skillDefinitions, builderFactories, skills);
            return vm;
        }
    }
}