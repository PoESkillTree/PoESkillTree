using System.Windows.Media;
using PoESkillTree.Common.ViewModels;

namespace PoESkillTree.ViewModels.Equipment
{
    public enum TabPickerResult
    {
        Affirmative,
        Negative,
        Delete,
        DeleteIncludingItems
    }

    public class TabPickerViewModel : CloseableViewModel<TabPickerResult>
    {
        private bool _isDeletable = true;
        public bool IsDeletable
        {
            get => _isDeletable;
            set => SetProperty(ref _isDeletable, value);
        }

        private string _name = "";
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private Color _color = Color.FromRgb(98, 128, 0);
        public Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }
    }
}