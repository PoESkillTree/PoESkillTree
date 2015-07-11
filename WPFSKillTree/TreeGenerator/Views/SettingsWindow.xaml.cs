using POESKillTree.TreeGenerator.ViewModels;

namespace POESKillTree.TreeGenerator.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow(SettingsViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            vm.StartController += VmOnStartController;
        }

        private void VmOnStartController(object sender, StartControllerEventArgs startControllerEventArgs)
        {
            var dialog = new ControllerWindow(startControllerEventArgs.ViewModel) {Owner = this};
            dialog.ShowDialog();
        }
    }
}
