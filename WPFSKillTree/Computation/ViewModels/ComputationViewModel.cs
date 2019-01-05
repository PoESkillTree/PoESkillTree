using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class ComputationViewModel : Notifier
    {
        public MainSkillSelectionViewModel MainSkillSelection { get; }
        public ResultStatsViewModel OffensiveStats { get; }
        public ResultStatsViewModel DefensiveStats { get; }
        public ObservableCollection<ConfigurationStatViewModel> ConfigurationStats { get; }

        private ComputationViewModel(SkillDefinitions skillDefinitions)
        {
            OffensiveStats = new ResultStatsViewModel();
            DefensiveStats = new ResultStatsViewModel();
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

            AddOffensiveStats(f.MetaStatBuilders.SkillDpsWithHits,
                new NodeValue(1234.56));
            AddOffensiveStats(f.DamageTypeBuilders.Physical.Damage.WithSkills.With(AttackDamageHand.MainHand),
                new NodeValue(2, 8));
            AddOffensiveStats(f.StatBuilders.HitRate);
            AddOffensiveStats(f.StatBuilders.Flag.CriticalStrikeChanceIsLucky,
                (NodeValue?) true);
            AddOffensiveStats(f.StatBuilders.Flag.FarShot);
            AddOffensiveStats(f.MetaStatBuilders.SkillHitDamageSource,
                new NodeValue((int) DamageSource.Attack));
            AddOffensiveStats(f.EffectBuilders.Stun.Threshold.With(AttackDamageHand.MainHand),
                new NodeValue(0.8), Entity.Enemy);
            AddAvailableStats(OffensiveStats, f.StatBuilders.BaseCastTime.With(AttackDamageHand.MainHand));
            AddAvailableStats(OffensiveStats, f.MetaStatBuilders.SkillDpsWithDoTs);

            AddDefensiveStats(f.StatBuilders.Pool.From(Pool.Life),
                new NodeValue(500));
            AddDefensiveStats(f.StatBuilders.Pool.From(Pool.Life),
                new NodeValue(100), nodeType: NodeType.Increase);
            AddDefensiveStats(f.DamageTypeBuilders.Cold.Resistance,
                new NodeValue(40));
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

        private void AddOffensiveStats(
            IStatBuilder statBuilder, NodeValue? value = null, Entity entity = Entity.Character,
            NodeType nodeType = NodeType.Total)
        {
            foreach (var stat in statBuilder.BuildToStats(entity))
            {
                OffensiveStats.AddStat(stat, value, nodeType);
            }
        }

        private void AddDefensiveStats(
            IStatBuilder statBuilder, NodeValue? value = null, Entity entity = Entity.Character,
            NodeType nodeType = NodeType.Total)
        {
            foreach (var stat in statBuilder.BuildToStats(entity))
            {
                DefensiveStats.AddStat(stat, value, nodeType);
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
            GameData gameData, IBuilderFactories builderFactories)
        {
            var skillDefinitions = await gameData.Skills;
            var vm = new ComputationViewModel(skillDefinitions);
            vm.Initialize(builderFactories);
            return vm;
        }
    }
}