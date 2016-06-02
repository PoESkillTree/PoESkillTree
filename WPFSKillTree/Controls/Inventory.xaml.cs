using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using MahApps.Metro.Controls;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Enums;

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

        private DragDropEffects DropEffect(object sender, DragEventArgs e)
        {
            var target = sender as ItemVisualizer;
            if (target != null && e.Data.GetDataPresent(typeof(DraggedItem)))
            {
                var targslot = (ItemSlot)target.Tag;
                var draggedItem = (DraggedItem)e.Data.GetData(typeof(DraggedItem));
                var effect = draggedItem.DropOnInventoryEffect;

                if (e.AllowedEffects.HasFlag(effect) && ItemAttributes.CanEquip(draggedItem.Item, targslot))
                {
                    return effect;
                }
            }
            return DragDropEffects.None;
        }

        private void ItemVisualizer_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = DropEffect(sender, e);
        }

        private void ItemVisualizer_Drop(object sender, DragEventArgs e)
        {
            var effect = DropEffect(sender, e);
            if (effect == DragDropEffects.None)
                return;

            e.Handled = true;
            e.Effects = effect;
            var draggedItem = (DraggedItem) e.Data.GetData(typeof(DraggedItem));
            var visualizer = (ItemVisualizer) sender;
            visualizer.Item = effect == DragDropEffects.Copy ? new Item(draggedItem.Item) : draggedItem.Item;
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
                var menu = mi.TryFindParent<ContextMenu>();
                var vis = menu.PlacementTarget as ItemVisualizer;
                if (vis != null)
                {
                    vis.Item = null;
                }
            }
        }

        private void iv_mouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var itemVis = sender as ItemVisualizer;
            if (itemVis == null || itemVis.Item == null)
                return;
            using (var dragged = new DraggedItem(itemVis) {DropOnStashEffect = DragDropEffects.Copy})
            {
                DragDrop.DoDragDrop(itemVis, dragged, dragged.AllowedEffects);
            }
        }
    }
}
