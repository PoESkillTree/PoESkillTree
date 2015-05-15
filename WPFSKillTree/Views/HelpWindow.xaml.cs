using System.Windows;
using MahApps.Metro.Controls;
using POESKillTree.Localization;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : MetroWindow
    {
        public string Document { get; set; }

        public HelpWindow()
        {
            Document = L10n.ReadAllText("Help.md");
            if (string.IsNullOrEmpty(Document))
                Document = L10n.Message("Help file not found");

            InitializeComponent();
        }

        private void btnPopupClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
