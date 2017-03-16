using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace POESKillTree.Controls
{
    public partial class ItemVisualizer
    {
        public ItemVisualizer()
        {
            InitializeComponent();
            // The ItemTooltip is lazily created. 
            // Else opening the Equipment tab for the first time would take a few seconds longer.
            tooltip.Opened += (sender, args) =>
            {
                if (tooltip.Child != null) return;
                var itemTooltip = new ItemTooltip
                {
                    DataContext = DataContext,
                    // padding of 5 to not interfere with drag operations
                    Padding = new Thickness(5)
                };
                itemTooltip.SetBinding(DataContextProperty, new Binding("DataContext") { Source = this });
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
