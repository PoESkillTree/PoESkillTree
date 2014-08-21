using System.Windows;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for FormBuildName.xaml
    /// </summary>
    public partial class FormBuildName : Window
    {
        public FormBuildName()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public string getBuildName()
        {
            return txtName.Text;
        }

        private void FormChooseBuildName_Loaded(object sender, RoutedEventArgs e)
        {
            txtName.Focus();
        }
    }
}
