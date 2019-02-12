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
using PoESkillTree.Utils.Extensions;
using POESKillTree.Common.ViewModels;
using POESKillTree.Controls;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Model.Items;
using POESKillTree.Utils;
using POESKillTree.Utils.Wpf;

namespace POESKillTree.ViewModels.Equipment
{
    public class StashViewModelProxy : BindingProxy<StashViewModel>
    {
    }

    /// <summary>
    /// View model for the stash.
    /// </summary>
    public class StashViewModel : Notifier, IDropTarget
    {
        /// <summary>
        /// Gets the width/height of a cell in the stash.
        /// </summary>
        public static double CellSize => 47.0;
        /// <summary>
        /// Gets the number of cells in the stash in each row.
        /// </summary>
        public static int Columns => 12;

        private IExtendedDialogCoordinator _dialogCoordinator;

        // The view model is created before the window is loaded, PersistentData is not available at that point.
        // This is done because some WPF things don't like DataContext initially being null and don't recognize it 
        // changing to a valid value.
        private IPersistentData _persistentData;

        // true if after BeginUpdate() and before EndUpdate()
        private bool _inBatchUpdate;
        // the smallest y of all items added in a batch update, is scrolled to at the end of the update
        private int _smallestAddedItemY;

        /// <summary>
        /// Gets the items in the stash.
        /// </summary>
        public ObservableCollection<StashItemViewModel> Items { get; }
            = new ObservableCollection<StashItemViewModel>();

        /// <summary>
        /// Gets the y values of all items matching the current search.
        /// </summary>
        public ObservableCollection<double> SearchMatches { get; } = new ObservableCollection<double>();

        /// <summary>
        /// Gets the bookmarks/tabs in the stash.
        /// </summary>
        public ObservableCollection<StashBookmarkViewModel> Bookmarks { get; }
            = new ObservableCollection<StashBookmarkViewModel>();

        private double _scrollBarValue;
        /// <summary>
        /// Gets or sets the value of the vertical stash scroll bar.
        /// This floored is the first partly visible row. This ceiled is the first fully visible row.
        /// </summary>
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
        /// <summary>
        /// Gets or sets the number of currently visible rows.
        /// </summary>
        public double VisibleRows
        {
            get { return _visibleRows; }
            set { SetProperty(ref _visibleRows, value, RowsChanged); }
        }

        /// <summary>
        /// Gets the last row that is occupied by an item or bookmark.
        /// </summary>
        public int LastOccupiedRow
        {
            get
            {
                return Math.Max(
                    Items.Select(i => i.Item.Y + i.Item.Height - 1).DefaultIfEmpty().Max(),
                    Bookmarks.Select(b => b.Bookmark.Position).DefaultIfEmpty().Max());
            }
        }

        /// <summary>
        /// Gets the number of cells in the stash in each column.
        /// This is set so there is always a full viewport (minus one row) available below the last item or bookmark.
        /// </summary>
        public double Rows => LastOccupiedRow + VisibleRows;

