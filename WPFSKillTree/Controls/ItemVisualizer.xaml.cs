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

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            tooltip.VerticalOffset = pos.Y;
            tooltip.HorizontalOffset = pos.X;
            if (pos.X < 0 || pos.Y < 0 || pos.X > ActualWidth || pos.Y > ActualHeight)
                tooltip.IsOpen = false;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            tooltip.IsOpen = true;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            tooltip.IsOpen = false;
        }

        private void Tooltip_OnMouseEnter(object sender, MouseEventArgs e)
        {
            tooltip.IsOpen = true;
        }

        private void Tooltip_OnMouseLeave(object sender, MouseEventArgs e)
        {
            tooltip.IsOpen = false;
        }
    }
}
