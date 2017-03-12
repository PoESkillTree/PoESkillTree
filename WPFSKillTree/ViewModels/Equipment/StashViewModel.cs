using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GongSolutions.Wpf.DragDrop;
using MoreLinq;
using POESKillTree.Common.ViewModels;
using POESKillTree.Controls;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Model.Items;
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;
using POESKillTree.Utils.Wpf;

namespace POESKillTree.ViewModels.Equipment
{
    public class StashViewModelProxy : BindingProxy<StashViewModel>
    {
    }

    public class StashViewModel : Notifier, IDropTarget
    {
        public static double CellSize => 47.0;
        public static int Columns => 12;

        private readonly IExtendedDialogCoordinator _dialogCoordinator;

        private IPersistentData _persistentData;
        public IPersistentData PersistentData
        {
            private get { return _persistentData; }
            set
            {
                if (_persistentData != null)
                {
                    throw new InvalidOperationException("PersistentData may only be set once");
                }
                SetProperty(ref _persistentData, value, OnPersistentDataChanged);
            }
        }

        private bool _suppressRebuild;
        private int _smallestAddedItemY;

        public ObservableCollection<StashItemViewModel> Items { get; }
            = new ObservableCollection<StashItemViewModel>();

        public ObservableCollection<double> SearchMatches { get; } = new ObservableCollection<double>();

        public ObservableCollection<StashBookmarkViewModel> Bookmarks { get; }
            = new ObservableCollection<StashBookmarkViewModel>();

        private double _scrollBarValue;
        // This floored is the first partly visible row. This ceiled is the first fully visible row.
        public double ScrollBarValue
        {
            get { return _scrollBarValue; }
            set
            {
                var v = Math.Min(Math.Max(value, 0), LastOccupiedRow);
                SetProperty(ref _scrollBarValue, v);
            }
        }

        private double _visibleRows;
        public double VisibleRows
        {
            get { return _visibleRows; }
            set { SetProperty(ref _visibleRows, value, RowsChanged); }
        }

        public int LastOccupiedRow
        {
            get
            {
                return Math.Max(
                    Items.Select(i => i.Item.Y + i.Item.Height - 1).DefaultIfEmpty().Max(),
                    Bookmarks.Select(b => b.Bookmark.Position).DefaultIfEmpty().Max());
            }
        }

