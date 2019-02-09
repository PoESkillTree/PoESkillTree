using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using EnumsNET;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.PassiveTree;
using PoESkillTree.GameModel.Skills;
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
            PassiveTreeDefinition passiveTreeDefinition, TimeSpan bufferTimeSpan)
        {
            var givenResultObservable = _parser.CreateGivenModifierParseDelegates().ToObservable()
                .Select(d => d());
            var passiveNodesObservable = passiveTreeDefinition.Nodes.ToObservable()
                .Select(n => _parser.ParsePassiveNode(n.Id).Modifiers);
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
            => ObserveCollection<SkillNode>(skilledNodes, n => ParseSkilledNode(n.Id));

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

        public IObservable<CalculatorUpdate> ObserveItems(ObservableCollection<(Item, ItemSlot)> items)
            => ObserveCollection<(Item item, ItemSlot slot)>(items, t => _parser.ParseItem(t.item, t.slot).Modifiers);

        public IObservable<CalculatorUpdate> ParseSkills(IEnumerable<IReadOnlyList<Skill>> skills)
            => AggregateModifiers(skills.ToObservable().SelectMany(ParseSkills));

        public IObservable<CalculatorUpdate> ObserveSkills(ObservableCollection<IReadOnlyList<Skill>> skills)
            => ObserveCollection<IReadOnlyList<Skill>>(skills, ParseSkills);

        private IReadOnlyList<Modifier> ParseSkills(IReadOnlyList<Skill> skills)
            => _parser.ParseSkills(skills).Modifiers;

        private static IObservable<CalculatorUpdate> AggregateModifiers(IObservable<Modifier> modifiers)
            => modifiers.Aggregate(Enumerable.Empty<Modifier>(), (ms, m) => ms.Append(m))
                .Select(ms => new CalculatorUpdate(ms.ToList(), new Modifier[0]))
                .Where(UpdateIsNotEmpty);

        private static IObservable<CalculatorUpdate> ObserveCollection<T>(
            INotifyCollectionChanged collection, Func<T, IReadOnlyList<Modifier>> parse)
            => ObserveCollection<T>(collection, t => new CalculatorUpdate(parse(t), new Modifier[0]));

        private static IObservable<CalculatorUpdate> ObserveCollection<T>(
            INotifyCollectionChanged collection, Func<T, CalculatorUpdate> parse)
        {
            return Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => collection.CollectionChanged += h,
                    h => collection.CollectionChanged -= h)
                .Select(p =>
                {
                    if (p.EventArgs.Action == NotifyCollectionChangedAction.Reset)
                        throw new NotSupportedException("Reset action is not supported");
                    return p.EventArgs;
                })
                .Select(args =>
                {
                    var addUpdate = Parse(args.NewItems);
                    var removeUpdate = Parse(args.OldItems).Invert();
                    return CalculatorUpdate.Accumulate(addUpdate, removeUpdate);
                })
                .Where(UpdateIsNotEmpty);

            CalculatorUpdate Parse(IEnumerable changedItems)
            {
                if (changedItems is null)
                    return CalculatorUpdate.Empty;
                return changedItems.Cast<T>().Select(parse).Aggregate(CalculatorUpdate.Accumulate);
            }
        }

        private static bool UpdateIsNotEmpty(CalculatorUpdate update)
            => update.AddedModifiers.Any() || update.RemovedModifiers.Any();
    }
}