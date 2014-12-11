using System.Windows;
using MahApps.Metro.Controls;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for DownloadItemsWindow.xaml
    /// </summary>
    public partial class HelpWindow : MetroWindow
    {
        public HelpWindow()
        {
            InitializeComponent();
        }

        private void btnPopupClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
