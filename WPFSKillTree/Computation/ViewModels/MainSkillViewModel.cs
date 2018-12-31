using System.Collections.Generic;
using PoESkillTree.GameModel.Skills;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class MainSkillViewModel : Notifier
    {
        private readonly SkillDefinition _skillDefinition;
        private readonly Skill _skill;
        private int _selectedSkillPart;

        public MainSkillViewModel(SkillDefinition skillDefinition, Skill skill)
        {
            (_skillDefinition, _skill) = (skillDefinition, skill);
        }

        public string Name => _skillDefinition.ActiveSkill.DisplayName;
        public IReadOnlyList<string> SkillParts => _skillDefinition.PartNames;
        public bool HasSkillParts => SkillParts.Count > 1;

        public int SelectedSkillPart
        {
            get => _selectedSkillPart;
            set => SetProperty(ref _selectedSkillPart, value);
        }
    }
}