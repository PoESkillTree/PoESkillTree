using System.Windows;
using MahApps.Metro.Controls;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for FormChooseBuildName.xaml
    /// </summary>
    public partial class FormChooseBuildName : MetroWindow
    {
        public FormChooseBuildName()
        {
            InitializeComponent();
        }
        public FormChooseBuildName(string characterName, string itemData)
        {
            InitializeComponent();
            txtCharacterName.Text = characterName;
            txtItemData.Text = itemData;
        }
        public FormChooseBuildName(string name, string note, string characterName, string itemData)
        {
            InitializeComponent();
            txtName.Text = name;
            txtName2.Text = note;
            txtCharacterName.Text = characterName;
            txtItemData.Text = itemData;

            txtName.Select(txtName.Text.Length, 0);
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
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
        public string GetItemData()
        {
            return txtItemData.Text;
        }
        private void FormChooseBuildName_Loaded(object sender, RoutedEventArgs e)
        {
            txtName.Focus();
        }
    }
}
