using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using log4net;
using PoESkillTree.Computation.Common.Builders;
using POESKillTree.Computation.Model;
using POESKillTree.Computation.ViewModels;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;

namespace POESKillTree.Computation
{
    public class ComputationInitializer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ComputationInitializer));

        private readonly GameDataWithOldTreeModel _gameData;

        private IBuilderFactories _builderFactories;
        private ComputationSchedulerProvider _schedulers;
        private ComputationObservables _observables;
        private ObservableCalculator _calculator;

        private readonly List<Task> _calculationTasks = new List<Task>();

        private ComputationInitializer()
        {
            _gameData = new GameDataWithOldTreeModel();
        }

        public static ComputationInitializer StartNew()
        {
            var instance = new ComputationInitializer();
            instance._gameData.Data.StartAllTasks();
            return instance;
        }

        public async Task InitializeAsync(IEnumerable<SkillNode> skillNodes)
        {
            await InitializeFields(skillNodes);
            var initialParseTask = DoInitialParseAsync();
            _calculationTasks.Add(initialParseTask);
        }

        private async Task InitializeFields(IEnumerable<SkillNode> skillNodes)
        {
            _gameData.PassiveNodes = skillNodes;

            var computationFactory = new ComputationFactory(_gameData.Data);
            var calculator = computationFactory.CreateCalculator();
            _builderFactories = await computationFactory.CreateBuilderFactoriesAsync();
            var parser = await computationFactory.CreateParserAsync();

            _schedulers = new ComputationSchedulerProvider();
            _observables = new ComputationObservables(parser);
            _calculator = new ObservableCalculator(calculator, _schedulers.CalculationThread);
        }

        private async Task DoInitialParseAsync()
        {
            var passiveTree = await _gameData.Data.PassiveTree;
            var initialObservable = _observables.InitialParse(passiveTree, TimeSpan.FromMilliseconds(200))
                .SubscribeOn(_schedulers.TaskPool);
            await _calculator.ForEachUpdateCalculatorAsync(initialObservable);
        }

        public async Task InitializeAfterBuildLoadAsync(ObservableSet<SkillNode> skilledNodes)
        {
            var skilledPassivesTask = ConnectToSkilledPassiveNodesAsync(skilledNodes);
            _calculationTasks.Add(skilledPassivesTask);
            await Task.WhenAll(_calculationTasks);
        }

        private async Task ConnectToSkilledPassiveNodesAsync(ObservableSet<SkillNode> skilledNodes)
        {
            var skilledNodesComputationObservable = _observables.ParseSkilledPassiveNodes(skilledNodes)
                .SubscribeOn(_schedulers.TaskPool);
            await _calculator.ForEachUpdateCalculatorAsync(skilledNodesComputationObservable);

            _calculator.SubscribeCalculatorTo(
                _observables.ObserveSkilledPassiveNodes(skilledNodes),
                ex => Log.Error("Exception while observing skilled node updates", ex));
        }

        public async Task<ComputationViewModel> CreateComputationViewModelAsync()
            => await ComputationViewModel.CreateAsync(_gameData.Data, _builderFactories, _calculator);

        public void SetupPeriodicActions()
            => _calculator.PeriodicallyRemoveUnusedNodes(
                ex => Log.Error("Exception while removing unused calculation nodes", ex));
    }
}