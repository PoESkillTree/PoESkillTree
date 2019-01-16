using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using log4net;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Core;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
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
            instance.GameData.StartAllTasks();
            return instance;
        }

        public GameData GameData => _gameData.Data;

        public async Task InitializeAsync(IEnumerable<SkillNode> skillNodes)
        {
            await InitializeFields(skillNodes);
            var initialParseTask = DoInitialParseAsync();
            _calculationTasks.Add(initialParseTask);
        }

        private async Task InitializeFields(IEnumerable<SkillNode> skillNodes)
        {
            _gameData.PassiveNodes = skillNodes;

            var computationFactory = new ComputationFactory(GameData);
            var calculator = computationFactory.CreateCalculator();
            _builderFactories = await computationFactory.CreateBuilderFactoriesAsync();
            var parser = await computationFactory.CreateParserAsync();

            _schedulers = new ComputationSchedulerProvider();
            _observables = new ComputationObservables(parser);
            _calculator = new ObservableCalculator(calculator, _schedulers.CalculationThread);
        }

        private async Task DoInitialParseAsync()
        {
            var passiveTree = await GameData.PassiveTree;
            var initialObservable = _observables.InitialParse(passiveTree, TimeSpan.FromMilliseconds(200))
                .SubscribeOn(_schedulers.TaskPool);
            await _calculator.ForEachUpdateCalculatorAsync(initialObservable);
        }

        public async Task InitializeAfterBuildLoadAsync(
            ObservableSet<SkillNode> skilledNodes, ObservableCollection<(Item, ItemSlot)> items)
        {
            var skilledPassivesTask = ConnectToSkilledPassiveNodesAsync(skilledNodes);
            _calculationTasks.Add(skilledPassivesTask);
            var equipmentTask = ConnectToEquipmentAsync(items);
            _calculationTasks.Add(equipmentTask);
            await Task.WhenAll(_calculationTasks);
        }

        private async Task ConnectToSkilledPassiveNodesAsync(ObservableSet<SkillNode> skilledNodes)
            => await ConnectAsync(
                _observables.ParseSkilledPassiveNodes(skilledNodes),
                _observables.ObserveSkilledPassiveNodes(skilledNodes));

        private async Task ConnectToEquipmentAsync(ObservableCollection<(Item, ItemSlot)> items)
            => await ConnectAsync(
                _observables.ParseItems(items),
                _observables.ObserveItems(items));

        private async Task ConnectAsync(
            IObservable<CalculatorUpdate> initialObservable, IObservable<CalculatorUpdate> changeObservable)
        {
            await _calculator.ForEachUpdateCalculatorAsync(
                initialObservable.SubscribeOn(_schedulers.TaskPool));
            _calculator.SubscribeCalculatorTo(
                changeObservable, ex => Log.Error("Exception while observing updates", ex));
        }

        public async Task<ComputationViewModel> CreateComputationViewModelAsync()
            => await ComputationViewModel.CreateAsync(GameData, _builderFactories, _calculator, _schedulers);

        public void SetupPeriodicActions()
            => _calculator.PeriodicallyRemoveUnusedNodes(
                ex => Log.Error("Exception while removing unused calculation nodes", ex));
    }
}