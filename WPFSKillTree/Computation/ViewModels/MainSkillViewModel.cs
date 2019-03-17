using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.ViewModels
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