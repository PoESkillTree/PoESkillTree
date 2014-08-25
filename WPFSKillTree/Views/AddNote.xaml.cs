using System.Windows;
using MahApps.Metro.Controls;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for AddNote.xaml
    /// </summary>
    public partial class AddNote : MetroWindow
    {
        public AddNote()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public string getNote()
        {
            return txtName.Text;
        }

        private void AddNote_Loaded(object sender, RoutedEventArgs e)
        {
            txtName.Focus();
        }
    }
}
