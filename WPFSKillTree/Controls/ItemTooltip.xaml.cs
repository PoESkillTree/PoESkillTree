using System.Windows;
using System.Windows.Controls;
using POESKillTree.Model.Items;

namespace POESKillTree.Controls
{
    /// <summary>
    /// Interaction logic for Item.xaml
    /// </summary>
    public partial class ItemTooltip : UserControl
    {
        public Item Item
        {
            get { return (Item)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register("Item", typeof(Item), typeof(ItemTooltip), new PropertyMetadata(null));

        public ItemTooltip()
        {
            InitializeComponent();
        }
    }
}
