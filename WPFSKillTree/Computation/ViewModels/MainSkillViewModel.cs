using PoESkillTree.GameModel.Skills;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class MainSkillViewModel : Notifier
    {
        public MainSkillViewModel(
            SkillDefinition skillDefinition, Skill skill, CalculationNodeViewModel skillIsMainNode,
            CalculationNodeViewModel selectedSkillPart)
            => (SkillDefinition, Skill, SkillIsMainNode, SelectedSkillPart) =
                (skillDefinition, skill, skillIsMainNode, selectedSkillPart);

        public Skill Skill { get; }
        public SkillDefinition SkillDefinition { get; }
        public bool HasSkillParts => SkillDefinition.PartNames.Count > 1;

        public CalculationNodeViewModel SkillIsMainNode { get; }
        public CalculationNodeViewModel SelectedSkillPart { get; }
    }
}