using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils.Extensions;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class MainSkillSelectionViewModel : Notifier
    {
        private const string DefaultSkillId = "PlayerMelee";

        private readonly SkillDefinitions _skillDefinitions;
        private readonly IBuilderFactories _builderFactories;
        private readonly CalculationNodeViewModelFactory _nodeFactory;
        private readonly MainSkillViewModel _defaultSkill;
        private readonly CalculationNodeViewModel _selectedSkillPart;

        private MainSkillViewModel _selectedSkill;

        public static MainSkillSelectionViewModel Create(
            SkillDefinitions skillDefinitions, IBuilderFactories builderFactories,
            CalculationNodeViewModelFactory nodeFactory,
            ObservableCollection<IReadOnlyList<Skill>> skills)
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
            _builderFactories = builderFactories;
            _nodeFactory = nodeFactory;
            _defaultSkill = CreateSkillViewModel(
                new Skill(DefaultSkillId, 1, 0, ItemSlot.Unequipable, 0, null));
            var selectedSkillPartStat = _builderFactories.StatBuilders.MainSkillPart
                .BuildToStats(Entity.Character).Single();
            _selectedSkillPart = nodeFactory.CreateConfiguration(selectedSkillPartStat);
        }

        private void Initialize(ObservableCollection<IReadOnlyList<Skill>> skills)
        {
            _selectedSkillPart.NumericValue = 0;
            ResetSkills(skills);
            skills.CollectionChanged += OnSkillsChanged;
        }

        public ObservableCollection<MainSkillViewModel> AvailableSkills { get; } =
            new ObservableCollection<MainSkillViewModel>();

        public MainSkillViewModel SelectedSkill
        {
            get => _selectedSkill;
            set => SetProperty(ref _selectedSkill, value, onChanging: OnSelectedSkillChanging);
        }

        private void OnSelectedSkillChanging(MainSkillViewModel newValue)
        {
            if (SelectedSkill != null)
            {
                SelectedSkill.SkillIsMainNode.BoolValue = false;
            }
            if (newValue != null)
            {
                newValue.SkillIsMainNode.BoolValue = true;
            }
        }

        private void OnSkillsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                ResetSkills((IEnumerable<IEnumerable<Skill>>) sender);
                return;
            }

            if (args.NewItems != null)
            {
                AddSkills(args.NewItems.Cast<IEnumerable<Skill>>());
            }
            if (args.OldItems != null)
            {
                RemoveSkills(args.OldItems.Cast<IEnumerable<Skill>>());
            }
        }

        private void ResetSkills(IEnumerable<IEnumerable<Skill>> skills)
        {
            AvailableSkills.Clear();
            AddSkill(_defaultSkill);
            AddSkills(skills);
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
            => !_skillDefinitions.GetSkillById(skill.Id).IsSupport;

        private void AddSkill(MainSkillViewModel skill)
        {
            if (skill != _defaultSkill)
            {
                AvailableSkills.Remove(_defaultSkill);
            }
            AvailableSkills.Add(skill);
            if (AvailableSkills.Count == 1)
            {
                SelectedSkill = AvailableSkills[0];
            }
        }

        private void RemoveSkill(Skill skill)
        {
            var vm = AvailableSkills.FirstOrDefault(x => x.Skill == skill);
            if (vm is null)
                return;

            var wasSelected = SelectedSkill == vm;
            AvailableSkills.Remove(vm);

            if (AvailableSkills.IsEmpty())
            {
                AddSkill(_defaultSkill);
            }
            else if (wasSelected)
            {
                SelectedSkill = AvailableSkills[0];
            }
        }

        private MainSkillViewModel CreateSkillViewModel(Skill skill)
        {
            var definition = _skillDefinitions.GetSkillById(skill.Id);
            var skillIsMainStat = _builderFactories.MetaStatBuilders.SkillIsMain(skill.ItemSlot, skill.SocketIndex)
                .BuildToStats(Entity.Character).Single();
            var skillIsMainNode = _nodeFactory.CreateConfiguration(skillIsMainStat);
            return new MainSkillViewModel(definition, skill, skillIsMainNode, _selectedSkillPart);
        }
    }
}