using System;
using System.Windows;
using MahApps.Metro.Controls;
using POESKillTree.Localization;
using POESKillTree.ViewModels;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for FormChooseBuildName.xaml
    /// </summary>
    public partial class FormChooseGroupName : MetroWindow
    {
        public FormChooseGroupName()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public string GetGroupName()
        {
            return txtName.Text;
        }

        private void FormChooseGroupName_Loaded(object sender, RoutedEventArgs e)
        {
            txtName.Focus();
        }
    }
}
