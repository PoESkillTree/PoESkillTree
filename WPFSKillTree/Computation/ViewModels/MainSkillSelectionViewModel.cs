using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils.Extensions;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class MainSkillSelectionViewModel : Notifier
    {
        private readonly SkillDefinitions _skillDefinitions;
        private MainSkillViewModel _selectedSkill;
        private uint _skillStage;
        private uint _maximumSkillStage;

        public MainSkillSelectionViewModel(SkillDefinitions skillDefinitions)
            => _skillDefinitions = skillDefinitions;

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
            AddSkills(skillCollection);
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
            SelectedSkill = null;
            AvailableSkills.Clear();
            AddSkills(skills);
        }

        private void AddSkills(IEnumerable<IEnumerable<Skill>> skills)
        {
            foreach (var skill in skills.Flatten().Where(IsActiveSkill))
            {
                AddSkill(skill);
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

        private void AddSkill(Skill skill)
        {
            AvailableSkills.Add(new MainSkillViewModel(_skillDefinitions.GetSkillById(skill.Id), skill));
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

            if (SelectedSkill == vm)
            {
                SelectedSkill = AvailableSkills.FirstOrDefault(x => x != vm);
            }
            AvailableSkills.Remove(vm);
        }
    }
}