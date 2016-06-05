using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for TabPicker.xaml
    /// </summary>
    public partial class TabPicker
    {
        public TabPicker()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TabPicker), new PropertyMetadata(""));



        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public bool DialogResult { get; private set; }

        public bool Delete { get; private set; }

        // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(TabPicker), new PropertyMetadata(Color.FromRgb(98,128,0)));


        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.SetValue(SelectedColorProperty, ((SolidColorBrush)((Rectangle)sender).Fill).Color);
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            CloseCommand.Execute(null);
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Delete = true;
            CloseCommand.Execute(null);
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            CloseCommand.Execute(null);
        }
    }
}
