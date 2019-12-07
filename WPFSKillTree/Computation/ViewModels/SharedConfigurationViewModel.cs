using System.Linq;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel;

namespace PoESkillTree.Computation.ViewModels
{
    public class SharedConfigurationViewModel
    {
        private readonly CalculationNodeViewModelFactory _nodeFactory;

#pragma warning disable CS8618 // Instances can only be created via Create, which initializes all instance variables
        private SharedConfigurationViewModel(CalculationNodeViewModelFactory nodeFactory)
#pragma warning restore
            => _nodeFactory = nodeFactory;

        private ConfigurationStatViewModel _levelStat;
        private ConfigurationStatViewModel _characterClassStat;
        private ConfigurationStatViewModel _banditStat;

        public void SetLevel(int level)
            => _levelStat.Node.NumericValue = level;

        public void SetCharacterClass(CharacterClass characterClass)
            => _characterClassStat.Node.NumericValue = (int) characterClass;

        public void SetBandit(Bandit bandit)
            => _banditStat.Node.NumericValue = (int) bandit;

        public static SharedConfigurationViewModel Create(
            CalculationNodeViewModelFactory nodeFactory, IBuilderFactories builderFactories)
        {
            var vm = new SharedConfigurationViewModel(nodeFactory);
            vm.Initialize(builderFactories);
            return vm;
        }

        private void Initialize(IBuilderFactories f)
        {
            _levelStat = CreateConfigurationStat(f.StatBuilders.Level);
            _characterClassStat = CreateConfigurationStat(f.StatBuilders.CharacterClass);
            _banditStat = CreateConfigurationStat(f.MetaStatBuilders.SelectedBandit);
        }

        private ConfigurationStatViewModel CreateConfigurationStat(
            IStatBuilder statBuilder, Entity entity = Entity.Character)
        {
            var stat = statBuilder.BuildToStats(entity).Single();
            return ConfigurationStatViewModel.Create(_nodeFactory, stat);
        }
    }
}