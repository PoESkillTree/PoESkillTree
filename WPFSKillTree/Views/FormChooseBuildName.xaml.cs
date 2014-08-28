using System.Windows;
using MahApps.Metro.Controls;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for FormBuildName.xaml
    /// </summary>
    public partial class FormBuildName : MetroWindow
    {
        public FormBuildName()
        {
            InitializeComponent();
        }

        public FormBuildName(string name, string note)
        {
            InitializeComponent();
            txtName.Text = name;
            txtName2.Text = note;
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

        private void FormChooseBuildName_Loaded(object sender, RoutedEventArgs e)
        {
            txtName.Focus();
        }
    }
}
