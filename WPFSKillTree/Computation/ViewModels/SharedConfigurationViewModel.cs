using System.Linq;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using POESKillTree.Computation.Model;

namespace POESKillTree.Computation.ViewModels
{
    public class SharedConfigurationViewModel
    {
        private readonly ObservableCalculator _observableCalculator;

        private SharedConfigurationViewModel(ObservableCalculator observableCalculator)
            => _observableCalculator = observableCalculator;

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
            ObservableCalculator observableCalculator, IBuilderFactories builderFactories)
        {
            var vm = new SharedConfigurationViewModel(observableCalculator);
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
            var vm = new ConfigurationStatViewModel(stat);
            vm.Observe(_observableCalculator);
            return vm;
        }
    }
}