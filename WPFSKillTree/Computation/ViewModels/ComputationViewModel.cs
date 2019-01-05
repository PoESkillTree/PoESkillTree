using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;
using POESKillTree.Computation.Model;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class ComputationViewModel : Notifier
    {
        public MainSkillSelectionViewModel MainSkillSelection { get; }
        public ResultStatsViewModel OffensiveStats { get; }
        public ResultStatsViewModel DefensiveStats { get; }
        public ObservableCollection<ConfigurationStatViewModel> ConfigurationStats { get; }

        private ComputationViewModel(SkillDefinitions skillDefinitions, ObservableCalculator observableCalculator)
        {
            OffensiveStats = new ResultStatsViewModel(observableCalculator);
            DefensiveStats = new ResultStatsViewModel(observableCalculator);
            ConfigurationStats = new ObservableCollection<ConfigurationStatViewModel>();
            MainSkillSelection = new MainSkillSelectionViewModel(skillDefinitions);
        }

        private void Initialize(IBuilderFactories f)
        {
            MainSkillSelection.AddSkill(new Skill("ChargedAttack", 20, 20, ItemSlot.Boots, 0, 0));
            MainSkillSelection.AddSkill(new Skill("Fireball", 21, 23, ItemSlot.Boots, 1, 0));
            MainSkillSelection.AddSkill(new Skill("BladeVortex", 18, 0, ItemSlot.Helm, 0, 0));
            MainSkillSelection.MaximumSkillStage = 10;
            MainSkillSelection.SkillStage = uint.MaxValue;

            AddStats(OffensiveStats, f.MetaStatBuilders.SkillDpsWithHits);
            AddStats(OffensiveStats, f.DamageTypeBuilders.Physical.Damage.WithSkills.With(AttackDamageHand.MainHand));
            AddStats(OffensiveStats, f.StatBuilders.HitRate);
            AddStats(OffensiveStats, f.StatBuilders.Flag.CriticalStrikeChanceIsLucky);
            AddStats(OffensiveStats, f.StatBuilders.Flag.FarShot);
            AddStats(OffensiveStats, f.MetaStatBuilders.SkillHitDamageSource);
            AddStats(OffensiveStats, f.EffectBuilders.Stun.Threshold.With(AttackDamageHand.MainHand), Entity.Enemy);
            AddAvailableStats(OffensiveStats, f.StatBuilders.BaseCastTime.With(AttackDamageHand.MainHand));
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.SkillDpsWithDoTs);

            AddStats(DefensiveStats, f.StatBuilders.Pool.From(Pool.Life));
            AddStats(DefensiveStats, f.StatBuilders.Pool.From(Pool.Life), nodeType: NodeType.Increase);
            AddStats(DefensiveStats, f.DamageTypeBuilders.Cold.Resistance);
            AddStats(DefensiveStats, f.StatBuilders.Attribute.Strength);
            AddStats(DefensiveStats, f.StatBuilders.Attribute.Dexterity);
            AddStats(DefensiveStats, f.StatBuilders.Attribute.Intelligence);
            AddAvailableStats(DefensiveStats, f.DamageTypeBuilders.AnyDamageType().Resistance);

            AddConfigurationStats(f.MetaStatBuilders.SelectedQuestPart, Entity.Character,
                new NodeValue((int) QuestPart.Epilogue));
            AddConfigurationStats(f.StatBuilders.Level, Entity.Enemy,
                new NodeValue(84), new NodeValue(0), new NodeValue(100));
            AddConfigurationStats(f.StatBuilders.Projectile.TravelDistance, Entity.Character,
                minimum: new NodeValue(0));
            AddConfigurationStats(f.EffectBuilders.Ailment.Shock.On(f.EntityBuilders.Enemy), Entity.Enemy);
            AddConfigurationStats(f.EffectBuilders.Ailment.Chill.On(f.EntityBuilders.Enemy), Entity.Enemy,
                (NodeValue?) true);
        }

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

        private void AddConfigurationStats(
            IStatBuilder statBuilder, Entity entity,
            NodeValue? value = null, NodeValue? minimum = null, NodeValue? maximum = null)
        {
            foreach (var stat in statBuilder.BuildToStats(entity))
            {
                ConfigurationStats.Add(new ConfigurationStatViewModel(stat)
                {
                    Value = value, Minimum = minimum, Maximum = maximum
                });
            }
        }

        public static async Task<ComputationViewModel> CreateAsync(
            GameData gameData, IBuilderFactories builderFactories, ObservableCalculator observableCalculator)
        {
            var skillDefinitions = await gameData.Skills;
            var vm = new ComputationViewModel(skillDefinitions, observableCalculator);
            vm.Initialize(builderFactories);
            return vm;
        }
    }
}