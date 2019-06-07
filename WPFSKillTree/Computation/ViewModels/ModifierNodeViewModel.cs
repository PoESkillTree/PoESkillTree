using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.ViewModels
{
    public class ModifierNodeViewModel
    {
        public ModifierNodeViewModel(Form form, ModifierSource modifierSource, CalculationNodeViewModel node)
        {
            Form = form;
            ModifierSource = modifierSource;
            Node = node;
        }

        public Form Form { get; }
        public ModifierSource ModifierSource { get; }
        public bool IsLocal => ModifierSource is ModifierSource.Local;
        public CalculationNodeViewModel Node { get; }
    }
}