        public double Rows
            => LastOccupiedRow + VisibleRows;

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value, OnSearchTextChanged); }
        }

        public ICommand EditStashTabCommand { get; }
        public ICommand AddStashTabCommand { get; }
        public ICommand ScrollToStashTabCommand { get; }

        public IDropTarget StashTabDropHandler { get; }

        public StashViewModel(IExtendedDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            EditStashTabCommand = new AsyncRelayCommand<StashBookmarkViewModel>(EditStashTabAsync);
            AddStashTabCommand = new AsyncRelayCommand(AddStashTabAsync);
            ScrollToStashTabCommand = new RelayCommand<StashBookmarkViewModel>(ScrollToStashTab);

            StashTabDropHandler = new StashTabDropTarget(this);
        }

        private void OnPersistentDataChanged()
        {
            BeginUpdate();
            Bookmarks.AddRange(PersistentData.StashBookmarks.Select(b => new StashBookmarkViewModel(b)));
            Bookmarks.CollectionChanged += (sender, args) => RowsChanged();
            foreach (var stashItem in PersistentData.StashItems)
            {
                var item = new StashItemViewModel(_dialogCoordinator, PersistentData.EquipmentData, stashItem);
                item.PropertyChanging += ItemOnPropertyChanging;
                item.PropertyChanged += ItemOnPropertyChanged;
                Items.Add(item);
            }
            Items.CollectionChanged += ItemsOnCollectionChanged;
            EndUpdate();
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems != null)
            {
                foreach (StashItemViewModel i in args.OldItems)
                {
                    i.PropertyChanging -= ItemOnPropertyChanging;
                    i.PropertyChanged -= ItemOnPropertyChanged;
                    PersistentData.StashItems.Remove(i.Item);
                }
            }

            if (args.NewItems != null)
            {
                foreach (StashItemViewModel i in args.NewItems)
                {
                    i.PropertyChanging += ItemOnPropertyChanging;
                    i.PropertyChanged += ItemOnPropertyChanged;
                    PersistentData.StashItems.Add(i.Item);
                }
            }

            if (!_suppressRebuild)
            {
                RowsChanged();
            }
        }

        private void ItemOnPropertyChanging(object sender, PropertyChangingEventArgs args)
        {
            if (args.PropertyName == nameof(StashItemViewModel.Item))
            {
                var item = (StashItemViewModel) sender;
                Items.Remove(item);
            }
        }

        private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(StashItemViewModel.Item))
            {
                var item = (StashItemViewModel) sender;
                if (item.Item != null)
                {
                    Items.Add(item);
                }
            }
        }

        private void OnSearchTextChanged()
        {
            Items.ForEach(i => i.Highlight = false);
            SearchMatches.Clear();

            if (SearchText == null || SearchText.Length < 3)
            {
                return;
            }

            var matches = Items.Where(IsSearchMatch).ToList();
            matches.ForEach(i => i.Highlight = true);
            SearchMatches.AddRange(matches.Select(i => i.Item.Y + i.Item.Height / 2.0).Distinct());
        }

        private bool IsSearchMatch(StashItemViewModel itemVm)
        {
            var i = itemVm.Item;
            var modstrings = new[] {
                i.BaseType.Name,
                i.FlavourText,
                i.Name
            }.Union(i.Properties.Select(p => p.Attribute)).Union(i.Mods.Select(m => m.Attribute));
            
            return modstrings.Any(s => s != null && s.ToLower().Contains(SearchText));
        }

        private void RowsChanged()
        {
            OnPropertyChanged(nameof(LastOccupiedRow));
            OnPropertyChanged(nameof(Rows));
        }

        public void BeginUpdate()
        {
            _suppressRebuild = true;
            _smallestAddedItemY = int.MaxValue;
        }
        
        public void EndUpdate()
        {
            _suppressRebuild = false;
            RowsChanged();
            if (_smallestAddedItemY < int.MaxValue)
            {
                ScrollBarValue = _smallestAddedItemY;
            }
        }

        public void AddItem(Item item, bool scrollToItem)
        {
            var itemVm = new StashItemViewModel(_dialogCoordinator, PersistentData.EquipmentData, item);
            Items.Add(itemVm);

            if (!scrollToItem)
            {
                return;
            }
            if (!_suppressRebuild)
            {
                ScrollBarValue = item.Y;
            }
            else if (item.Y < _smallestAddedItemY)
            {
                _smallestAddedItemY = item.Y;
            }
        }

        #region Stash Tabs

        private async Task AddStashTabAsync()
        {
            var vm = new TabPickerViewModel
            {
                IsDeletable = false
            };
            var result = await _dialogCoordinator.EditStashTabAsync(this, vm);
            if (result == TabPickerResult.Affirmative)
            {
                AddStashTab(new StashBookmark(vm.Name, (int) ScrollBarValue + 1, vm.Color));
            }
        }

        public void AddStashTab(StashBookmark stashBookmark)
        {
            var position = FindTabPos(stashBookmark.Position, 0, Bookmarks.Count);
            Bookmarks.Insert(position, 
                new StashBookmarkViewModel(stashBookmark));
            PersistentData.StashBookmarks.Insert(position, stashBookmark);
        }

        private int FindTabPos(int position, int from, int limit)
        {
            if (Bookmarks.Count == 0)
                return 0;

            var middle = from + (limit - from) / 2;

            if (middle == from)
                return (Bookmarks[middle].Bookmark.Position > position) ? middle : middle + 1;

            if (middle == limit)
                return limit + 1;

            if (Bookmarks[middle].Bookmark.Position > position)
                return FindTabPos(position, from, middle);
            return FindTabPos(position, middle, limit);
        }

        private async Task EditStashTabAsync(StashBookmarkViewModel bookmarkVm)
        {
            var bookmark = bookmarkVm.Bookmark;
            var vm = new TabPickerViewModel
            {
                Color = bookmark.Color,
                Name = bookmark.Name
            };
            var result = await _dialogCoordinator.EditStashTabAsync(this, vm);
            switch (result)
            {
                case TabPickerResult.Delete:
                    Bookmarks.Remove(bookmarkVm);
                    PersistentData.StashBookmarks.Remove(bookmark);
                    break;

                case TabPickerResult.DeleteIncludingItems:
                    var confirmationResult = await _dialogCoordinator.ShowQuestionAsync(this,
                        L10n.Message(
                            "This will delete all items between this and the next bookmark and can not be undone.\nDo you want to continue?"),
                        title: L10n.Message("Delete items"));
                    if (confirmationResult == MessageBoxResult.Yes)
                    {
                        DeleteBookmarkAndItems(bookmarkVm);
                    }
                    break;

                case TabPickerResult.Affirmative:
                    bookmark.Name = vm.Name;
                    bookmark.Color = vm.Color;
                    break;
            }
        }

        private void DeleteBookmarkAndItems(StashBookmarkViewModel bm)
        {
            BeginUpdate();

            var from = bm.Bookmark.Position;
            var to =
                Bookmarks.Where(b => b.Bookmark.Position > from)
                    .Select(b => b.Bookmark.Position)
                    .DefaultIfEmpty(LastOccupiedRow)
                    .Min();
            var diff = to - from;

            foreach (var item in Items.ToList())
            {
                var y = item.Item.Y;
                if (y >= from && y < to)
                {
                    Items.Remove(item);
                }
                else if (y >= to)
                {
                    item.Item.Y -= diff;
                }
            }

            Bookmarks.Remove(bm);
            PersistentData.StashBookmarks.Remove(bm.Bookmark);
            foreach (var bookmark in Bookmarks.ToList())
            {
                if (bookmark.Bookmark.Position >= to)
                {
                    bookmark.Bookmark.Position -= diff;
                }
            }

            EndUpdate();
        }

        private void ScrollToStashTab(StashBookmarkViewModel bookmark)
        {
            ScrollBarValue = bookmark.Bookmark.Position;
        }

        #endregion

        #region Drag&Drop

        private static int NearestCell(double pos)
        {
            return (int) Math.Round(pos / CellSize);
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var draggedItem = dropInfo.Data as DraggableItemViewModel;
            var bookmark = dropInfo.Data as StashBookmarkViewModel;

            Point pos = dropInfo.DropPosition;
            // Scroll up or down if at upper or lower end of stash grid.
            const double scrollRate = 0.6;
            const double scrollThreshold = 0.5;
            if (pos.Y / CellSize > VisibleRows - scrollThreshold)
            {
                ScrollBarValue += scrollRate;
            }
            else if (pos.Y / CellSize < scrollThreshold)
            {
                ScrollBarValue -= scrollRate;
            }

            if (draggedItem != null)
            {
                Point dragStart = dropInfo.DragInfo.PositionInDraggedItem;
                pos.X -= dragStart.X;
                pos.Y -= dragStart.Y;
                var x = NearestCell(pos.X);
                var y = NearestCell(pos.Y + ScrollBarValue * CellSize);

                var item = draggedItem.Item;
                var hasOverlap = false;
                foreach (var itemViewModel in Items)
                {
                    var i = itemViewModel.Item;
                    if (i == item)
                    {
                        continue;
                    }
                    if ((i.X < x + item.Width && x < i.X + i.Width)
                        && (i.Y < y + item.Height && y < i.Y + i.Height))
                    {
                        hasOverlap = true;
                        break;
                    }
                }
                if (!hasOverlap && x >= 0 && y >= 0 && x < Columns)
                {
                    dropInfo.Effects = draggedItem.DropOnStashEffect;
                }
                dropInfo.DropTargetAdorner = typeof(ItemDropTargetAdorner);
            }
            else if (bookmark != null)
            {
                dropInfo.Effects = DragDropEffects.Move;
                dropInfo.DropTargetAdorner = typeof(BookmarkDropTargetAdorner);
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var effect = dropInfo.Effects;

            var draggedItem = dropInfo.Data as DraggableItemViewModel;
            var bookmark = dropInfo.Data as StashBookmarkViewModel;
            Point pos = dropInfo.DropPosition;

            if (draggedItem != null)
            {
                Point dragStart = dropInfo.DragInfo.PositionInDraggedItem;
                pos.X -= dragStart.X;
                pos.Y -= dragStart.Y;
                var x = NearestCell(pos.X);
                var y = NearestCell(pos.Y + ScrollBarValue * CellSize);

                if (effect == DragDropEffects.Move)
                {
                    var item = draggedItem.Item;
                    item.X = x;
                    item.Y = y;
                    if (!Items.Contains(draggedItem))
                    {
                        draggedItem.Item = null;
                        AddItem(item, false);
                    }
                }
                else
                {
                    var item = new Item(draggedItem.Item)
                    {
                        X = x,
                        Y = y
                    };
                    AddItem(item, false);
                }
            }
            else if (bookmark != null)
            {
                var y = NearestCell(pos.Y + ScrollBarValue * CellSize);
                bookmark.Bookmark.Position = y;
            }

            RowsChanged();
        }


        private class StashTabDropTarget : IDropTarget
        {
            private readonly StashViewModel _stashViewModel;

            public StashTabDropTarget(StashViewModel stashViewModel)
            {
                _stashViewModel = stashViewModel;
            }

            public void DragOver(IDropInfo dropInfo)
            {
                var tab = ((FrameworkElement) dropInfo.VisualTarget).DataContext as StashBookmarkViewModel;
                if (tab != null)
                {
                    _stashViewModel.ScrollToStashTab(tab);
                }
            }

            public void Drop(IDropInfo dropInfo)
            {
                // drop is not allowed, DragDropEffects are always None
            }
        }


        private class ItemDropTargetAdorner : DropTargetAdorner
        {
            private static readonly Brush DefaultBrush = new SolidColorBrush(Color.FromArgb(80, 0, 100, 0));
            private static readonly Brush NoneBrush = new SolidColorBrush(Color.FromArgb(80, 139, 0, 0));

            public ItemDropTargetAdorner(UIElement adornedElement, DropInfo dropInfo)
                : base(adornedElement, dropInfo)
            {
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                // get drop position relative to AdornedElement
                Point pos = DropInfo.VisualTargetItem.TranslatePoint(DropInfo.DropPosition, AdornedElement);
                Point dragStart = DropInfo.DragInfo.PositionInDraggedItem;
                pos.X -= dragStart.X;
                pos.Y -= dragStart.Y;

                var item = ((DraggableItemViewModel) DropInfo.Data).Item;
                var rect = new Rect
                {
                    X = NearestCell(pos.X) * CellSize,
                    Y = NearestCell(pos.Y) * CellSize,
                    Width = item.Width * CellSize,
                    Height = item.Height * CellSize
                };

                var brush = DropInfo.Effects == DragDropEffects.None
                    ? NoneBrush : DefaultBrush;
                drawingContext.DrawRectangle(brush, null, rect);
            }
        }


        private class BookmarkDropTargetAdorner : DropTargetAdorner
        {
            public BookmarkDropTargetAdorner(UIElement adornedElement, DropInfo dropInfo)
                : base(adornedElement, dropInfo)
            {
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                // get drop position relative to AdornedElement
                Point pos = DropInfo.VisualTargetItem.TranslatePoint(DropInfo.DropPosition, AdornedElement);
                var rect = new Rect
                {
                    X = 0,
                    Y = NearestCell(pos.Y) * CellSize - 1,
                    Width = VisualTreeHelper.GetDescendantBounds(DropInfo.VisualTargetItem).Width,
                    Height = 2
                };

                var bookmark = ((StashBookmarkViewModel)DropInfo.Data).Bookmark;
                var brush = new SolidColorBrush(bookmark.Color);
                drawingContext.DrawRectangle(brush, null, rect);
            }
        }

        #endregion
    }
}