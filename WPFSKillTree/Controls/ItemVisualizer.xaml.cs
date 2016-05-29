using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using POESKillTree.Model.Items;

namespace POESKillTree.Controls
{
    public partial class ItemVisualizer
    {
        public static readonly Brush FoundItemBrush = new SolidColorBrush(Color.FromArgb(0x80,0x43,0xD9,0xE8));

        public Item Item
        {
            get { return (Item)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register("Item", typeof(Item),
            typeof(ItemVisualizer), new PropertyMetadata(null));

        public ItemVisualizer()
        {
            InitializeComponent();
            // The ItemTooltip is lazily created because it is a major slowdown if all tooltips are recreated for each scroll.
            tooltip.Opened += (sender, args) =>
            {
                if (tooltip.Child != null) return;
                var itemTooltip = new ItemTooltip {Item = Item};
                itemTooltip.SetBinding(ItemTooltip.ItemProperty, new Binding("Item") {Source = this});
                tooltip.Child = itemTooltip;
            };
        }

        private void control_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);

            tooltip.VerticalOffset = pos.Y + 2;
            tooltip.HorizontalOffset = pos.X + 2;
        }
    }
}
