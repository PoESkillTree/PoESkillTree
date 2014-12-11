using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using MahApps.Metro.Controls;

namespace POESKillTree.Views
{
    public partial class AboutWindow : MetroWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void btnPopupClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Href_Navigation(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }
    }
}
