using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.ViewModels
{
    public class ModifierNodeViewModel
    {
        public ModifierNodeViewModel(Form form, ModifierSource modifierSource, ResultNodeViewModel node)
        {
            Form = form;
            ModifierSource = modifierSource;
            Node = node;
        }

        public Form Form { get; }
        public ModifierSource ModifierSource { get; }
        public bool IsLocal => ModifierSource is ModifierSource.Local;
        public ResultNodeViewModel Node { get; }
    }
}