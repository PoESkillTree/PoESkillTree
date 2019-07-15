using System.Linq;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel;

namespace PoESkillTree.Computation.ViewModels
{
    public class SharedConfigurationViewModel
    {
        private readonly CalculationNodeViewModelFactory _nodeFactory;

        private SharedConfigurationViewModel(CalculationNodeViewModelFactory nodeFactory)
            => _nodeFactory = nodeFactory;

        private ConfigurationStatViewModel LevelStat { get; set; }
        private ConfigurationStatViewModel CharacterClassStat { get; set; }
        private ConfigurationStatViewModel BanditStat { get; set; }

        public void SetLevel(int level)
            => LevelStat.Node.NumericValue = level;

        public void SetCharacterClass(CharacterClass characterClass)
            => CharacterClassStat.Node.NumericValue = (int) characterClass;

        public void SetBandit(Bandit bandit)
            => BanditStat.Node.NumericValue = (int) bandit;

        public static SharedConfigurationViewModel Create(
            CalculationNodeViewModelFactory nodeFactory, IBuilderFactories builderFactories)
        {
            var vm = new SharedConfigurationViewModel(nodeFactory);
            vm.Initialize(builderFactories);
            return vm;
        }

        private void Initialize(IBuilderFactories f)
        {
            LevelStat = CreateConfigurationStat(f.StatBuilders.Level);
            CharacterClassStat = CreateConfigurationStat(f.StatBuilders.CharacterClass);
            BanditStat = CreateConfigurationStat(f.MetaStatBuilders.SelectedBandit);
        }

        private ConfigurationStatViewModel CreateConfigurationStat(
            IStatBuilder statBuilder, Entity entity = Entity.Character)
        {
            var stat = statBuilder.BuildToStats(entity).Single();
            return ConfigurationStatViewModel.Create(_nodeFactory, stat);
        }
    }
}