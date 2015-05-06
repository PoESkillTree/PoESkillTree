using POESKillTree.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POESKillTree.Controls
{
    public partial class ItemVisualizer : UserControl
    {
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

            tooltip.VerticalOffset = pos.Y+2;
            tooltip.HorizontalOffset = pos.X;
        }
    }
}
