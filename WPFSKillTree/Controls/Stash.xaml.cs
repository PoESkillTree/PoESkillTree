using MB.Algodat;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using POESKillTree.ViewModels.Items;
using POESKillTree.Views;
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



        public ObservableCollection<int> SearchMatches
        {
            get { return (ObservableCollection<int>)GetValue(SearchMatchesProperty); }
            set { SetValue(SearchMatchesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchMatches.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchMatchesProperty =
            DependencyProperty.Register("SearchMatches", typeof(ObservableCollection<int>), typeof(Stash), new PropertyMetadata(new ObservableCollection<int>()));




        Dictionary<Item, ItemVisualizer> _usedVisualizers = new Dictionary<Item, ItemVisualizer>();
        Dictionary<StashBookmark, Rectangle> _usedBMarks = new Dictionary<StashBookmark, Rectangle>();

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

        private double getValueForTop(double top)
        {
            var d = (asBar.Maximum - asBar.Minimum);
            return top * d / (d - StashGridHeight);
        }

        private double getTopForValue(double value)
        {
            return value - value / (asBar.Maximum - asBar.Minimum) * StashGridHeight;
        }

        public int PageTop
        { 
            get
            {
                return (int)Math.Round(getTopForValue(asBar.Value));
            }
        }

        public int PageBottom
        { 
            get
            {
                return PageTop + StashGridHeight + 1;
            }
        }

        private void RedrawItems()
        {
            int from = PageTop;
            int to = PageBottom;

            foreach (var tab in Bookmarks)//TODO bisection method?
            {
                if (tab.Position >= from && tab.Position <= to)
                {
                    var y = (tab.Position - from) * GridSize;

                    Rectangle r;

                    if (!_usedBMarks.TryGetValue(tab, out r))
                    {
                        r = new Rectangle();
                        r.Fill = new SolidColorBrush(tab.Color);
                        r.Height = 2;
                        r.Tag = tab;
                        r.Cursor = Cursors.SizeNS;
                        r.MouseDown += R_MouseDown;

                        gcontent.Children.Add(r);
                        Grid.SetZIndex(r, -1000000);
                        _usedBMarks.Add(tab, r);
                    }
                    r.Margin = new Thickness(0, y - 1, 0, gcontent.ActualHeight - y - 1);
                    continue;
                }

                if (_usedBMarks.ContainsKey(tab))
                {
                    gcontent.Children.Remove(_usedBMarks[tab]);
                    _usedBMarks.Remove(tab);
                }

            }


            var items = new HashSet<Item>(_StashRange.Query(new Range<int>(from, to)));
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

                    if (_FoundItems.Contains(item))
                        iv.Background = ItemVisualizer.FoundItemBrush;

                    iv.MouseLeftButtonDown += Iv_MouseLeftButtonDown;

                    _usedVisualizers.Add(item, iv);
                    gcontent.Children.Add(iv);
                }
                Thickness m = new Thickness(item.X * GridSize, (item.Y - from) * GridSize, 0, 0);
                iv.Margin = m;
                iv.Width = item.W * GridSize;
                iv.Height = item.H * GridSize;
            }
        }

        private void R_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle r = sender as Rectangle;
            StashBookmark sb = r.Tag as StashBookmark;
            _dnd_startDrag = Mouse.GetPosition(gcontent);
            _dnd_overlay = new Rectangle()
            {
                Fill = r.Fill,
                Height = r.Height,
                Margin = r.Margin,
                Opacity = 0.3,
            };
            gcontent.Children.Add(_dnd_overlay);
            Grid.SetZIndex(_dnd_overlay, 52635);
            DragDrop.DoDragDrop(this, sender, DragDropEffects.Move);
            gcontent.Children.Remove(_dnd_overlay);
            _dnd_overlay = null;
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
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.SetValue(BookmarksProperty, new ObservableCollection<StashBookmark>()
                {
                    new StashBookmark("0",0, Colors.Blue),
                    new StashBookmark("1",12, Colors.Maroon),
                    new StashBookmark("20",24, Colors.DarkGreen),
                    new StashBookmark("300",36, Colors.Brown),
                    new StashBookmark("4000",48, Colors.Orange),
                    new StashBookmark("50000",60, Colors.Purple),
                    new StashBookmark("600000",72, Colors.Violet),
                    new StashBookmark("7000000",84, Colors.SteelBlue),
                    new StashBookmark("0",96),
                });
            }

            InitializeComponent();
        }



        public ObservableCollection<StashBookmark> Bookmarks
        {
            get { return (ObservableCollection<StashBookmark>)GetValue(BookmarksProperty); }
            set { SetValue(BookmarksProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bookmarks.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BookmarksProperty =
            DependencyProperty.Register("Bookmarks", typeof(ObservableCollection<StashBookmark>), typeof(Stash), new PropertyMetadata(new ObservableCollection<StashBookmark>(), BookmarksPropertyChanged));

        private static void BookmarksPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var stash = d as Stash;

            if (d == null)
                return;

            var ov = e.OldValue as ObservableCollection<StashBookmark>;
            var nv = e.OldValue as ObservableCollection<StashBookmark>;

            if (ov != null)
                ov.CollectionChanged -= stash.bookmarks_CollectionChanged;

            if (nv != null)
                nv.CollectionChanged += stash.bookmarks_CollectionChanged;
        }

        private void bookmarks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("LastLine");
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



        public int StashGridHeight
        {
            get { return (int)(gcontent.ActualHeight / GridSize); }
        }

        public int LastLine
        {
            get
            {
                return Math.Max(_StashRange.Max, Bookmarks.Count > 0 ? Bookmarks.Max(b => b.Position) : 0) + StashGridHeight;
            }
        }

        bool _supressrebuild = false;

        private void asBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RedrawItems();
        }

        private void gcontent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            asBar.LargeChange = StashGridHeight;

            var p = asBar.LargeChange / (asBar.Maximum - asBar.Minimum);

            asBar.ViewportSize = (asBar.Maximum - asBar.Minimum) * p / (1 - p);

            OnPropertyChanged("LastLine");

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
            var pos = e.GetPosition(gcontent).Y;
            if (pos > gcontent.ActualHeight - GridSize / 3)
            {
                asBar.Value++;
            }
            else if (pos < GridSize / 3)
            {
                asBar.Value--;
            }

            if ((e.AllowedEffects & DragDropEffects.Move) != 0)
            {

                bool line = false;
                if (e.Data.GetDataPresent(typeof(Rectangle)))
                {
                    line = true;
                    if (!((e.Data.GetData(typeof(Rectangle)) as Rectangle).Tag is StashBookmark))
                    {
                        e.Effects = DragDropEffects.None;
                        return;
                    }

                }
                else if (!e.Data.GetDataPresent(typeof(ItemVisualizer)))
                {
                    e.Effects = DragDropEffects.None;
                    return;
                }

                e.Handled = true;
                e.Effects = DragDropEffects.Move;

                var newpos = e.GetPosition(gcontent);

                var dx = newpos.X - _dnd_startDrag.X;
                var dy = newpos.Y - _dnd_startDrag.Y;

                var newx = _dnd_original_Margin.Left + dx;
                var newy = _dnd_original_Margin.Top + dy;

                var x = (int)Math.Round((newx / GridSize));
                var y = (int)Math.Round((newy / GridSize));

                if (line)
                {
                    y = (int)Math.Round(newpos.Y / GridSize);

                    _dnd_overlay.Margin = new Thickness(0, y * GridSize - 1, 0, gcontent.ActualHeight - y * GridSize - 1);
                }
                else
                {
                    _dnd_overlay.Margin = new Thickness(x * GridSize, y * GridSize, 0, 0);

                    y += (int)PageTop;

                    var itm = _dndVis.Item;
                    var overlapedy = _StashRange.Query(new Range<int>(y, y + itm.H - 1));

                    var newposr = new Range<int>(x, x + itm.W - 1);

                    if (overlapedy.Where(i => i != itm).Any(i => new Range<int>(i.X, i.X + i.W - 1).Intersects(newposr)) || newposr.To >= 12 || y < 0 || x < 0)
                        _dnd_overlay.Fill = Brushes.DarkRed;
                    else
                        _dnd_overlay.Fill = Brushes.DarkGreen;

                    _dndVis.Margin = new Thickness(newx, newy, 0, 0);
                }
            }
        }

        private void gcontent_Drop(object sender, DragEventArgs e)
        {
            if ((e.AllowedEffects & DragDropEffects.Move) != 0)
            {

                bool line = false;
                if (e.Data.GetDataPresent(typeof(Rectangle)))
                {
                    line = true;
                    if (!((e.Data.GetData(typeof(Rectangle)) as Rectangle).Tag is StashBookmark))
                    {
                        e.Effects = DragDropEffects.None;
                        return;
                    }

                }
                else if (!e.Data.GetDataPresent(typeof(ItemVisualizer)) || _dnd_overlay.Fill != Brushes.DarkGreen)
                {
                    e.Effects = DragDropEffects.None;
                    return;
                }

                e.Handled = true;
                e.Effects = DragDropEffects.Move;

                var newpos = e.GetPosition(gcontent);

                var dx = newpos.X - _dnd_startDrag.X;
                var dy = newpos.Y - _dnd_startDrag.Y;

                var newx = _dnd_original_Margin.Left + dx;
                var newy = _dnd_original_Margin.Top + dy;

                var x = (int)Math.Round((newx / GridSize));
                var y = (int)Math.Round((newy / GridSize));

                y += (int)PageTop;

                if (line)
                {
                    y = (int)Math.Round(newpos.Y / GridSize + PageTop);
                    Rectangle r = e.Data.GetData(typeof(Rectangle)) as Rectangle;
                    StashBookmark sb = r.Tag as StashBookmark;
                    sb.Position = y;
                    Bookmarks.Remove(sb);
                    AddBookmark(sb);
                    RedrawItems();
                }
                else
                {

                    var itm = _dnd_item;
                    _dnd_item = null;
                    itm.X = x;
                    itm.Y = y;

                    Items.Remove(itm);
                    Items.Add(itm);
                }
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

        private void Button_StashTab_Click(object sender, RoutedEventArgs e)
        {
            var bm = (sender as Button).DataContext as StashBookmark;
            asBar.Value = getValueForTop(bm.Position);
        }

        

        private void Button_AddBookmark(object sender, RoutedEventArgs e)
        {
            int bfr;
            var picker = new TabPicker() { Owner = Window.GetWindow(this) };
            var ret = picker.ShowDialog();
            if (ret == true)
            {
                AddBookmark(new StashBookmark(picker.Text, PageTop + 1, picker.SelectedColor));
                RedrawItems();
            }
        }

        private void AddBookmark(StashBookmark stashBookmark)
        {
            Bookmarks.Insert(findbpos(stashBookmark.Position, 0, Bookmarks.Count), stashBookmark);
        }

        private int findbpos(int position, int from, int limit)
        {
            if (Bookmarks.Count == 0)
                return 0;

            var middle = from + (limit - from) / 2;

            if (middle == from)
                return (Bookmarks[middle].Position > position) ? middle : middle + 1;

            if (middle == limit)
                return limit + 1;

            if (Bookmarks[middle].Position > position)
                return findbpos(position, from, middle);
            else
                return findbpos(position, middle, limit);
        }

        private void ButtonStashTabMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                var bm = (sender as Button).DataContext as StashBookmark;

                var picker = new TabPicker() { Owner = Window.GetWindow(this), SelectedColor = bm.Color, Text = bm.Name };
                if (picker.ShowDialog() == true)
                {
                    if (picker.Delete)
                    {
                        Bookmarks.Remove(bm);
                        RedrawItems();
                    }
                    else
                    {
                        bm.Name = picker.Text;
                        bm.Color = picker.SelectedColor;
                    }
                }
            }
        }

        private void control_Loaded(object sender, RoutedEventArgs e)
        {
            var w = Window.GetWindow(this);
            Keyboard.AddKeyDownHandler(w, KeyDown);
        }

        private void KeyDown(object sender, KeyEventArgs e)
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
                    Items.Remove(((ItemVisualizer)hh).Item);

            }
        }



        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = tbSearch.Text;

            foreach (var item in _usedVisualizers)
            {
                if (_FoundItems.Contains(item.Key))
                    item.Value.ClearValue(ItemVisualizer.BackgroundProperty);
            }

            if (string.IsNullOrWhiteSpace(txt))
            {
                SearchMatches.Clear();
                _FoundItems.Clear();
                return;
            }

            _FoundItems = new HashSet<Item>(Items.Where(i => IsSearchMatch(i, txt)));

            foreach (var item in _usedVisualizers)
            {
                if (_FoundItems.Contains(item.Key))
                {
                    item.Value.Background = ItemVisualizer.FoundItemBrush;
                }
            }

            SearchMatches = new ObservableCollection<int>(_FoundItems.Select(i => i.Y).Distinct());
        }

        HashSet<Item> _FoundItems = new HashSet<Item>();

        private bool IsSearchMatch(Item i, string txt)
        {
            return i.BaseType.ToLower().Contains(txt);
        }

        private void Button_DragEnter(object sender, DragEventArgs e)
        {
            var sb = (sender as Button).DataContext as StashBookmark;
            asBar.Value = getValueForTop(sb.Position);
        }
    }
}
