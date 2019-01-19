using PoESkillTree.GameModel.Skills;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class MainSkillViewModel : Notifier
    {
        private int _selectedSkillPart;

        public MainSkillViewModel(
            SkillDefinition skillDefinition, Skill skill, CalculationNodeViewModel skillIsMainNode)
            => (SkillDefinition, Skill, SkillIsMainNode) = (skillDefinition, skill, skillIsMainNode);

        public Skill Skill { get; }
        public SkillDefinition SkillDefinition { get; }
        public bool HasSkillParts => SkillDefinition.PartNames.Count > 1;

        public CalculationNodeViewModel SkillIsMainNode { get; }

        public int SelectedSkillPart
        {
            get => _selectedSkillPart;
            set => SetProperty(ref _selectedSkillPart, value);
        }
    }
}