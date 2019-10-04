using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using NLog;
using PoESkillTree.Computation.Model;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;

namespace PoESkillTree.ViewModels.Equipment
{
    public class AbyssalSocketObserver
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly ObservableCalculator _observableCalculator;
        private readonly IStat _abyssalSocketsStat;
        private readonly IScheduler _observeScheduler;

        private IReadOnlyDictionary<ItemSlot, IReadOnlyList<InventoryItemViewModel>> _jewels;
        private IDisposable _subscription;

        private AbyssalSocketObserver(
            ObservableCalculator observableCalculator, IScheduler observeScheduler, IStat abyssalSocketsStat)
            => (_observableCalculator, _observeScheduler, _abyssalSocketsStat)
                = (observableCalculator, observeScheduler, abyssalSocketsStat);

        public static AbyssalSocketObserver Create(
            ObservableCalculator observableCalculator, IScheduler observeScheduler, IBuilderFactories builderFactories)
        {
            var abyssalSocketsStat = builderFactories.StatBuilders.AbyssalSockets
                .BuildToStats(Entity.Character).Single();
            return new AbyssalSocketObserver(observableCalculator, observeScheduler, abyssalSocketsStat);
        }

        public void ResetItemJewelViewModels()
        {
            _subscription?.Dispose();
            _jewels = null;
        }

        public void SetItemJewelViewModels(
            IReadOnlyDictionary<ItemSlot, IReadOnlyList<InventoryItemViewModel>> jewels)
        {
            ResetItemJewelViewModels();
            _jewels = jewels;
            var subscriptions = jewels.Keys.Select(SubscribeToAbyssalSocketsForSlot);
            _subscription = new CompositeDisposable(subscriptions);
        }

        private IDisposable SubscribeToAbyssalSocketsForSlot(ItemSlot slot)
            => _observableCalculator
                .ObserveNode(_abyssalSocketsStat, NodeType.PathTotal, new ModifierSource.Local.Item(slot))
                .ObserveOn(_observeScheduler)
                .Subscribe(
                    v => AbyssalSocketsStatOnValueChanged(slot, v),
                    ex => Log.Error(ex, $"SubscribeToAbyssalSocketsForSlot({slot}) failed"));

        private void AbyssalSocketsStatOnValueChanged(ItemSlot slot, NodeValue? value)
        {
            if (_jewels is null)
                return;

            var jewelsToEnable = (int) (value.SingleOrNull() ?? 0);
            for (var i = 0; i < _jewels[slot].Count; i++)
            {
                _jewels[slot][i].IsEnabled = i < jewelsToEnable;
            }
        }
    }
}