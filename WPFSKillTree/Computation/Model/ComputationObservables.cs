using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EnumsNET;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Core;
using PoESkillTree.Engine.Computation.Parsing;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Model
{
    public class ComputationObservables
    {
        private readonly IParser _parser;
        private readonly IScheduler _parsingScheduler;
        private readonly IScheduler _calculationScheduler;

        public ComputationObservables(IParser parser, IScheduler parsingScheduler, IScheduler calculationScheduler)
        {
            _parser = parser;
            _parsingScheduler = parsingScheduler;
            _calculationScheduler = calculationScheduler;
        }

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

        public Task<CalculatorUpdate> ParseSkilledPassiveNodesAsync(IEnumerable<SkillNode> skilledNodes) =>
            _parsingScheduler.ScheduleAsync(() => ParseCollection(skilledNodes, ParseSkilledNode));

        public IObservable<CalculatorUpdate> ObserveSkilledPassiveNodes(
            INotifyCollectionChanged<SkillNode> skilledNodes)
            => ObserveCollection(skilledNodes, ParseSkilledNode, _parsingScheduler);

        private IReadOnlyList<Modifier> ParseSkilledNode(SkillNode node)
            => _parser.ParseSkilledPassiveNode(node.Id).Modifiers;

        public Task<CalculatorUpdate> ParseItemsAsync(IEnumerable<(Item item, ItemSlot slot)> items)
        {
            var itemsBySlot = items.ToDictionary(t => t.slot, t => t.item);
            var itemsAndSlots = Enums.GetValues<ItemSlot>()
                .Where(s => itemsBySlot.ContainsKey(s))
                .Select(s => (itemsBySlot[s], s));
            return _parsingScheduler.ScheduleAsync(() => ParseCollection(itemsAndSlots, ParseItem));
        }

        public IObservable<CalculatorUpdate> ObserveItems(INotifyCollectionChanged<(Item item, ItemSlot slot)> items)
            => ObserveCollection(items, ParseItem, _parsingScheduler);

        private IReadOnlyList<Modifier> ParseItem((Item item, ItemSlot slot) t) =>
            _parser.ParseItem(t.item, t.slot).Modifiers;

        public Task<CalculatorUpdate> ParseJewelsAsync(
            IEnumerable<(Item item, ItemSlot slot, ushort socket, JewelRadius radius)> jewels) =>
            _parsingScheduler.ScheduleAsync(() => ParseCollection(jewels, ParseJewel));

        public IObservable<CalculatorUpdate> ObserveJewels(
            INotifyCollectionChanged<(Item item, ItemSlot slot, ushort socket, JewelRadius radius)> jewels)
            => ObserveCollection(jewels, ParseJewel, _parsingScheduler);

        private IReadOnlyList<Modifier> ParseJewel((Item item, ItemSlot slot, ushort socket, JewelRadius radius) jewel)
        {
            var (item, slot, socket, radius) = jewel;
            return slot == ItemSlot.SkillTree
                ? _parser.ParseJewelSocketedInSkillTree(item, radius, socket).Modifiers
                : _parser.ParseJewelSocketedInItem(item, slot).Modifiers;
        }

        public Task<(CalculatorUpdate update, IReadOnlyList<IReadOnlyList<Skill>> skills)> ParseGemsAsync(IEnumerable<IReadOnlyList<Gem>> gems) =>
            _parsingScheduler.ScheduleAsync(() => ParseGems(gems));

        public (IObservable<CalculatorUpdate> updateObservable, IObservable<IReadOnlyList<IReadOnlyList<Skill>>> skillsObservable) ObserveGems(
            INotifyCollectionChanged<IReadOnlyList<Gem>> gems)
        {
            var updateAndSkillsObservable = CreateObservableFromCollection(gems)
                .ObserveOn(_parsingScheduler)
                .Select(args =>
                {
                    var (addUpdate, skills) = ParseGems(args.AddedItems);
                    var (removeUpdate, _) = ParseGems(args.RemovedItems);
                    var update = CalculatorUpdate.Accumulate(addUpdate, removeUpdate.Invert());
                    return (update, skills);
                })
                .Publish();
            var updateObservable = updateAndSkillsObservable
                .Select(t => t.update)
                .Where(UpdateIsNotEmpty);
            var skillsObservable = updateAndSkillsObservable
                .Select(t => t.skills);
            updateAndSkillsObservable.Connect();
            return (updateObservable, skillsObservable);
        }

        private (CalculatorUpdate, IReadOnlyList<IReadOnlyList<Skill>>) ParseGems(IEnumerable<IReadOnlyList<Gem>> gems)
        {
            var modifiersAndSkills = gems.Select(ParseGems).ToList();
            var modifiers = modifiersAndSkills.SelectMany(t => t.modifiers).ToList();
            var calculatorUpdate = new CalculatorUpdate(modifiers, Array.Empty<Modifier>());
            var skills = modifiersAndSkills.Select(t => t.skills).ToList();
            return (calculatorUpdate, skills);
        }

        private (IReadOnlyList<Modifier> modifiers, IReadOnlyList<Skill> skills) ParseGems(IReadOnlyList<Gem> gems)
        {
            var (result, skills) = _parser.ParseGems(gems);
            return (result.Modifiers, skills);
        }

        public Task<CalculatorUpdate> ParseSkillsAsync(IEnumerable<IReadOnlyList<Skill>> skills) =>
            _calculationScheduler.ScheduleAsync(() => ParseCollection(skills, ParseSkills));

        public IObservable<CalculatorUpdate> ObserveSkills(INotifyCollectionChanged<IReadOnlyList<Skill>> skills) =>
            ObserveCollection(skills, ParseSkills, _calculationScheduler);

        private IReadOnlyList<Modifier> ParseSkills(IReadOnlyList<Skill> skills)
            => _parser.ParseSkills(skills).Modifiers;

        private static CalculatorUpdate ParseCollection<T>(IEnumerable<T> collection, Func<T, IReadOnlyList<Modifier>> parse)
        {
            var modifiers = collection.SelectMany(parse).ToList();
            return new CalculatorUpdate(modifiers, Array.Empty<Modifier>());
        }

        private static IObservable<CalculatorUpdate> ObserveCollection<T>(
            INotifyCollectionChanged<T> collection, Func<T, IReadOnlyList<Modifier>> parse, IScheduler scheduler)
        {
            return CreateObservableFromCollection(collection)
                .ObserveOn(scheduler)
                .Select(args =>
                {
                    var addedModifiers = args.AddedItems.SelectMany(parse).ToList();
                    var removedModifiers = args.RemovedItems.SelectMany(parse).ToList();
                    return new CalculatorUpdate(addedModifiers, removedModifiers);
                })
                .Where(UpdateIsNotEmpty);
        }

        private static IObservable<CollectionChangedEventArgs<T>> CreateObservableFromCollection<T>(
            INotifyCollectionChanged<T> collection)
        {
            return Observable.FromEventPattern<CollectionChangedEventHandler<T>, CollectionChangedEventArgs<T>>(
                    h => collection.CollectionChanged += h,
                    h => collection.CollectionChanged -= h)
                .Select(p => p.EventArgs);
        }

        private static bool UpdateIsNotEmpty(CalculatorUpdate update)
            => update.AddedModifiers.Any() || update.RemovedModifiers.Any();
    }
}