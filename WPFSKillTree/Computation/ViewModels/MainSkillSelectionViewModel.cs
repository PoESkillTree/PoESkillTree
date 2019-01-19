using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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

        private readonly MainSkillViewModel _defaultSkill;

        private MainSkillViewModel _selectedSkill;
        private uint _skillStage;
        private uint _maximumSkillStage;

        public MainSkillSelectionViewModel(SkillDefinitions skillDefinitions)
        {
            _skillDefinitions = skillDefinitions;
            _defaultSkill = new MainSkillViewModel(
                _skillDefinitions.GetSkillById(DefaultSkillId),
                new Skill(DefaultSkillId, 1, 0, ItemSlot.Unequipable, 0, null));
        }

        public ObservableCollection<MainSkillViewModel> AvailableSkills { get; } =
            new ObservableCollection<MainSkillViewModel>();

        public MainSkillViewModel SelectedSkill
        {
            get => _selectedSkill;
            set => SetProperty(ref _selectedSkill, value);
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

        public void Observe(ObservableCollection<IReadOnlyList<Skill>> skillCollection)
        {
            ResetSkills(skillCollection);
            skillCollection.CollectionChanged += OnSkillsChanged;
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
                AddSkill(new MainSkillViewModel(_skillDefinitions.GetSkillById(skill.Id), skill));
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
    }
}