        private string _searchText;
        /// <summary>
        /// Gets or sets the text by which items should be searched/highlight.
        /// </summary>
        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value, OnSearchTextChanged); }
        }

        public ICommand EditStashTabCommand { get; }
        public ICommand AddStashTabCommand { get; }
        public ICommand ScrollToStashTabCommand { get; }

        /// <summary>
        /// Gets the drop handler that handles dragging over the stash tab buttons.
        /// </summary>
        public IDropTarget StashTabDropHandler { get; }

        public StashViewModel()
        {
            EditStashTabCommand = new AsyncRelayCommand<StashBookmarkViewModel>(EditStashTabAsync);
            AddStashTabCommand = new AsyncRelayCommand(AddStashTabAsync);
            ScrollToStashTabCommand = new RelayCommand<StashBookmarkViewModel>(ScrollToStashTab);

            StashTabDropHandler = new StashTabDropTarget(this);
        }

        public void Initialize(IExtendedDialogCoordinator dialogCoordinator, IPersistentData persistentData)
        {
            _dialogCoordinator = dialogCoordinator;
            _persistentData = persistentData;

            // add view models for bookmarks and items
            // PersistentData.StashBookmarks and PersistentData.StashItems may only be changed through the stash after this
            BeginUpdate();
            Bookmarks.AddRange(persistentData.StashBookmarks.Select(b => new StashBookmarkViewModel(b)));
            Bookmarks.CollectionChanged += (sender, args) => RowsChanged();
            foreach (var stashItem in persistentData.StashItems)
            {
                var item = new StashItemViewModel(stashItem);
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
                    _persistentData.StashItems.Remove(i.Item);
                }
            }

            if (args.NewItems != null)
            {
                foreach (StashItemViewModel i in args.NewItems)
                {
                    i.PropertyChanging += ItemOnPropertyChanging;
                    i.PropertyChanged += ItemOnPropertyChanged;
                    _persistentData.StashItems.Add(i.Item);
                }
            }

            if (!_inBatchUpdate)
            {
                RowsChanged();
            }
        }

        private void ItemOnPropertyChanging(object sender, PropertyChangingEventArgs args)
        {
            if (args.PropertyName == nameof(StashItemViewModel.Item))
            {
                // always remove the view model so the item is removed from PersistentData
                var item = (StashItemViewModel) sender;
                Items.Remove(item);
            }
        }

        private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(StashItemViewModel.Item))
            {
                // add the view model back if the item was changed and not removed
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
            // search in base type name, flavour text, name, properties and mods
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
            // these are getter-only, changes must be triggered manually
            OnPropertyChanged(nameof(LastOccupiedRow));
            OnPropertyChanged(nameof(Rows));
        }

        /// <summary>
        /// Starts a batch update. Call this before adding multiple items to the stash.
        /// </summary>
        public void BeginUpdate()
        {
            _inBatchUpdate = true;
            _smallestAddedItemY = int.MaxValue;
        }
        
        /// <summary>
        /// Ends a batch update. Call this after adding multiple items to the stash.
        /// </summary>
        public void EndUpdate()
        {
            _inBatchUpdate = false;
            RowsChanged();
            if (_smallestAddedItemY < int.MaxValue)
            {
                ScrollBarValue = _smallestAddedItemY;
            }
        }

        public void AddItem(Item item, bool scrollToItem)
        {
            var itemVm = new StashItemViewModel(item);
            Items.Add(itemVm);

            if (!scrollToItem)
            {
                return;
            }
            if (!_inBatchUpdate)
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
            var vm = new StashBookmarkViewModel(stashBookmark);
            var index = FindTabIndex(vm);
            Bookmarks.Insert(index, vm);
            _persistentData.StashBookmarks.Insert(index, stashBookmark);
        }

        private int FindTabIndex(StashBookmarkViewModel bookmark)
        {
            var position = bookmark.Bookmark.Position;
            return Bookmarks.Where(b => b != bookmark).TakeWhile(b => b.Bookmark.Position <= position).Count();
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
                    _persistentData.StashBookmarks.Remove(bookmark);
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
                    .DefaultIfEmpty(LastOccupiedRow + 1)
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
            _persistentData.StashBookmarks.Remove(bm.Bookmark);
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

        /// <summary>
        /// Returns the index of the cell that is closest to the given position (position in pixel)
        /// </summary>
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
                // only allow drop if the drop area does not overlap any item (that is not the source) and
                // the position is inside the stash's bounds
                if (!hasOverlap && x >= 0 && y >= 0 && x + item.Width <= Columns)
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
                // make sure bookmarks are still ordererd
                var oldIndex = Bookmarks.IndexOf(bookmark);
                var newIndex = FindTabIndex(bookmark);
                // with Bookmarks.Move() the scroll bar element's position is not updated
                Bookmarks.RemoveAt(oldIndex);
                Bookmarks.Insert(newIndex, bookmark);
                _persistentData.StashBookmarks.RemoveAt(oldIndex);
                _persistentData.StashBookmarks.Insert(newIndex, bookmark.Bookmark);
            }

            RowsChanged();
        }


        /// <summary>
        /// IDropTarget for dragging over stash tab buttons.
        /// </summary>
        private class StashTabDropTarget : IDropTarget
        {
            private readonly StashViewModel _stashViewModel;

            public StashTabDropTarget(StashViewModel stashViewModel)
            {
                _stashViewModel = stashViewModel;
            }

            public void DragOver(IDropInfo dropInfo)
            {
                // scroll to the targeted tab
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


        /// <summary>
        /// DropTargetAdorner for dragging items over the stash. Shows a rectangle the size of the item that fills
        /// the cells the item would cover if dropped.
        /// </summary>
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


        /// <summary>
        /// DropTargetAdorner for dragging tabs/bookmarks over the stash. Shows a rectangle at the position the 
        /// bookmark would cover if dropped.
        /// </summary>
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