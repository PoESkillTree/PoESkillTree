using PoESkillTree.GameModel.Skills;
using POESKillTree.Utils;

namespace POESKillTree.Computation.ViewModels
{
    public class MainSkillViewModel : Notifier
    {
        public MainSkillViewModel(
            SkillDefinition skillDefinition, Skill skill, ConfigurationNodeViewModel selectedSkillPart)
            => (SkillDefinition, Skill, SelectedSkillPart) =
                (skillDefinition, skill, selectedSkillPart);

        public Skill Skill { get; }
        public SkillDefinition SkillDefinition { get; }
        public bool HasSkillParts => SkillDefinition.PartNames.Count > 1;

        public ConfigurationNodeViewModel SelectedSkillPart { get; }
    }
}