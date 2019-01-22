using PoESkillTree.GameModel.Skills;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class MainSkillViewModel : Notifier
    {
        public MainSkillViewModel(
            SkillDefinition skillDefinition, Skill skill, ConfigurationNodeViewModel skillIsMainNode,
            ConfigurationNodeViewModel selectedSkillPart)
            => (SkillDefinition, Skill, SkillIsMainNode, SelectedSkillPart) =
                (skillDefinition, skill, skillIsMainNode, selectedSkillPart);

        public Skill Skill { get; }
        public SkillDefinition SkillDefinition { get; }
        public bool HasSkillParts => SkillDefinition.PartNames.Count > 1;

        public ConfigurationNodeViewModel SkillIsMainNode { get; }
        public ConfigurationNodeViewModel SelectedSkillPart { get; }
    }
}