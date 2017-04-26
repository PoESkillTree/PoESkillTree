using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using POESKillTree.Common.ViewModels;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Enums;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;
using POESKillTree.Utils.Wpf;

namespace POESKillTree.ViewModels.Equipment
{
    /// <summary>
    /// View model for a gem socketed in an item. An item will be created from this when the dialog is accepted.
    /// </summary>
    public class SocketedGemViewModel : Notifier
    {
        private int _level;
        private int _quality;
        private int _group;
        private GemBaseViewModel _gemBase;

        /// <summary>
        /// Gets or sets the level of this gem.
        /// </summary>
        public int Level
        {
            get { return _level; }
            set { SetProperty(ref _level, value); }
        }

        /// <summary>
        /// Gets or sets the quality of this gem (in percent).
        /// </summary>
        public int Quality
        {
            get { return _quality; }
            set { SetProperty(ref _quality, value); }
        }

        /// <summary>
        /// Gets or sets the socket group this gem is in. Gems of the same group are linked.
        /// </summary>
        public int Group
        {
            get { return _group; }
            set { SetProperty(ref _group, value); }
        }

        public GemBaseViewModel GemBase
        {
            get { return _gemBase; }
            set { SetProperty(ref _gemBase, value); }
        }

        public SocketedGemViewModel Clone()
        {
            return new SocketedGemViewModel
            {
                GemBase = GemBase,
                Group = Group,
                Quality = Quality,
                Level = Level
            };
        }
    }

    /// <summary>
    /// View model for a gem that can be socketed in items.
    /// </summary>
    public class GemBaseViewModel
    {
        public string Name { get; }

        public ItemImage Icon { get; }

        public ItemDB.Gem Gem { get; }

        public GemBaseViewModel(ItemImageService itemImageService, ItemDB.Gem gem)
        {
            Gem = gem;
            Name = Gem.Name;
            Icon = new ItemImage(itemImageService, Name, ItemGroup.Gem);
        }
    }

    public class SocketedGemsEditingViewModelProxy : BindingProxy<SocketedGemsEditingViewModel>
    {
    }

    /// <summary>
    /// View model for editing gems socketed in an item.
    /// </summary>
    public class SocketedGemsEditingViewModel : CloseableViewModel<bool>
    {
        private readonly Item _itemWithSockets;

        /// <summary>
        /// Gets the gems that can be socketed in the item.
        /// </summary>
        public IReadOnlyList<GemBaseViewModel> AvailableGems { get; }

        /// <summary>
        /// Gets a view source for the gems currently socketed in the item.
        /// </summary>
        public CollectionViewSource SocketedGemsViewSource { get; }

        /// <summary>
        /// The gems currently socketed in the item.
        /// </summary>
        private readonly ObservableCollection<SocketedGemViewModel> _socketedGems
            = new ObservableCollection<SocketedGemViewModel>();

        public ICommand AddGemCommand { get; }
        public ICommand RemoveGemCommand { get; }

        public int NumberOfSockets { get; }

        private SocketedGemViewModel _newSocketedGem;
        /// <summary>
        /// Gets the currently edited gem that can be socketed into the item with AddGemCommand.
        /// </summary>
        public SocketedGemViewModel NewSocketedGem
        {
            get { return _newSocketedGem; }
            private set { SetProperty(ref _newSocketedGem, value); }
        }

        public SocketedGemsEditingViewModel(ItemImageService itemImageService, Item itemWithSockets)
        {
            _itemWithSockets = itemWithSockets;
            // convert ItemDB.Gems into GemBaseViewModels
            AvailableGems = ItemDB.GetAllGems().Select(g => new GemBaseViewModel(itemImageService, g)).ToList();
            NumberOfSockets = _itemWithSockets.BaseType.MaximumNumberOfSockets;
            NewSocketedGem = new SocketedGemViewModel
            {
                GemBase = AvailableGems[0],
                Level = 1,
                Quality = 0,
                Group = 1
            };
            AddGemCommand = new RelayCommand(AddGem, CanAddGem);
            RemoveGemCommand = new RelayCommand<SocketedGemViewModel>(RemoveGem);

            SocketedGemsViewSource = new CollectionViewSource
            {
                Source = _socketedGems
            };
            SocketedGemsViewSource.SortDescriptions.Add(new SortDescription(
                nameof(SocketedGemViewModel.GemBase) + "." + nameof(GemBaseViewModel.Name),
                ListSortDirection.Ascending));
            SocketedGemsViewSource.SortDescriptions.Add(new SortDescription(
                nameof(SocketedGemViewModel.Group), 
                ListSortDirection.Ascending));

            // convert currently socketed gem Items into SocketedGemViewModels
            foreach (var gem in _itemWithSockets.Gems)
            {
                var gemBase = AvailableGems.FirstOrDefault(g => g.Name == gem.Name);
                if (gemBase == null)
                {
                    continue;
                }
                var socketedGem = new SocketedGemViewModel
                {
                    GemBase = gemBase,
                    Level = ItemDB.LevelOf(gem),
                    Quality = ItemDB.QualityOf(gem),
                    Group = gem.SocketGroup + 1
                };
                socketedGem.PropertyChanged += SocketedGemsOnPropertyChanged;
                _socketedGems.Add(socketedGem);
            }
        }

        private void AddGem()
        {
            var addedGem = NewSocketedGem.Clone();
            addedGem.PropertyChanged += SocketedGemsOnPropertyChanged;
            _socketedGems.Add(addedGem);
        }

        private bool CanAddGem()
            => _socketedGems.Count < NumberOfSockets;

        private void RemoveGem(SocketedGemViewModel gem)
        {
            gem.PropertyChanged -= SocketedGemsOnPropertyChanged;
            _socketedGems.Remove(gem);
            NewSocketedGem = gem;
        }

        private void SocketedGemsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(SocketedGemViewModel.Group))
            {
                SocketedGemsViewSource.View.Refresh();
            }
        }

        protected override void OnClose(bool param)
        {
            if (param)
            {
                // replace gems in the edited item with SocketedGems if dialog is accepted
                var gems = new List<Item>();
                foreach (var gem in _socketedGems)
                {
                    var tags = gem.GemBase.Gem.Tags.Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries);
                    var gemItem = new Item(gem.GemBase.Name, tags, gem.Level, gem.Quality, gem.Group - 1);
                    gemItem.SetJsonBase();
                    gems.Add(gemItem);
                }
                _itemWithSockets.Gems = gems;
                _itemWithSockets.SetJsonBase();
            }
            base.OnClose(param);
        }
    }
}