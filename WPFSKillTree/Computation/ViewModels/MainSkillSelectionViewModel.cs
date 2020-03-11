using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.ViewModels
{
    public class MainSkillSelectionViewModel : Notifier
    {
        private readonly SkillDefinitions _skillDefinitions;
        private readonly ConfigurationNodeViewModel _selectedSkillItemSlot;
        private readonly ConfigurationNodeViewModel _selectedSkillSocketIndex;
        private readonly ConfigurationNodeViewModel _selectedSkillSkillIndex;
        private readonly ConfigurationNodeViewModel _selectedSkillPart;

        private MainSkillViewModel _selectedSkill;

        public static MainSkillSelectionViewModel Create(
            SkillDefinitions skillDefinitions, IBuilderFactories builderFactories,
            CalculationNodeViewModelFactory nodeFactory,
            ObservableSet<IReadOnlyList<Skill>> skills)
        {
            var vm = new MainSkillSelectionViewModel(skillDefinitions, builderFactories, nodeFactory);
            vm.Initialize(skills);
            return vm;
        }

#pragma warning disable CS8618 // SelectedSkill is initialized in Initialize and can't be set to null.
        private MainSkillSelectionViewModel(
#pragma warning restore
            SkillDefinitions skillDefinitions, IBuilderFactories builderFactories,
            CalculationNodeViewModelFactory nodeFactory)
        {
            _skillDefinitions = skillDefinitions;
            var selectedSkillItemSlotStat = builderFactories.MetaStatBuilders.MainSkillItemSlot
                .BuildToStats(Entity.Character).Single();
            _selectedSkillItemSlot = nodeFactory.CreateConfiguration(selectedSkillItemSlotStat, new NodeValue((int) Skill.Default.ItemSlot));
            var selectedSkillSocketIndexStat = builderFactories.MetaStatBuilders.MainSkillSocketIndex
                .BuildToStats(Entity.Character).Single();
            _selectedSkillSocketIndex = nodeFactory.CreateConfiguration(selectedSkillSocketIndexStat, new NodeValue(Skill.Default.SocketIndex));
            var selectedSkillSkillIndexStat = builderFactories.MetaStatBuilders.MainSkillSkillIndex
                .BuildToStats(Entity.Character).Single();
            _selectedSkillSkillIndex = nodeFactory.CreateConfiguration(selectedSkillSkillIndexStat, new NodeValue(Skill.Default.SkillIndex));
            var selectedSkillPartStat = builderFactories.StatBuilders.MainSkillPart
                .BuildToStats(Entity.Character).Single();
            _selectedSkillPart = nodeFactory.CreateConfiguration(selectedSkillPartStat, new NodeValue(0));
            ConfigurationNodes = new[] { _selectedSkillItemSlot, _selectedSkillSocketIndex, _selectedSkillSkillIndex, _selectedSkillPart };
        }

        private void Initialize(ObservableSet<IReadOnlyList<Skill>> skills)
        {
            AddSkills(skills);
            SelectedSkill = GetSelectedAndAvailableSkill() ?? AvailableSkills.First();
            _selectedSkillItemSlot.PropertyChanged += OnSelectedSkillStatChanged;
            _selectedSkillSocketIndex.PropertyChanged += OnSelectedSkillStatChanged;
            _selectedSkillSkillIndex.PropertyChanged += OnSelectedSkillStatChanged;
            skills.CollectionChanged += OnSkillsChanged;
        }

        public IEnumerable<ConfigurationNodeViewModel> ConfigurationNodes { get; }

        public ObservableCollection<MainSkillViewModel> AvailableSkills { get; } =
            new ObservableCollection<MainSkillViewModel>();

        [AllowNull]
        public MainSkillViewModel SelectedSkill
        {
            get => _selectedSkill;
            set
            {
                if (value == null)
                    return;
                _selectedSkillItemSlot.NumericValue = (double?) value.Skill.ItemSlot;
                _selectedSkillSocketIndex.NumericValue = value.Skill.SocketIndex;
                _selectedSkillSkillIndex.NumericValue = value.Skill.SkillIndex;
                SetProperty(ref _selectedSkill, value);
            }
        }

        private MainSkillViewModel? GetSelectedAndAvailableSkill()
            => AvailableSkills.FirstOrDefault(
                s => s.Skill.ItemSlot == (ItemSlot?) _selectedSkillItemSlot.NumericValue &&
                     s.Skill.SocketIndex == (int?) _selectedSkillSocketIndex.NumericValue &&
                     s.Skill.SkillIndex == (int?) _selectedSkillSkillIndex.NumericValue);

        private void OnSelectedSkillStatChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(CalculationNodeViewModel.Value))
            {
                SelectedSkill = GetSelectedAndAvailableSkill();
            }
        }

        private void OnSkillsChanged(object sender, CollectionChangedEventArgs<IReadOnlyList<Skill>> args)
        {
            AddSkills(args.AddedItems);
            RemoveSkills(args.RemovedItems);
            SelectedSkill = GetSelectedAndAvailableSkill() ?? AvailableSkills.First();
        }

        private void AddSkills(IEnumerable<IEnumerable<Skill>> skills)
        {
            foreach (var skill in skills.Flatten().Where(IsActiveSkill))
            {
                AddSkill(CreateSkillViewModel(skill));
            }
        }

        private void RemoveSkills(IEnumerable<IEnumerable<Skill>> skills)
        {
            foreach (var skill in skills.Flatten().Where(IsActiveSkill))
            {
                RemoveSkill(skill);
            }
        }

        private bool IsActiveSkill(Skill skill)
            => skill.IsEnabled && (skill.Gem is null || skill.Gem.IsEnabled) && !_skillDefinitions.GetSkillById(skill.Id).IsSupport;

        private void AddSkill(MainSkillViewModel skill)
            => AvailableSkills.Add(skill);

        private void RemoveSkill(Skill skill)
        {
            var vm = AvailableSkills.FirstOrDefault(x => x.Skill == skill);
            if (vm is null)
                return;

            AvailableSkills.Remove(vm);
        }

        private MainSkillViewModel CreateSkillViewModel(Skill skill)
        {
            var definition = _skillDefinitions.GetSkillById(skill.Id);
            return new MainSkillViewModel(definition, skill, _selectedSkillPart);
        }
    }
}