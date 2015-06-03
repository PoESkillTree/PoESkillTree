using MB.Algodat;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using POESKillTree.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
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

namespace POESKillTree.Controls
{
    /// <summary>
    /// Interaction logic for Stash.xaml
    /// </summary>
    public partial class Stash : UserControl, INotifyPropertyChanged
    {

        public ObservableCollection<Item> Items
        {
            get { return (ObservableCollection<Item>)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }

        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<Item>), typeof(Stash), new PropertyMetadata(null, ItemsPropertyChanged));

        private static void ItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var stash = d as Stash;

            if (e.OldValue != null)
            {
                var list = (ObservableCollection<Item>)e.OldValue;
                list.CollectionChanged -= stash.items_CollectionChanged;
            }

            if (e.NewValue != null)
            {
                var list = (ObservableCollection<Item>)e.NewValue;
                list.CollectionChanged += stash.items_CollectionChanged;
            }

            stash.ReloadItem();
        }


        Dictionary<Item, ItemVisualizer> _usedVisualizers = new Dictionary<Item, ItemVisualizer>();

        void items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            if (e.OldItems != null)
            {
                foreach (Item i in e.OldItems)
                    _StashRange.Remove(i);
            }

            if (e.NewItems != null)
            {
                foreach (Item i in e.NewItems)
                    _StashRange.Add(i);

            }

            if (!_supressrebuild)
            {
                _StashRange.Rebuild();
                OnPropertyChanged("LastLine");
            }

            RedrawItems();
        }

        private void RedrawItems()
        {

            int pos = (int)asBar.Value;
            var items = new HashSet<Item>(_StashRange.Query(new Range<int>(pos, (int)Math.Ceiling(pos + asBar.LargeChange))));



            var toremove = _usedVisualizers.Where(p => !items.Contains(p.Key)).ToArray();
            foreach (var item in toremove)
            {
                gcontent.Children.Remove(item.Value);
                _usedVisualizers.Remove(item.Key);
            }

            foreach (var item in items)
            {
                ItemVisualizer iv = null;

                if (!_usedVisualizers.TryGetValue(item, out iv))
                {
                    iv = new ItemVisualizer()
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Item = item
                    };
                    if (item == _dnd_item)
                        iv.Opacity = 0.3;
                    iv.MouseLeftButtonDown += Iv_MouseLeftButtonDown;

                    _usedVisualizers.Add(item, iv);
                    gcontent.Children.Add(iv);
                }

                Thickness m = new Thickness(item.X * GridSize, (item.Y - pos) * GridSize, 0, 0);
                iv.Margin = m;
                iv.Width = item.W * GridSize;
                iv.Height = item.H * GridSize;
            }
        }

        private void ReloadItem()
        {
            _supressrebuild = true;
            foreach (var i in Items)
            {
                _StashRange.Add(i);
            }
            _supressrebuild = false;

            _StashRange.Rebuild();
            OnPropertyChanged("LastLine");

        }




        public double GridSize
        {
            get { return (double)GetValue(GridSizeProperty); }
            set { SetValue(GridSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GridSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridSizeProperty =
            DependencyProperty.Register("GridSize", typeof(double), typeof(Stash), new PropertyMetadata(47.0));




        public Stash()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog of = new OpenFileDialog();
                if (of.ShowDialog() == true)
                {
                    var data = File.ReadAllText(of.FileName);

                    var json = JObject.Parse(data);
                    var items = (json["items"] as JArray).Select(i => new Item((JObject)i));

                    //get free line
                    var y = LastLine + 1;
                    int x = 0;
                    int maxh = 0;
                    foreach (var item in items)
                    {
                        if (x + item.W > 12) //next line
                        {
                            x = 0;
                            y += maxh;
                            maxh = 0;
                        }

                        item.X = x;
                        x += item.W;

                        if (maxh < item.H)
                            maxh = item.H;

                        item.Y = y;
                        Items.Add(item);
                    }
                }
            }
            catch
            {

            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }


        private class ItemModComparer : IComparer<Item>
        {
            public int Compare(Item x, Item y)
            {
                return ((IRangeProvider<int>)x).Range.CompareTo(((IRangeProvider<int>)y).Range);
            }
        }

        private RangeTree<int, Item> _StashRange = new RangeTree<int, Item>(new ItemModComparer());


        public int LastLine
        {
            get { return _StashRange.Max; }
        }
        bool _supressrebuild = false;

        private void asBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RedrawItems();
        }

        private void gcontent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            asBar.LargeChange = (int)(gcontent.ActualHeight / GridSize);
            RedrawItems();
        }

        private void control_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                asBar.Value -= 1;
            else
                asBar.Value += 1;
        }

        ItemVisualizer _dndVis = null;
        Rectangle _dnd_overlay = null;
        Point _dnd_startDrag;
        Thickness _dnd_original_Margin;
        Item _dnd_item = null;
        private void Iv_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            var siv = sender as ItemVisualizer;
            if (siv != null)
            {
                _dnd_startDrag = Mouse.GetPosition(gcontent);// e.GetPosition(gcontent);
                _dnd_overlay = new Rectangle()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = siv.ActualWidth,
                    Height = siv.ActualHeight,
                    Fill = Brushes.DarkGreen,
                    Margin = siv.Margin,
                    Opacity = 0.3,
                };
                _dnd_original_Margin = siv.Margin;
                gcontent.Children.Add(_dnd_overlay);
                Grid.SetZIndex(_dnd_overlay, 52635);

                _dndVis = new ItemVisualizer()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = siv.ActualWidth,
                    Height = siv.ActualHeight,
                    Item = siv.Item,
                    Margin = siv.Margin,
                    Opacity = 0.5,
                    Background = null,
                };
                gcontent.Children.Add(_dndVis);
                Grid.SetZIndex(_dndVis, 52636);


                siv.Opacity = 0.3;
                _dnd_item = siv.Item;
                DragDrop.DoDragDrop(this, sender, DragDropEffects.Link | DragDropEffects.Move);
                siv.Opacity = 1;
                gcontent.Children.Remove(_dnd_overlay);
                gcontent.Children.Remove(_dndVis);
                _dndVis = null;
                _dnd_overlay = null;
                _dnd_item = null;
            }
        }


        private void control_DragOver(object sender, DragEventArgs e)
        {
            if ((e.AllowedEffects & DragDropEffects.Move) != 0 && e.Data.GetDataPresent(typeof(ItemVisualizer)))
            {
                e.Handled = true;
                e.Effects = DragDropEffects.Move;

                var newpos = e.GetPosition(gcontent);

                var dx = newpos.X - _dnd_startDrag.X;
                var dy = newpos.Y - _dnd_startDrag.Y;

                var newx = _dnd_original_Margin.Left + dx;
                var newy = _dnd_original_Margin.Top + dy;

                var x = (int)Math.Round((newx / GridSize));
                var y = (int)Math.Round((newy / GridSize));

                _dnd_overlay.Margin = new Thickness(x * GridSize, y * GridSize, 0, 0);

                y += (int)asBar.Value;

                var itm = _dndVis.Item;
                var overlapedy = _StashRange.Query(new Range<int>(y, y + itm.H - 1));

                var newposr = new Range<int>(x, x + itm.W - 1);

                if (overlapedy.Where(i => i != itm).Any(i => new Range<int>(i.X, i.X + i.W - 1).Intersects(newposr)) || newposr.To >= 12 || y < 0 || x < 0 )
                    _dnd_overlay.Fill = Brushes.DarkRed;
                else
                    _dnd_overlay.Fill = Brushes.DarkGreen;

                _dndVis.Margin = new Thickness(newx, newy, 0, 0);

            }
        }

        private void gcontent_Drop(object sender, DragEventArgs e)
        {
            if ((e.AllowedEffects & DragDropEffects.Move) != 0 && e.Data.GetDataPresent(typeof(ItemVisualizer)) && _dnd_overlay.Fill == Brushes.DarkGreen)
            {
                e.Handled = true;
                e.Effects = DragDropEffects.Move;

                var newpos = e.GetPosition(gcontent);

                var dx = newpos.X - _dnd_startDrag.X;
                var dy = newpos.Y - _dnd_startDrag.Y;

                var newx = _dnd_original_Margin.Left + dx;
                var newy = _dnd_original_Margin.Top + dy;

                var x = (int)Math.Round((newx / GridSize));
                var y = (int)Math.Round((newy / GridSize));

                y += (int)asBar.Value;

                var itm = _dnd_item;
                _dnd_item = null;
                itm.X = x;
                itm.Y = y;

                Items.Remove(itm);
                Items.Add(itm);
            }
        }

        private void gcontent_DragLeave(object sender, DragEventArgs e)
        {
            if (_dndVis != null)
                _dndVis.Visibility = Visibility.Collapsed;

            if (_dnd_overlay != null)
                _dnd_overlay.Visibility = Visibility.Collapsed;
        }

        private void gcontent_DragEnter(object sender, DragEventArgs e)
        {
            if (_dndVis != null)
                _dndVis.Visibility = Visibility.Visible;

            if (_dnd_overlay != null)
                _dnd_overlay.Visibility = Visibility.Visible;
        }
        //public Int32Rect GetFreeSpace(int width, int heighh, int startY = 0, int startX = 0)
        //{
        //    for (int y = 0; y < _StashRange.Max; y++)
        //    {
        //        var itemline = _StashRange.Query(y).OrderBy(i=>i.X).ToArray();

        //        for (int i = 0; i < itemline.Length; i++)
        //        {

        //        }
        //    }
        //}
    }
}
