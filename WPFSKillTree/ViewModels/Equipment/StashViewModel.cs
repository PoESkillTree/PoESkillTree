using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using MoreLinq;
using POESKillTree.Common.ViewModels;
using POESKillTree.Controls;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Model.Items;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Equipment
{
    public class StashViewModel : Notifier, IDropTarget
    {
        // todo order members; sections

        public static double CellSize => 47.0;
        public static int Columns => 12;

        private readonly IExtendedDialogCoordinator _dialogCoordinator;
        private readonly IPersistentData _persistentData;

        private bool _suppressRebuild;
        private int _smallestAddedItemY;

        public ObservableCollection<StashItemViewModel> Items { get; }
            = new ObservableCollection<StashItemViewModel>();

        private IReadOnlyList<int> _searchMatches;
        public IReadOnlyList<int> SearchMatches
        {
            get { return _searchMatches; }
            private set { SetProperty(ref _searchMatches, value); }
        }

        public ObservableCollection<StashBookmark> Bookmarks => _persistentData.StashBookmarks;

        private double _scrollBarValue;
        public double ScrollBarValue
        {
            get { return _scrollBarValue; }
            set { SetProperty(ref _scrollBarValue, value); } // todo may need redraw call?
        }

        // todo bind ScrollBar.LargeChange to this
        // todo bind ScrollBar.ViewportSize to this
        // todo bind Grid.ActualHeight / CellSize to this
        // the amount of rows visible at the same time
        private int _visibleRows;
        public int VisibleRows
        {
            get { return _visibleRows; }
            set { SetProperty(ref _visibleRows, value, () => OnPropertyChanged(nameof(Rows))); }
        }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value, OnSearchTextChanged); }
        }

        public ICommand EditStashTabCommand { get; }
        public ICommand AddStashTabCommand { get; }
        public ICommand ScrollToStashTabCommand { get; }

        public StashViewModel(IExtendedDialogCoordinator dialogCoordinator, IPersistentData persistentData)
        {
            _dialogCoordinator = dialogCoordinator;
            _persistentData = persistentData;
            Items.CollectionChanged += ItemsOnCollectionChanged;
            Bookmarks.CollectionChanged += BookmarksOnCollectionChanged;

            EditStashTabCommand = new AsyncRelayCommand<StashBookmark>(EditStashTabAsync);
            AddStashTabCommand = new AsyncRelayCommand(AddStashTabAsync);
            ScrollToStashTabCommand = new RelayCommand<StashBookmark>(ScrollToStashTab);

            BeginUpdate();
            foreach (var stashItem in _persistentData.StashItems)
            {
                var item = new StashItemViewModel(dialogCoordinator, persistentData.EquipmentData, stashItem);
                Items.Add(item);
            }
            EndUpdate();
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems != null)
            {
                foreach (StashItemViewModel i in args.OldItems)
                {
                    i.PropertyChanged -= ItemOnPropertyChanged;
                }
            }

            if (args.NewItems != null)
            {
                foreach (StashItemViewModel i in args.NewItems)
                {
                    i.PropertyChanged += ItemOnPropertyChanged;
                }
            }

            if (!_suppressRebuild)
            {
                OnPropertyChanged(nameof(Rows));
            }
        }

        private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(StashItemViewModel.Item))
            {
                var item = (StashItemViewModel) sender;
                if (item.Item == null)
                {
                    Items.Remove(item);
                }
            }
        }

        private void BookmarksOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            OnPropertyChanged(nameof(Rows));
        }

        private void OnSearchTextChanged()
        {
            Items.ForEach(i => i.Highlight = false);

            if (SearchText == null || SearchText.Length < 3)
            {
                SearchMatches = new int[0];
                return;
            }

            var matches = Items.Where(IsSearchMatch).ToList();
            matches.ForEach(i => i.Highlight = true);
            SearchMatches = new ObservableCollection<int>(matches.Select(i => i.Item.Y).Distinct());
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

        /* todo if scrollable Grid or ItemsControl
         * 
         * StashBookmark: todo DragDrop settings
         * <Rectangle Fill="{Binding Color}" Height="2" Cursor="SizeNS"
         *            VerticalAlignment="Top" HorizontalAlignment="Stretch"
         *            Panel.ZIndex="10000" />
         * with Margin Top = {Binding Position} * GridSize
         * when dragged:
         *  IsHitTestVisible = false
         *  Opacity = 0.3
         * 
         * StashItemViewModel:
         * <StashItemView VerticalAlignment="Top" HorizontalAlignment="Left" />
         * with Width = {Binding Width} * GridSize
         *      Height = {Binding Height} * GridSize
         *      Margin Top = {Binding Y} * GridSize
         *      Margin Left = {Binding X} * GridSize
         * 
         * EffectMoveAdornerTemplate for bookmark:
         * <Rectangle VerticalAlignment="Top" HorizontalAlignment="Left"
         *            Fill="{Binding Color}" Opacity="0.3" Height="2" />
         * with Width = Rows * GridSize
         *      Margin/Padding Top (may need centered outer grid) so that rectangle is on nearest GridSize multiple
         *        (GridSize * (int) Math.Round(y / CellSize))
         *      Move icon
         * 
         * EffectMoveAdornerTemplate and EffectCopyAdornerTemplate for item:
         * <Rectangle VerticalAlignment="Top" HorizontalAlignment="Left"
         *            Fill="DarkGreen" Opacity="0.3" />
         * with Width = {Binding Width} * GridSize
         *      Height = {Binding Height} * GridSize
         *      Margin/Padding Top and Left (may need centered outer grid) so that DragStart is cursor position on recangle,
         *      adjusted to fit into GridSize
         *        (GridSize * (int) Math.Round(value / CellSize))
         *      Move/Copy icon
         * see https://github.com/punker76/gong-wpf-dragdrop/blob/dev/src/GongSolutions.WPF.DragDrop.Shared/DragDrop.cs#L170
         * 
         * EffectNoneAdornerTemplate for item:
         * same as above with Fill = Brushes.DarkRed and None icon
         * 
         */
        /* todo if fixed Grid or ItemsControl
         * 
         * same as above but:
         * 
         * StashBookmark:
         * with Margin Top = ({Binding Position} - FirstVisibleLine) * GridSize
         * 
         * StashItemViewModel:
         * with Margin Top = ({Binding Y} - FirstVisibleLine) * GridSize
         * 
         * EffectMoveAdornerTemplate and EffectCopyAdornerTemplate for bookmark:
         * 
         * EffectMoveAdornerTemplate and EffectCopyAdornerTemplate for item:
         * 
         * EffectNoneAdornerTemplate for item:
         * 
         * this plus commands:
        private void ScrollUp()
        {
            ScrollBarValue++;
        }

        private void ScrollDown()
        {
            ScrollBarValue--;
        }
         */

        public int FirstVisibleRow
        {
            get { return (int) Math.Round(ScrollBarValue); }
        }

        // todo bind ScrollBar.Maximum to this and set ScrollBar.Minimum to 0
        public int Rows
        {
            get { return LastOccupiedRow + VisibleRows; }
        }

        public int LastOccupiedRow
        {
            get
            {
                return Math.Max(
                    Items.Select(i => i.Item.Y).DefaultIfEmpty().Max(),
                    Bookmarks.Select(b => b.Position).DefaultIfEmpty().Max());
            }
        }

        public void BeginUpdate()
        {
            _suppressRebuild = true;
            _smallestAddedItemY = int.MaxValue;
        }
        
        public void EndUpdate()
        {
            _suppressRebuild = false;
            OnPropertyChanged(nameof(Rows));
            if (_smallestAddedItemY < int.MaxValue)
            {
                ScrollBarValue = _smallestAddedItemY;
            }
        }

        public void AddItem(Item item)
        {
            var itemVm = new StashItemViewModel(_dialogCoordinator, _persistentData.EquipmentData, item);
            Items.Add(itemVm);
            if (!_suppressRebuild)
            {
                ScrollBarValue = item.Y;
            }
            else if (item.Y < _smallestAddedItemY)
            {
                _smallestAddedItemY = item.Y;
            }
        }

        private async Task AddStashTabAsync()
        {
            var vm = new TabPickerViewModel
            {
                IsDeletable = false
            };
            var result = await _dialogCoordinator.EditStashTabAsync(this, vm);
            if (result == TabPickerResult.Affirmative)
            {
                AddStashTab(new StashBookmark(vm.Name, FirstVisibleRow + 1, vm.Color));
            }
        }

        public void AddStashTab(StashBookmark stashBookmark)
        {
            Bookmarks.Insert(FindTabPos(stashBookmark.Position, 0, Bookmarks.Count), stashBookmark);
        }

        private int FindTabPos(int position, int from, int limit)
        {
            if (Bookmarks.Count == 0)
                return 0;

            var middle = from + (limit - from) / 2;

            if (middle == from)
                return (Bookmarks[middle].Position > position) ? middle : middle + 1;

            if (middle == limit)
                return limit + 1;

            if (Bookmarks[middle].Position > position)
                return FindTabPos(position, from, middle);
            return FindTabPos(position, middle, limit);
        }

        private async Task EditStashTabAsync(StashBookmark bookmark)
        {
            var vm = new TabPickerViewModel
            {
                Color = bookmark.Color,
                Name = bookmark.Name
            };
            var result = await _dialogCoordinator.EditStashTabAsync(this, vm);
            switch (result)
            {
                case TabPickerResult.Delete:
                    Bookmarks.Remove(bookmark);
                    break;

                case TabPickerResult.DeleteIncludingItems:
                    var confirmationResult = await _dialogCoordinator.ShowQuestionAsync(this,
                        L10n.Message(
                            "This will delete all items between this and the next bookmark and can not be undone.\nDo you want to continue?"),
                        title: L10n.Message("Delete items"));
                    if (confirmationResult == MessageBoxResult.Yes)
                    {
                        DeleteBookmarkAndItems(bookmark);
                    }
                    break;

                case TabPickerResult.Affirmative:
                    bookmark.Name = vm.Name;
                    bookmark.Color = vm.Color;
                    break;
            }
        }

        private void DeleteBookmarkAndItems(StashBookmark bm)
        {
            BeginUpdate();

            var from = bm.Position;
            var to =
                Bookmarks.Where(b => b.Position > from)
                    .Select(b => b.Position)
                    .DefaultIfEmpty(Rows)
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
                    Items.Remove(item);
                    Items.Add(item);
                }
            }

            Bookmarks.Remove(bm);
            foreach (var bookmark in Bookmarks.ToList())
            {
                if (bookmark.Position >= to)
                {
                    bookmark.Position -= diff;
                }
            }

            EndUpdate();
        }

        private void ScrollToStashTab(StashBookmark bookmark)
        {
            ScrollBarValue = bookmark.Position;
        }

        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var draggedItem = dropInfo.Data as DraggableItemViewModel;
            var bookmark = dropInfo.Data as StashBookmark;

            // todo might not be necessary
            // todo use DragDropEffects.Scroll?
            Point pos = dropInfo.DropPosition;
            // Scroll up or down if at upper or lower end of stash grid.
            if (pos.Y / CellSize > VisibleRows - 0.3)
            {
                ScrollBarValue++;
            }
            else if (pos.Y / CellSize < 0.3)
            {
                ScrollBarValue--;
            }

            if (draggedItem != null)
            {
                Point dragStart = dropInfo.DragInfo.PositionInDraggedItem;
                pos.X -= dragStart.X;
                pos.Y -= dragStart.Y;
                var x = (int) Math.Round(pos.X / CellSize);
                var y = (int) Math.Round(pos.Y / CellSize);
                y += FirstVisibleRow; // todo necessary?

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
                        || (i.Y < y + item.Height && y < i.Y + i.Height))
                    {
                        hasOverlap = true;
                        break;
                    }
                }
                if (!hasOverlap && x >= 0 && y >= 0 && x < Columns)
                {
                    dropInfo.Effects = draggedItem.DropOnStashEffect;
                }
            }
            else if (bookmark != null)
            {
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var effect = dropInfo.Effects;

            var draggedItem = dropInfo.Data as DraggableItemViewModel;
            var bookmark = dropInfo.Data as StashBookmark;
            Point pos = dropInfo.DropPosition;
            var rowsBefore = Rows;

            if (draggedItem != null)
            {
                Point dragStart = dropInfo.DragInfo.PositionInDraggedItem;
                pos.X -= dragStart.X;
                pos.Y -= dragStart.Y;
                var x = (int) Math.Round(pos.X / CellSize);
                var y = (int) Math.Round(pos.Y / CellSize);
                y += FirstVisibleRow; // todo necessary?

                if (effect == DragDropEffects.Move)
                {
                    var item = draggedItem.Item;
                    item.X = x;
                    item.Y = y;
                    if (!Items.Contains(draggedItem))
                    {
                        draggedItem.Item = null;
                        AddItem(item);
                    }
                }
                else
                {
                    var item = new Item(draggedItem.Item)
                    {
                        X = x,
                        Y = y
                    };
                    AddItem(item);
                }
            }
            else if (bookmark != null)
            {
                var y = (int) Math.Round(pos.Y / CellSize);
                y += FirstVisibleRow; // todo necessary?
                bookmark.Position = y;
            }

            if (Rows != rowsBefore)
            {
                OnPropertyChanged(nameof(Rows));
            }
        }
    }
}