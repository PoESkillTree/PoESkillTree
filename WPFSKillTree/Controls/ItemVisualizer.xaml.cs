using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using POESKillTree.Model.Items;

namespace POESKillTree.Controls
{
    public partial class ItemVisualizer : UserControl
    {
        public static readonly Brush FoundItemBrush = new SolidColorBrush(Color.FromArgb(0x80,0x43,0xD9,0xE8));

        public Item Item
        {
            get { return (Item)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register("Item", typeof(Item), typeof(ItemVisualizer), new PropertyMetadata(null));


        public ItemVisualizer()
        {
            InitializeComponent();
        }

        private void control_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);

            tooltip.VerticalOffset = pos.Y + 2;
            tooltip.HorizontalOffset = pos.X;
        }
    }
}
