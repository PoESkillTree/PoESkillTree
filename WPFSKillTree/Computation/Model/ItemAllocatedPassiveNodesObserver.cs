using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.Computation.Core;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree;

namespace PoESkillTree.Computation.Model
{
    public class ItemAllocatedPassiveNodesObservableFactory
    {
        private readonly ICalculator _calculator;
        private readonly IScheduler _calculationScheduler;
        private readonly IScheduler _observationScheduler;
        private readonly IPassiveTreeBuilders _passiveTreeBuilders;
        private readonly IEnumerable<PassiveNodeDefinition> _nodes;

        private ItemAllocatedPassiveNodesObservableFactory(
            ICalculator calculator, IScheduler calculationScheduler, IScheduler observationScheduler,
            IPassiveTreeBuilders passiveTreeBuilders, IEnumerable<PassiveNodeDefinition> nodes)
        {
            _calculator = calculator;
            _calculationScheduler = calculationScheduler;
            _observationScheduler = observationScheduler;
            _passiveTreeBuilders = passiveTreeBuilders;
            _nodes = nodes;
        }

        public static IObservable<IEnumerable<ushort>> Create(
            ICalculator calculator, IScheduler calculationScheduler, IScheduler observationScheduler,
            IPassiveTreeBuilders passiveTreeBuilders, IEnumerable<PassiveNodeDefinition> nodes)
        {
            var factory = new ItemAllocatedPassiveNodesObservableFactory(
                calculator, calculationScheduler, observationScheduler, passiveTreeBuilders, nodes);
            return factory.Create();
        }

        private IObservable<IEnumerable<ushort>> Create() =>
            RelevantNodes
                .Select(NodeChanges)
                .Merge()
                .ObserveOn(_observationScheduler)
                .Buffer(TimeSpan.FromMilliseconds(50), _observationScheduler)
                .Scan(Enumerable.Empty<ushort>(), Accumulate);

        private IEnumerable<ushort> RelevantNodes =>
            _nodes
                .Where(n => !n.IsAscendancyNode)
                .Where(n => n.Type == PassiveNodeType.Keystone || n.Type == PassiveNodeType.Notable)
                .Select(n => n.Id);

        private IObservable<(bool, ushort)> NodeChanges(ushort node) =>
            Observable.Create<bool>(o => Subscribe(node, o))
                .DistinctUntilChanged()
                .Select(b => (b, node))
                .SubscribeOn(_calculationScheduler);

        private Action Subscribe(ushort node, IObserver<bool> observer)
        {
            var allocatedStat = _passiveTreeBuilders.NodeAllocated(node).BuildToStats(Entity.Character).Single();
            var pointSpentStat = _passiveTreeBuilders.NodeSkillPointSpent(node).BuildToStats(Entity.Character).Single();
            var allocatedNode = _calculator.NodeRepository.GetNode(allocatedStat);
            var pointSpentNode = _calculator.NodeRepository.GetNode(pointSpentStat);

            allocatedNode.ValueChanged += ValueChanged;
            pointSpentNode.ValueChanged += ValueChanged;

            observer.OnNext(Value());
            void ValueChanged(object? _, EventArgs __) => observer.OnNext(Value());
            bool Value() => allocatedNode.Value.IsTrue() && !pointSpentNode.Value.IsTrue();

            return () =>
            {
                allocatedNode.ValueChanged -= ValueChanged;
                pointSpentNode.ValueChanged -= ValueChanged;
            };
        }

        private static IEnumerable<ushort> Accumulate(IEnumerable<ushort> previous, IEnumerable<(bool, ushort)> current)
        {
            var set = previous.ToHashSet();
            foreach (var (isItemAllocated, node) in current)
            {
                if (isItemAllocated)
                {
                    set.Add(node);
                }
                else
                {
                    set.Remove(node);
                }
            }

            return set;
        }
    }
}