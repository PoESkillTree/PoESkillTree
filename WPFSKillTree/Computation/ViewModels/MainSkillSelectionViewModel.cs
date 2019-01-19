using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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
        private readonly IMetaStatBuilders _statBuilders;
        private readonly CalculationNodeViewModelFactory _nodeFactory;
        private readonly MainSkillViewModel _defaultSkill;

        private MainSkillViewModel _selectedSkill;
        private uint _skillStage;
        private uint _maximumSkillStage;

        public static MainSkillSelectionViewModel Create(
            SkillDefinitions skillDefinitions, IMetaStatBuilders statBuilders,
            CalculationNodeViewModelFactory nodeFactory,
            ObservableCollection<IReadOnlyList<Skill>> skills)
        {
            var vm = new MainSkillSelectionViewModel(skillDefinitions, statBuilders, nodeFactory);
            vm.Initialize(skills);
            return vm;
        }

        private MainSkillSelectionViewModel(
            SkillDefinitions skillDefinitions, IMetaStatBuilders statBuilders,
            CalculationNodeViewModelFactory nodeFactory)
        {
            _skillDefinitions = skillDefinitions;
            _statBuilders = statBuilders;
            _nodeFactory = nodeFactory;
            _defaultSkill = CreateSkillViewModel(
                new Skill(DefaultSkillId, 1, 0, ItemSlot.Unequipable, 0, null));
        }

        private void Initialize(ObservableCollection<IReadOnlyList<Skill>> skills)
        {
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
            newValue.SkillIsMainNode.BoolValue = true;
        }

        public uint SkillStage
        {
            get => _skillStage;
            set => SetProperty(ref _skillStage, value);
        }

        public uint MaximumSkillStage
        {
            get => _maximumSkillStage;
            set => SetProperty(ref _maximumSkillStage, value);
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
            var stat = _statBuilders.SkillIsMain(skill.ItemSlot, skill.SocketIndex).BuildToStats(Entity.Character)
                .Single();
            var node = _nodeFactory.CreateConfiguration(stat);
            return new MainSkillViewModel(definition, skill, node);
        }
    }
}