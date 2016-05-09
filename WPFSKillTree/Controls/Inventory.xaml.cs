using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using POESKillTree.Utils;
using System.ComponentModel;
using POESKillTree.Model.Items;

namespace POESKillTree.Controls
{
    /// <summary>
    /// Interaction logic for Inventory.xaml
    /// </summary>
    public partial class Inventory
    {
        public ItemAttributes ItemAttributes
        {
            get { return (ItemAttributes)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register("ItemAttributes", typeof(ItemAttributes), typeof(Inventory), new PropertyMetadata(null));


        public Inventory()
        {
            InitializeComponent();
        }

        private void ItemVisualizer_DragOver(object sender, DragEventArgs e)
        {
            if ((e.AllowedEffects & DragDropEffects.Link) != 0 && e.Data.GetDataPresent(typeof(ItemVisualizer)))
            {
                var targslot = (ItemSlot)((ItemVisualizer) sender).Tag;
                var itm = ((ItemVisualizer) e.Data.GetData(typeof(ItemVisualizer))).Item;

                if (itm != null && (((int)itm.Class & (int)targslot) != 0 || (itm.Class == ItemClass.TwoHand && targslot == ItemSlot.MainHand)))
                {
                    e.Handled = true;
                    e.Effects = DragDropEffects.Link;
                }
                else
                {
                    e.Handled = true;
                    e.Effects = DragDropEffects.None;
                }
            }
        }

        private void ItemVisualizer_Drop(object sender, DragEventArgs e)
        {
            var target = sender as ItemVisualizer;
            if (target != null && (e.AllowedEffects & DragDropEffects.Link) != 0 && e.Data.GetDataPresent(typeof(ItemVisualizer)))
            {
                var vis = (ItemVisualizer) e.Data.GetData(typeof(ItemVisualizer));
                var targslot = (ItemSlot) target.Tag;
                var itm = ((ItemVisualizer)e.Data.GetData(typeof(ItemVisualizer))).Item;

                if (itm != null && (((int)itm.Class & (int)targslot) != 0 || (itm.Class == ItemClass.TwoHand && targslot == ItemSlot.MainHand)))
                {
                    e.Handled = true;
                    e.Effects = DragDropEffects.Link;
                    target.Item = vis.Item;
                }
            }
        }

        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Point pt = Mouse.GetPosition(this);
                var hit = VisualTreeHelper.HitTest(this, pt);

                if (hit == null)
                    return;

                var hh = hit.VisualHit;

                while (hh != null && !(hh is ItemVisualizer))
                    hh = VisualTreeHelper.GetParent(hh);

                if (hh != null)
                    (hh as ItemVisualizer).Item = null;
            }
        }

        private void control_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var w = Window.GetWindow(this);
                Keyboard.AddKeyDownHandler(w, KeyDownHandler);
            }
        }

        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            if (mi != null)
            {
                var menu = mi.FindParent<ContextMenu>();
                var vis = menu.PlacementTarget as ItemVisualizer;
                if (vis != null)
                {
                    vis.Item = null;
                }
            }
        }

        private void iv_mouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragDrop.DoDragDrop(this, sender, DragDropEffects.Link | DragDropEffects.Move);
        }
    }
}
