using System.Windows.Media;

namespace PoESkillTree.Views
{
    public class AccentItemViewModel
    {
        public AccentItemViewModel(string name, Brush showcaseBrush)
        {
            Name = name;
            ShowcaseBrush = showcaseBrush;
        }

        public string Name { get; }
        public Brush ShowcaseBrush { get; }
    }
}