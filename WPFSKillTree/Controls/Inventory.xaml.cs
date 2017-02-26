using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Linq;
using MahApps.Metro.Controls;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Enums;
using POESKillTree.ViewModels.Crafting;
using POESKillTree.Views.Crafting;

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

        public static readonly DependencyProperty EquipmentDataProperty = DependencyProperty.Register(
            "EquipmentData", typeof(EquipmentData), typeof(Inventory), new PropertyMetadata(default(EquipmentData)));

        public EquipmentData EquipmentData
        {
            get { return (EquipmentData) GetValue(EquipmentDataProperty); }
            set { SetValue(EquipmentDataProperty, value); }
        }

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
            if (effect == DragDropEffects.Copy)
            {
                var newItem = new Item(draggedItem.Item);
                var oldItem = visualizer.Item;
                // Copy gems from old item if the old item has gems, this item doesn't have gems and is not
                // from this Inventory (but from the Stash).
                if (oldItem?.Gems != null && oldItem.Gems.Any()
                    && (newItem.Gems == null || !newItem.Gems.Any())
                    && draggedItem.SourceItemVisualizer.TryFindParent<Inventory>() != this)
                {
                    newItem.CopyGemsFrom(oldItem);
                }
                visualizer.Item = newItem;
            }
            else
            {
                visualizer.Item = draggedItem.Item;
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
                var menu = mi.TryFindParent<ContextMenu>();
                var vis = menu.PlacementTarget as ItemVisualizer;
                if (vis != null)
                {
                    vis.Item = null;
                }
            }
        }

        private async void MenuItem_EditSocketedGems_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            if (mi != null)
            {
                var menu = mi.TryFindParent<ContextMenu>();
                var vis = menu.PlacementTarget as ItemVisualizer;
                if (vis?.Item != null && vis.Item.BaseType.MaximumNumberOfSockets > 0)
                {
                    var w = (MetroWindow) Window.GetWindow(this);
                    await w.ShowDialogAsync(
                        new SocketedGemsEditingViewModel(EquipmentData.ItemImageService, vis.Item), 
                        new SocketedGemsEditingView());
                }
            }
        }

        private void iv_mouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var itemVis = sender as ItemVisualizer;
            if (itemVis?.Item == null)
                return;
            using (var dragged = new DraggedItem(itemVis) {DropOnStashEffect = DragDropEffects.Copy})
            {
                DragDrop.DoDragDrop(itemVis, dragged, dragged.AllowedEffects);
            }
        }
    }
}
