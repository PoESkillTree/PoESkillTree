using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using EnumsNET;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.PassiveTree;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils;
using PoESkillTree.Utils.Extensions;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;

namespace POESKillTree.Computation.Model
{
    public class ComputationObservables
    {
        private readonly IParser _parser;

        public ComputationObservables(IParser parser)
            => _parser = parser;

        public IObservable<CalculatorUpdate> InitialParse(
            PassiveTreeDefinition passiveTreeDefinition, TimeSpan bufferTimeSpan, IScheduler scheduler)
        {
            var givenResultObservable = _parser.CreateGivenModifierParseDelegates().ToObservable()
                .SelectMany(d => Observable.Start(d, scheduler));
            var passiveNodesObservable = passiveTreeDefinition.Nodes.ToObservable()
                .SelectMany(n => Observable.Start(() => _parser.ParsePassiveNode(n.Id).Modifiers, scheduler));
            return givenResultObservable.Merge(passiveNodesObservable)
                .Buffer(bufferTimeSpan)
                .Select(ms => ms.Flatten().ToList())
                .Where(ms => ms.Any())
                .Select(ms => new CalculatorUpdate(ms, new Modifier[0]));
        }

        public IObservable<CalculatorUpdate> ParseSkilledPassiveNodes(IEnumerable<SkillNode> skilledNodes)
            => AggregateModifiers(
                skilledNodes.Select(n => n.Id).ToObservable()
                    .SelectMany(ParseSkilledNode));

        public IObservable<CalculatorUpdate> ObserveSkilledPassiveNodes(ObservableSet<SkillNode> skilledNodes)
            => ObserveCollection(skilledNodes, n => ParseSkilledNode(n.Id));

        private IReadOnlyList<Modifier> ParseSkilledNode(ushort nodeId)
            => _parser.ParseSkilledPassiveNode(nodeId).Modifiers;

        public IObservable<CalculatorUpdate> ParseItems(IEnumerable<(Item item, ItemSlot slot)> items)
        {
            var itemsBySlot = items.ToDictionary(t => t.slot, t => t.item);
            return AggregateModifiers(
                Enums.GetValues<ItemSlot>().ToObservable()
                    .Select(Parse)
                    .SelectMany(r => r.Modifiers));

            ParseResult Parse(ItemSlot slot)
                => itemsBySlot.TryGetValue(slot, out var item)
                    ? _parser.ParseItem(item, slot)
                    : ParseResult.Empty;
        }

        public IObservable<CalculatorUpdate> ObserveItems(ObservableSet<(Item item, ItemSlot slot)> items)
            => ObserveCollection(items, t => _parser.ParseItem(t.item, t.slot).Modifiers);

        public IObservable<CalculatorUpdate> ParseSkills(IEnumerable<IReadOnlyList<Skill>> skills)
            => AggregateModifiers(skills.ToObservable().SelectMany(ParseSkills));

        public IObservable<CalculatorUpdate> ObserveSkills(ObservableSet<IReadOnlyList<Skill>> skills)
            => ObserveCollection(skills, ParseSkills);

        private IReadOnlyList<Modifier> ParseSkills(IReadOnlyList<Skill> skills)
            => _parser.ParseSkills(skills).Modifiers;

        private static IObservable<CalculatorUpdate> AggregateModifiers(IObservable<Modifier> modifiers)
            => modifiers.Aggregate(Enumerable.Empty<Modifier>(), (ms, m) => ms.Append(m))
                .Select(ms => new CalculatorUpdate(ms.ToList(), new Modifier[0]))
                .Where(UpdateIsNotEmpty);

        private static IObservable<CalculatorUpdate> ObserveCollection<T>(
            INotifyCollectionChanged<T> collection, Func<T, IReadOnlyList<Modifier>> parse)
            => ObserveCollection(collection, t => new CalculatorUpdate(parse(t), new Modifier[0]));

        private static IObservable<CalculatorUpdate> ObserveCollection<T>(
            INotifyCollectionChanged<T> collection, Func<T, CalculatorUpdate> parse)
        {
            return Observable.FromEventPattern<CollectionChangedEventHandler<T>, CollectionChangedEventArgs<T>>(
                    h => collection.CollectionChanged += h,
                    h => collection.CollectionChanged -= h)
                .Select(p => p.EventArgs)
                .Select(args =>
                {
                    var addUpdate = Parse(args.AddedItems);
                    var removeUpdate = Parse(args.RemovedItems).Invert();
                    return CalculatorUpdate.Accumulate(addUpdate, removeUpdate);
                })
                .Where(UpdateIsNotEmpty);

            CalculatorUpdate Parse(IEnumerable<T> changedItems)
                => changedItems.Select(parse).Aggregate(CalculatorUpdate.Empty, CalculatorUpdate.Accumulate);
        }

        private static bool UpdateIsNotEmpty(CalculatorUpdate update)
            => update.AddedModifiers.Any() || update.RemovedModifiers.Any();
    }
}