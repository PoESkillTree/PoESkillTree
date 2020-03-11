using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Model;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Model.Items;
using PoESkillTree.Utils;

namespace PoESkillTree.Model
{
    public class ObservableSkillCollection
    {
        private SkillEnabler? _skillEnabler;
        private ObservableSet<IReadOnlyList<Skill>>? _itemAttributesSkills;

        private readonly ObservableSet<IReadOnlyList<Gem>> _gems;
        private readonly AdditionalSkillStatApplier _additionalSkillStatApplier;

        public ObservableSet<IReadOnlyList<Skill>> Collection { get; } = new ObservableSet<IReadOnlyList<Skill>>();

        public ObservableSkillCollection(ObservableSet<IReadOnlyList<Gem>> gems, AdditionalSkillStatApplier additionalSkillStatApplier)
        {
            _gems = gems;
            _additionalSkillStatApplier = additionalSkillStatApplier;
            _additionalSkillStatApplier.StatChangedForSlot += AdditionalSkillStatApplierOnStatChangedForSlot;
            Collection.CollectionChanged += OnCollectionChanged;
        }

        public void ConnectTo(ItemAttributes itemAttributes)
        {
            if (_skillEnabler != null)
                _skillEnabler.EnabledChangedForSlots -= SkillEnablerOnEnabledChangedForSlots;
            _skillEnabler = itemAttributes.SkillEnabler;
            _itemAttributesSkills = itemAttributes.Skills;

            Collection.ResetTo(GetDefaultSkills().Select(Update));
            _itemAttributesSkills.ResetTo(Collection);

            _skillEnabler.EnabledChangedForSlots += SkillEnablerOnEnabledChangedForSlots;
        }

        public void InitializeSkillsFromGems(IEnumerable<IReadOnlyList<Skill>> skills)
        {
            Collection.ResetTo(skills.Concat(GetDefaultSkills()).Select(Update));
        }

        public void UpdateSkillsFromGems(IReadOnlyList<IReadOnlyList<Skill>> toAdd)
        {
            var slotsWithGemsOrItemSkills = _gems.Select(gs => gs.First().ItemSlot).Append(Skill.Default.ItemSlot).ToHashSet();
            var changedSlots = toAdd.Select(ss => ss.First().ItemSlot);
            var toRemove = Collection
                .Where(ss => !slotsWithGemsOrItemSkills.Contains(ss.First().ItemSlot) || changedSlots.Contains(ss.First().ItemSlot))
                .ToList();
            Collection.ExceptAndUnionWith(toRemove, toAdd.Select(Update));
        }

        private static IEnumerable<IReadOnlyList<Skill>> GetDefaultSkills() =>
            new[] {new[] {Skill.Default}};

        private IReadOnlyList<Skill> Update(IEnumerable<Skill> skills) =>
            skills.Select(Update).ToList();

        private Skill Update(Skill skill)
        {
            if (_skillEnabler != null)
            {
                skill = _skillEnabler.Apply(skill);
            }
            return _additionalSkillStatApplier.Apply(skill);
        }

        private void OnCollectionChanged(
            object sender, CollectionChangedEventArgs<IReadOnlyList<Skill>> args)
        {
            _additionalSkillStatApplier.CleanSubscriptions(args.RemovedItems, args.AddedItems);
            _itemAttributesSkills?.ExceptAndUnionWith(args.RemovedItems, args.AddedItems);
        }

        private void SkillEnablerOnEnabledChangedForSlots(object? sender, IReadOnlyCollection<ItemSlot> changedSlots)
        {
            var toRemove = Collection.Where(ss => changedSlots.Contains(ss.First().ItemSlot)).ToList();
            var toAdd = toRemove.Select(Update);
            Collection.ExceptAndUnionWith(toRemove, toAdd);
        }

        private void AdditionalSkillStatApplierOnStatChangedForSlot(object? sender, ItemSlot e)
        {
            var toRemove = Collection.Where(ss => ss.First().ItemSlot == e).ToList();
            var toAdd = toRemove.Select(Update);
            Collection.ExceptAndUnionWith(toRemove, toAdd);
        }
    }
}