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
        private readonly ObservableCalculator _observableCalculator;
        public MainSkillSelectionViewModel MainSkillSelection { get; }
        public ResultStatsViewModel OffensiveStats { get; }
        public ResultStatsViewModel DefensiveStats { get; }
        public ConfigurationStatsViewModel ConfigurationStats { get; }
        public SharedConfigurationViewModel SharedConfiguration { get; private set; }

        private ComputationViewModel(SkillDefinitions skillDefinitions, ObservableCalculator observableCalculator)
        {
            _observableCalculator = observableCalculator;
            OffensiveStats = new ResultStatsViewModel(observableCalculator);
            DefensiveStats = new ResultStatsViewModel(observableCalculator);
            ConfigurationStats = new ConfigurationStatsViewModel(observableCalculator);
            MainSkillSelection = new MainSkillSelectionViewModel(skillDefinitions);
        }

        private async Task InitializeAsync(IBuilderFactories f)
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
            AddStats(OffensiveStats, f.StatBuilders.AreaOfEffect);
            AddAvailableStats(OffensiveStats, f.StatBuilders.BaseCastTime.With(AttackDamageHand.MainHand));
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.SkillDpsWithDoTs);

            AddStats(DefensiveStats, f.StatBuilders.Pool.From(Pool.Life));
            AddStats(DefensiveStats, f.StatBuilders.Pool.From(Pool.Life), nodeType: NodeType.Increase);
            AddStats(DefensiveStats, f.DamageTypeBuilders.Cold.Resistance);
            AddStats(DefensiveStats, f.StatBuilders.Attribute.Strength);
            AddStats(DefensiveStats, f.StatBuilders.Attribute.Dexterity);
            AddStats(DefensiveStats, f.StatBuilders.Attribute.Intelligence);
            AddStats(DefensiveStats, f.StatBuilders.PassivePoints, nodeType: NodeType.UncappedSubtotal);
            AddStats(DefensiveStats, f.StatBuilders.PassivePoints.Maximum);
            AddStats(DefensiveStats, f.StatBuilders.AscendancyPassivePoints, nodeType: NodeType.UncappedSubtotal);
            AddStats(DefensiveStats, f.StatBuilders.AscendancyPassivePoints.Maximum);
            AddStats(DefensiveStats, f.MetaStatBuilders.SelectedBandit);
            AddAvailableStats(DefensiveStats, f.DamageTypeBuilders.AnyDamageType().Resistance);

            await AddConfigurationStatAsync(f.StatBuilders.Level, Entity.Enemy);
            await AddConfigurationStatAsync(f.MetaStatBuilders.SelectedQuestPart);
            ConfigurationStats.Observe();

            SharedConfiguration = SharedConfigurationViewModel.Create(_observableCalculator, f);
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

        private async Task AddConfigurationStatAsync(IStatBuilder statBuilder, Entity entity = Entity.Character)
        {
            foreach (var stat in statBuilder.BuildToStats(entity))
            {
                await ConfigurationStats.AddPinnedAsync(stat);
            }
        }

        public static async Task<ComputationViewModel> CreateAsync(
            GameData gameData, IBuilderFactories builderFactories, ObservableCalculator observableCalculator)
        {
            var skillDefinitions = await gameData.Skills;
            var vm = new ComputationViewModel(skillDefinitions, observableCalculator);
            await vm.InitializeAsync(builderFactories);
            return vm;
        }
    }
}