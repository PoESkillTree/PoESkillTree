using PoESkillTree.GameModel.Skills;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class MainSkillViewModel : Notifier
    {
        private int _selectedSkillPart;

        public MainSkillViewModel(SkillDefinition skillDefinition, Skill skill)
            => (SkillDefinition, Skill) = (skillDefinition, skill);

        public Skill Skill { get; }
        public SkillDefinition SkillDefinition { get; }
        public bool HasSkillParts => SkillDefinition.PartNames.Count > 1;

        public int SelectedSkillPart
        {
            get => _selectedSkillPart;
            set => SetProperty(ref _selectedSkillPart, value);
        }
    }
}