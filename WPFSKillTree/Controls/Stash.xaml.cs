using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MB.Algodat;
using MoreLinq;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Localization;
using POESKillTree.Model.Items;
using POESKillTree.Utils.Extensions;
using POESKillTree.ViewModels.Equipment;
using POESKillTree.Views.Equipment;

namespace POESKillTree.Controls
{

    internal class IntRange
    {
        public int From { get; set; }
        public int Range { get; set; }
    }

    /// <summary>
    /// Interaction logic for Stash.xaml
    /// </summary>
    public partial class Stash : INotifyPropertyChanged
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
            var stash = (Stash) d;

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
            DependencyProperty.Register("SearchMatches", typeof(ObservableCollection<int>), typeof(Stash), new PropertyMetadata(null));



        internal ObservableCollection<IntRange> NewlyAddedRanges
        {
            get { return (ObservableCollection<IntRange>)GetValue(NewlyAddedRangesProperty); }
            set { SetValue(NewlyAddedRangesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewlyAddedRanges.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewlyAddedRangesProperty =
            DependencyProperty.Register("NewlyAddedRanges", typeof(ObservableCollection<IntRange>), typeof(Stash), new PropertyMetadata(null));


        private readonly Dictionary<Item, ItemVisualizer> _usedVisualizers = new Dictionary<Item, ItemVisualizer>();
        private readonly Dictionary<StashBookmark, Rectangle> _usedBMarks = new Dictionary<StashBookmark, Rectangle>();

        private void items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            if (e.OldItems != null)
            {
                foreach (Item i in e.OldItems)
                    _stashRange.Remove(i);
            }

            if (e.NewItems != null)
            {
                foreach (Item i in e.NewItems)
                    _stashRange.Add(i);

            }

            if (!_supressrebuild)
            {
                _stashRange.Rebuild();
                OnPropertyChanged("LastLine");
            }

            RedrawItems();
        }

        private double GetValueForTop(double top)
        {
            var d = asBar.Maximum - asBar.Minimum;
            if (d == StashGridHeight)
                return 0;

            return top * d / (d - StashGridHeight);
        }

        private double GetTopForValue(double value)
        {
            return value - value / (asBar.Maximum - asBar.Minimum) * StashGridHeight;
        }

        private int PageTop
        {
            get { return (int) Math.Round(GetTopForValue(asBar.Value)); }
        }

        private int PageBottom
        {
            get { return PageTop + StashGridHeight + 1; }
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
                        r = new Rectangle
                        {
                            Fill = new SolidColorBrush(tab.Color),
                            Height = 2,
                            Tag = tab,
                            Cursor = Cursors.SizeNS
                        };
                        r.MouseDown += R_MouseDown;

                        gcontent.Children.Add(r);
                        Panel.SetZIndex(r, 10000);
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


            var items = new HashSet<Item>(_stashRange.Query(new Range<int>(from, to)));
            var toremove = _usedVisualizers.Where(p => !items.Contains(p.Key)).ToArray();
            foreach (var item in toremove)
            {
                gcontent.Children.Remove(item.Value);
                _usedVisualizers.Remove(item.Key);
            }

            foreach (var item in items)
            {
                ItemVisualizer iv;

                if (!_usedVisualizers.TryGetValue(item, out iv))
                {
                    iv = new ItemVisualizer
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Item = item
                    };

                    Binding b = new Binding();
                    var menu = Resources["itemContextMenu"] as ContextMenu;

                    b.Source = menu;
                    b.Mode = BindingMode.OneWay;

                    iv.SetBinding(ContextMenuProperty, b);

                    if (_foundItems.Contains(item))
                        iv.Background = ItemVisualizer.FoundItemBrush;

                    iv.MouseLeftButtonDown += Iv_MouseLeftButtonDown;

                    _usedVisualizers.Add(item, iv);
                    gcontent.Children.Add(iv);
                }
                Thickness m = new Thickness(item.X * GridSize, (item.Y - from) * GridSize, 0, 0);
                iv.Margin = m;
                iv.Width = item.Width * GridSize;
                iv.Height = item.Height * GridSize;
            }
        }

        internal void BeginUpdate()
        {
            _supressrebuild = true;
        }


        internal void EndUpdate()
        {
            NewlyAddedRanges.Clear();
            _supressrebuild = false;
            _stashRange.Rebuild();
            ResizeScrollbarThumb();
        }


        private void ReloadItem()
        {
            _supressrebuild = true;
            foreach (var i in Items)
            {
                _stashRange.Add(i);
            }
            _supressrebuild = false;

            _stashRange.Rebuild();
            OnPropertyChanged("LastLine");

        }


        private const double GridSize = 47.0;




        public Stash()
        {
            NewlyAddedRanges = new ObservableCollection<IntRange>();
            SearchMatches = new ObservableCollection<int>();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                SetValue(BookmarksProperty, new ObservableCollection<StashBookmark>
                {
                    new StashBookmark("0",0, Colors.Blue),
                    new StashBookmark("1",12, Colors.Maroon),
                    new StashBookmark("20",24, Colors.DarkGreen),
                    new StashBookmark("300",36, Colors.Brown),
                    new StashBookmark("4000",48, Colors.Orange),
                    new StashBookmark("50000",60, Colors.Purple),
                    new StashBookmark("600000",72, Colors.Violet),
                    new StashBookmark("7000000",84, Colors.SteelBlue),
                    new StashBookmark("0",96)
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
            var stash = (Stash) d;

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

        internal void AddHighlightRange(IntRange range)
        {
            while (NewlyAddedRanges.Count > 5)
                NewlyAddedRanges.RemoveAt(0);

            NewlyAddedRanges.Add(range);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string prop)
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

        private readonly RangeTree<int, Item> _stashRange = new RangeTree<int, Item>(new ItemModComparer());


        private int StashGridHeight
        {
            get { return (int)(gcontent.ActualHeight / GridSize); }
        }

        public int LastLine
        {
            get { return LastOccupiedLine + StashGridHeight; }
        }


        public int LastOccupiedLine
        {
            get { return Math.Max(_stashRange.Max, Bookmarks.Count > 0 ? Bookmarks.Max(b => b.Position) : 0); }
        }

        private bool _supressrebuild;

        private void asBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RedrawItems();
        }

        private void ResizeScrollbarThumb()
        {
            OnPropertyChanged("LastLine");
            asBar.LargeChange = StashGridHeight;
            var length = asBar.Maximum - asBar.Minimum;
            var p = Math.Round(gcontent.ActualHeight / GridSize) / length;

            var newsize = length * p / (1 - p);
            if (newsize <= 0 || double.IsNaN(newsize) || double.IsInfinity(newsize))
                asBar.ViewportSize = double.MaxValue;
            else
                asBar.ViewportSize = newsize;

            RedrawItems();
        }


        private void gcontent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeScrollbarThumb();
        }

        private void control_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                asBar.Value -= 1;
            else
                asBar.Value += 1;
        }

        private Rectangle _dndOverlay;

        private void R_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var r = sender as Rectangle;
            if (r == null)
                return;
            DragDrop.DoDragDrop(this, sender, DragDropEffects.Move);
            r.Opacity = 1;
            r.IsHitTestVisible = true;
        }

