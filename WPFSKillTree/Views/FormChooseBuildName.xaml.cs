using System;
using System.Windows;
using POESKillTree.Localization;
using POESKillTree.ViewModels;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for FormChooseBuildName.xaml
    /// </summary>
    public partial class FormChooseBuildName
    {
        public FormChooseBuildName()
        {
            InitializeComponent();
        }
        public FormChooseBuildName(string characterName, string accountName)
        {
            InitializeComponent();
            txtCharacterName.Text = characterName;
            txtAccountName.Text = accountName;
        }
        public FormChooseBuildName(PoEBuild selectedBuild)
        {
            InitializeComponent();
            txtName.Text = selectedBuild.Name;
            txtName2.Text = selectedBuild.Note;
            txtCharacterName.Text = selectedBuild.CharacterName;
            txtAccountName.Text = selectedBuild.AccountName;
            string date = selectedBuild.LastUpdated == DateTime.MinValue ? L10n.Message("Not Available") : selectedBuild.LastUpdated.ToString();
            lblLastUpdated.Content = string.Format(L10n.Message("Last updated: {0}"), date);
            txtName.Select(txtName.Text.Length, 0);
        }

        public string GetBuildName()
        {
            return txtName.Text;
        }
        public string GetNote()
        {
            return txtName2.Text;
        }
        public string GetCharacterName()
        {
            return txtCharacterName.Text;
        }
        public string GetAccountName()
        {
            return txtAccountName.Text;
        }
        private void FormChooseBuildName_Loaded(object sender, RoutedEventArgs e)
        {
            txtName.Focus();
        }
    }
}
