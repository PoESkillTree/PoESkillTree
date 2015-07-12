using POESKillTree.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using POESKillTree.Utils;
using System.ComponentModel;

namespace POESKillTree.Controls
{
    /// <summary>
    /// Interaction logic for Inventory.xaml
    /// </summary>
    public partial class Inventory : UserControl
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
                var targslot = (ItemSlot)(sender as ItemVisualizer).Tag;
                var itm = (e.Data.GetData(typeof(ItemVisualizer)) as ItemVisualizer).Item;

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
                var vis = e.Data.GetData(typeof(ItemVisualizer)) as ItemVisualizer;
                var targslot = (ItemSlot)(sender as ItemVisualizer).Tag;
                var itm = (e.Data.GetData(typeof(ItemVisualizer)) as ItemVisualizer).Item;

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