        private void Iv_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var itemVis = sender as ItemVisualizer;
            if (itemVis == null || itemVis.Item == null)
                return;
            using (var dragged = new DraggedItem(itemVis))
            {
                DragDrop.DoDragDrop(itemVis, dragged, dragged.AllowedEffects);
                itemVis.Opacity = 1;
                itemVis.IsHitTestVisible = true;
            }
        }

        private static DragDropEffects ItemDropEffect(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DraggedItem)))
            {
                var draggedItem = (DraggedItem)e.Data.GetData(typeof(DraggedItem));
                var effect = draggedItem.DropOnStashEffect;

                if (e.AllowedEffects.HasFlag(effect))
                {
                    return effect;
                }
            }
            return DragDropEffects.None;
        }

        private static DragDropEffects StashBookmarkDropEffect(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Rectangle))
                && ((Rectangle)e.Data.GetData(typeof(Rectangle))).Tag is StashBookmark
                && e.AllowedEffects.HasFlag(DragDropEffects.Move))
            {
                return DragDropEffects.Move;
            }
            return DragDropEffects.None;
        }

        private void gcontent_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            var itemDropEffect = ItemDropEffect(e);
            var stashBookmarkDropEffect = StashBookmarkDropEffect(e);
            if (itemDropEffect == DragDropEffects.None && stashBookmarkDropEffect == DragDropEffects.None)
            {
                return;
            }

            var pos = e.GetPosition(gcontent);
            // Scroll up or down if at upper or lower end of stash grid.
            if (pos.Y > gcontent.ActualHeight - GridSize / 3)
            {
                asBar.Value++;
            }
            else if (pos.Y < GridSize / 3)
            {
                asBar.Value--;
            }

            if (stashBookmarkDropEffect != DragDropEffects.None)
            {
                e.Effects = stashBookmarkDropEffect;

                var y = (int)Math.Round(pos.Y / GridSize);
                _dndOverlay.Margin = new Thickness(0, y * GridSize - 1, 0, gcontent.ActualHeight - y * GridSize - 1);

                var r = (Rectangle)e.Data.GetData(typeof(Rectangle));
                r.Opacity = 0.3;
                r.IsHitTestVisible = false;
            }
            else
            {
                var draggedItem = (DraggedItem)e.Data.GetData(typeof(DraggedItem));
                pos.X -= draggedItem.DragStart.X;
                pos.Y -= draggedItem.DragStart.Y;

                var x = (int)Math.Round(pos.X / GridSize);
                var y = (int)Math.Round(pos.Y / GridSize);

                _dndOverlay.Margin = new Thickness(x * GridSize, y * GridSize, 0, 0);

                y += PageTop;

                var itm = draggedItem.Item;
                var overlapedy = _stashRange.Query(new Range<int>(y, y + itm.Height - 1));

                var newposr = new Range<int>(x, x + itm.Width - 1);

                if (overlapedy.Where(i => i != itm).Any(i => new Range<int>(i.X, i.X + i.Width - 1).Intersects(newposr))
                    || newposr.To >= 12 || y < 0 || x < 0)
                {
                    e.Effects = DragDropEffects.None;
                    _dndOverlay.Fill = Brushes.DarkRed;
                }
                else
                {
                    e.Effects = itemDropEffect;
                    _dndOverlay.Fill = Brushes.DarkGreen;
                }

                var visualizer = draggedItem.SourceItemVisualizer;
                if (visualizer.TryFindParent<Stash>() != null)
                {
                    visualizer.Opacity = 0.3;
                    visualizer.IsHitTestVisible = false;
                }
            }
        }

        private void gcontent_Drop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            var overlayFill = _dndOverlay.Fill;
            gcontent_DragLeave(sender, e);

            var itemDropEffect = ItemDropEffect(e);
            var stashBookmarkDropEffect = StashBookmarkDropEffect(e);
            if (itemDropEffect == DragDropEffects.None && stashBookmarkDropEffect == DragDropEffects.None)
            {
                return;
            }

            var pos = e.GetPosition(gcontent);
            if (stashBookmarkDropEffect != DragDropEffects.None)
            {
                e.Effects = stashBookmarkDropEffect;

                var r = (Rectangle)e.Data.GetData(typeof(Rectangle));

                var y = (int)Math.Round(pos.Y / GridSize);
                y += PageTop;

                var sb = (StashBookmark)r.Tag;
                sb.Position = y;
                Bookmarks.Remove(sb);
                AddBookmark(sb);
                RedrawItems();
            }
            else
            {
                if (!Equals(overlayFill, Brushes.DarkGreen))
                {
                    return;
                }
                e.Effects = itemDropEffect;

                var draggedItem = (DraggedItem)e.Data.GetData(typeof(DraggedItem));
                pos.X -= draggedItem.DragStart.X;
                pos.Y -= draggedItem.DragStart.Y;

                var x = (int)Math.Round(pos.X / GridSize);
                var y = (int)Math.Round(pos.Y / GridSize);
                y += PageTop;

                var itm = itemDropEffect == DragDropEffects.Move ? draggedItem.Item : new Item(draggedItem.Item);
                itm.X = x;
                itm.Y = y;
                Items.Remove(itm);
                Items.Add(itm);
            }
            ResizeScrollbarThumb();
        }

        private void gcontent_DragLeave(object sender, DragEventArgs e)
        {
            if (_dndOverlay != null)
            {
                gcontent.Children.Remove(_dndOverlay);
                _dndOverlay = null;
            }
        }

        private void gcontent_DragEnter(object sender, DragEventArgs e)
        {
            if (ItemDropEffect(e) != DragDropEffects.None)
            {
                var draggedItem = (DraggedItem)e.Data.GetData(typeof(DraggedItem));
                var vis = draggedItem.SourceItemVisualizer;
                _dndOverlay = new Rectangle
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = vis.Item.Width * GridSize,
                    Height = vis.Item.Height * GridSize,
                    Fill = Brushes.DarkGreen,
                    Margin = vis.Margin,
                    Opacity = 0.3,
                    IsHitTestVisible = false
                };
                gcontent.Children.Add(_dndOverlay);
                Panel.SetZIndex(_dndOverlay, 52635);
            }
            else if (StashBookmarkDropEffect(e) != DragDropEffects.None)
            {
                var r = (Rectangle) e.Data.GetData(typeof(Rectangle));
                _dndOverlay = new Rectangle
                {
                    Fill = r.Fill,
                    Height = r.Height,
                    Margin = r.Margin,
                    IsHitTestVisible = false
                };
                gcontent.Children.Add(_dndOverlay);
                Panel.SetZIndex(_dndOverlay, 52635);
            }
        }

        private void Button_StashTab_Click(object sender, RoutedEventArgs e)
        {
            var bm = (StashBookmark) ((Button) sender).DataContext;
            asBar.Value = GetValueForTop(bm.Position);
        }



        private async void Button_AddBookmark(object sender, RoutedEventArgs e)
        {
            var vm = new TabPickerViewModel
            {
                IsDeletable = false
            };
            var result = await this.GetMetroWindow().ShowDialogAsync(vm, new TabPicker());
            if (result == TabPickerResult.Affirmative)
            {
                AddBookmark(new StashBookmark(vm.Name, PageTop + 1, vm.Color));
                RedrawItems();
            }
        }

        public void AddBookmark(StashBookmark stashBookmark)
        {
            Bookmarks.Insert(FindBPos(stashBookmark.Position, 0, Bookmarks.Count), stashBookmark);
        }

        private int FindBPos(int position, int from, int limit)
        {
            if (Bookmarks.Count == 0)
                return 0;

            var middle = from + (limit - from) / 2;

            if (middle == from)
                return (Bookmarks[middle].Position > position) ? middle : middle + 1;

            if (middle == limit)
                return limit + 1;

            if (Bookmarks[middle].Position > position)
                return FindBPos(position, from, middle);
            return FindBPos(position, middle, limit);
        }

        private async void ButtonStashTabMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                var bm = (StashBookmark) ((Button) sender).DataContext;

                var vm = new TabPickerViewModel
                {
                    Name = bm.Name,
                    Color = bm.Color
                };
                var result = await this.GetMetroWindow().ShowDialogAsync(vm, new TabPicker());
                switch (result)
                {
                    case TabPickerResult.Delete:
                        Bookmarks.Remove(bm);
                        RedrawItems();
                        if (_usedBMarks.ContainsKey(bm))
                        {
                            gcontent.Children.Remove(_usedBMarks[bm]);
                            _usedBMarks.Remove(bm);
                        }
                        ResizeScrollbarThumb();
                        break;

                    case TabPickerResult.DeleteIncludingItems:
                        if (await this.GetMetroWindow().ShowQuestionAsync(
                                L10n.Message("This will delete all items between this and the next bookmark and can not be undone.\nDo you want to continue?"),
                                title: L10n.Message("Delete items"))
                            == MessageBoxResult.Yes)
                        {
                            DeleteBookmarkAndItems(bm);
                        }
                        break;

                    case TabPickerResult.Affirmative:
                        bm.Name = vm.Name;
                        bm.Color = vm.Color;
                        if (_usedBMarks.ContainsKey(bm))
                        {
                            _usedBMarks[bm].Fill = new SolidColorBrush(bm.Color);
                        }
                        break;
                }
            }
        }

        private void DeleteBookmarkAndItems(StashBookmark bm)
        {
            var from = bm.Position;
            var to =
                Bookmarks.Where(b => b.Position > from)
                    .Select(b => b.Position)
                    .DefaultIfEmpty(LastLine)
                    .Min();
            var diff = to - from;

            foreach (var item in Items.ToList())
            {
                if (item.Y >= from && item.Y < to)
                {
                    Items.Remove(item);
                }
                else if (item.Y >= to)
                {
                    item.Y -= diff;
                    Items.Remove(item);
                    Items.Add(item);
                }
            }

            Bookmarks.Remove(bm);
            _usedBMarks.Values.ForEach(gcontent.Children.Remove);
            _usedBMarks.Clear();
            foreach (var bookmark in Bookmarks.ToList())
            {
                if (bookmark.Position >= to)
                {
                    bookmark.Position -= diff;
                    Bookmarks.Remove(bookmark);
                    AddBookmark(bookmark);
                }
            }
            ResizeScrollbarThumb();
        }

        private void control_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var w = Window.GetWindow(this);
                Keyboard.AddKeyDownHandler(w, KeyDownHandler);
            }
            ResizeScrollbarThumb();
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
                    Items.Remove(((ItemVisualizer)hh).Item);

                NewlyAddedRanges.Clear();
                ResizeScrollbarThumb();
            }
        }



        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = tbSearch.Text;

            foreach (var item in _usedVisualizers)
            {
                if (_foundItems.Contains(item.Key))
                    item.Value.ClearValue(BackgroundProperty);
            }

            if (string.IsNullOrWhiteSpace(txt))
            {
                SearchMatches.Clear();
                _foundItems.Clear();
                return;
            }

            if (txt.Length < 3)
                return;

            _foundItems = new HashSet<Item>(Items.Where(i => IsSearchMatch(i, txt)));

            foreach (var item in _usedVisualizers)
            {
                if (_foundItems.Contains(item.Key))
                {
                    item.Value.Background = ItemVisualizer.FoundItemBrush;
                }
            }

            SearchMatches = new ObservableCollection<int>(_foundItems.Select(i => i.Y).Distinct());
        }

        private HashSet<Item> _foundItems = new HashSet<Item>();

        private static bool IsSearchMatch(Item i, string txt)
        {
            var modstrings = new [] {
                i.BaseType.Name,
                i.FlavourText,
                i.Name
            }.Union(i.Properties.Select(p => p.Attribute)).Union(i.Mods.Select(m => m.Attribute));

            return modstrings.Any(s => s != null && s.ToLower().Contains(txt));
        }

        private void Button_DragEnter(object sender, DragEventArgs e)
        {
            var sb = (StashBookmark) ((Button)sender).DataContext;
            asBar.Value = GetValueForTop(sb.Position);
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
                    RemoveItem(vis.Item);
                }
            }
        }

        internal void RemoveItem(Item item)
        {
            Items.Remove(item);
            NewlyAddedRanges.Clear();
            ResizeScrollbarThumb();
        }
    }
}
