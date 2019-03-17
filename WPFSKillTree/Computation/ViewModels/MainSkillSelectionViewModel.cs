using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils;
using PoESkillTree.Utils.Extensions;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.ViewModels
{
    public class MainSkillSelectionViewModel : Notifier
    {
        private readonly SkillDefinitions _skillDefinitions;
        private readonly ConfigurationNodeViewModel _selectedSkillItemSlot;
        private readonly ConfigurationNodeViewModel _selectedSkillSocketIndex;
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

        private MainSkillSelectionViewModel(
            SkillDefinitions skillDefinitions, IBuilderFactories builderFactories,
            CalculationNodeViewModelFactory nodeFactory)
        {
            _skillDefinitions = skillDefinitions;
            var selectedSkillItemSlotStat = builderFactories.MetaStatBuilders.MainSkillItemSlot
                .BuildToStats(Entity.Character).Single();
            _selectedSkillItemSlot = nodeFactory.CreateConfiguration(selectedSkillItemSlotStat, new NodeValue(0));
            var selectedSkillSocketIndexStat = builderFactories.MetaStatBuilders.MainSkillSocketIndex
                .BuildToStats(Entity.Character).Single();
            _selectedSkillSocketIndex = nodeFactory.CreateConfiguration(selectedSkillSocketIndexStat, new NodeValue(0));
            var selectedSkillPartStat = builderFactories.StatBuilders.MainSkillPart
                .BuildToStats(Entity.Character).Single();
            _selectedSkillPart = nodeFactory.CreateConfiguration(selectedSkillPartStat, new NodeValue(0));
            ConfigurationNodes = new[] { _selectedSkillItemSlot, _selectedSkillSocketIndex, _selectedSkillPart };
        }

        private void Initialize(ObservableSet<IReadOnlyList<Skill>> skills)
        {
            AddSkills(skills);
            SelectedSkill = GetSelectedAndAvailableSkill() ?? AvailableSkills.First();
            _selectedSkillItemSlot.PropertyChanged += OnSelectedSkillStatChanged;
            _selectedSkillSocketIndex.PropertyChanged += OnSelectedSkillStatChanged;
            skills.CollectionChanged += OnSkillsChanged;
        }

        public IEnumerable<ConfigurationNodeViewModel> ConfigurationNodes { get; }

        public ObservableCollection<MainSkillViewModel> AvailableSkills { get; } =
            new ObservableCollection<MainSkillViewModel>();

        public MainSkillViewModel SelectedSkill
        {
            get => _selectedSkill;
            set
            {
                if (value == null)
                    return;
                _selectedSkillItemSlot.NumericValue = (double?) value.Skill.ItemSlot;
                _selectedSkillSocketIndex.NumericValue = value.Skill.SocketIndex;
                SetProperty(ref _selectedSkill, value);
            }
        }

        [CanBeNull]
        private MainSkillViewModel GetSelectedAndAvailableSkill()
            => AvailableSkills.FirstOrDefault(
                s => s.Skill.ItemSlot == (ItemSlot?) _selectedSkillItemSlot.NumericValue &&
                     s.Skill.SocketIndex == (int?) _selectedSkillSocketIndex.NumericValue);

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
            => skill.IsEnabled && !_skillDefinitions.GetSkillById(skill.Id).IsSupport;

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