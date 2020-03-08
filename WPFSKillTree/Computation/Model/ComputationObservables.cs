using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using EnumsNET;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Core;
using PoESkillTree.Engine.Computation.Parsing;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils;
using PoESkillTree.SkillTreeFiles;

namespace PoESkillTree.Computation.Model
{
    public class ComputationObservables
    {
        private readonly IParser _parser;
        private readonly IScheduler _parsingScheduler;

        public ComputationObservables(IParser parser, IScheduler parsingScheduler)
            => (_parser, _parsingScheduler) = (parser, parsingScheduler);

        public IObservable<CalculatorUpdate> InitialParse(PassiveTreeDefinition passiveTreeDefinition, TimeSpan bufferTimeSpan)
        {
            var givenResultObservable = _parser.CreateGivenModifierParseDelegates().ToObservable()
                .ObserveOn(_parsingScheduler)
                .SelectMany(d => d());
            var passiveNodesObservable = passiveTreeDefinition.Nodes.ToObservable()
                .ObserveOn(_parsingScheduler)
                .SelectMany(n => _parser.ParsePassiveNode(n.Id).Modifiers);
            return givenResultObservable.Merge(passiveNodesObservable)
                .Buffer(bufferTimeSpan)
                .Where(ms => ms.Any())
                .Select(ms => new CalculatorUpdate(ms.ToList(), Array.Empty<Modifier>()));
        }

        public IObservable<CalculatorUpdate> ParseSkilledPassiveNodes(IEnumerable<SkillNode> skilledNodes)
            => ParseCollection(skilledNodes, ParseSkilledNode);

        public IObservable<CalculatorUpdate> ObserveSkilledPassiveNodes(
            INotifyCollectionChanged<SkillNode> skilledNodes)
            => ObserveCollection(skilledNodes, ParseSkilledNode);

        private IReadOnlyList<Modifier> ParseSkilledNode(SkillNode node)
            => _parser.ParseSkilledPassiveNode(node.Id).Modifiers;

        public IObservable<CalculatorUpdate> ParseItems(IEnumerable<(Item item, ItemSlot slot)> items)
        {
            var itemsBySlot = items.ToDictionary(t => t.slot, t => t.item);
            var itemsAndSlots = Enums.GetValues<ItemSlot>()
                .Where(s => itemsBySlot.ContainsKey(s))
                .Select(s => (itemsBySlot[s], s));
            return ParseCollection(itemsAndSlots, ParseItem);
        }

        public IObservable<CalculatorUpdate> ObserveItems(INotifyCollectionChanged<(Item item, ItemSlot slot)> items)
            => ObserveCollection(items, ParseItem);

        private IReadOnlyList<Modifier> ParseItem((Item item, ItemSlot slot) t) =>
            _parser.ParseItem(t.item, t.slot).Modifiers;

        public IObservable<CalculatorUpdate> ParseJewels(
            IEnumerable<(Item item, ItemSlot slot, ushort socket, JewelRadius radius)> jewels)
            => ParseCollection(jewels, ParseJewel);

        public IObservable<CalculatorUpdate> ObserveJewels(
            INotifyCollectionChanged<(Item item, ItemSlot slot, ushort socket, JewelRadius radius)> jewels)
            => ObserveCollection(jewels, ParseJewel);

        private IReadOnlyList<Modifier> ParseJewel((Item item, ItemSlot slot, ushort socket, JewelRadius radius) jewel)
        {
            var (item, slot, socket, radius) = jewel;
            return slot == ItemSlot.SkillTree
                ? _parser.ParseJewelSocketedInSkillTree(item, radius, socket).Modifiers
                : _parser.ParseJewelSocketedInItem(item, slot).Modifiers;
        }

        public IObservable<CalculatorUpdate> ParseSkills(IEnumerable<IReadOnlyList<Skill>> skills)
            => ParseCollection(skills, ParseSkills);

        public IObservable<CalculatorUpdate> ObserveSkills(INotifyCollectionChanged<IReadOnlyList<Skill>> skills)
            => ObserveCollection(skills, ParseSkills);

        private IReadOnlyList<Modifier> ParseSkills(IReadOnlyList<Skill> skills)
            => _parser.ParseSkills(skills).Modifiers;

        private IObservable<CalculatorUpdate> ParseCollection<T>(IEnumerable<T> collection, Func<T, IReadOnlyList<Modifier>> parse) =>
            AggregateModifiers(collection.ToObservable().ObserveOn(_parsingScheduler).SelectMany(parse));

        private static IObservable<CalculatorUpdate> AggregateModifiers(IObservable<Modifier> modifiers)
            => modifiers.Aggregate(Enumerable.Empty<Modifier>(), (ms, m) => ms.Append(m))
                .Select(ms => new CalculatorUpdate(ms.ToList(), new Modifier[0]))
                .Where(UpdateIsNotEmpty);

        private IObservable<CalculatorUpdate> ObserveCollection<T>(
            INotifyCollectionChanged<T> collection, Func<T, IReadOnlyList<Modifier>> parse)
            => ObserveCollection(collection, t => new CalculatorUpdate(parse(t), new Modifier[0]));

        private IObservable<CalculatorUpdate> ObserveCollection<T>(
            INotifyCollectionChanged<T> collection, Func<T, CalculatorUpdate> parse)
        {
            return Observable.FromEventPattern<CollectionChangedEventHandler<T>, CollectionChangedEventArgs<T>>(
                    h => collection.CollectionChanged += h,
                    h => collection.CollectionChanged -= h)
                .Select(p => p.EventArgs)
                .ObserveOn(_parsingScheduler)
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