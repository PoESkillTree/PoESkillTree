using System.Windows;
using POESKillTree.TreeGenerator.ViewModels;

namespace POESKillTree.TreeGenerator.Views
{
    /// <summary>
    /// Interaction logic for ControllerWindow.xaml
    /// </summary>
    public partial class ControllerWindow
    {
        private readonly ControllerViewModel _vm;

        public ControllerWindow(ControllerViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = vm;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _vm.WindowLoaded();
        }
    }
}
