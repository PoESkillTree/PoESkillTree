using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Model.Items;
using PoESkillTree.Utils.Wpf;

namespace PoESkillTree.ViewModels.Equipment
{
    /// <summary>
    /// View model for a gem socketed in an item. An item will be created from this when the dialog is accepted.
    /// </summary>
    public class SocketedGemViewModel : Notifier
    {
        private int _level;
        private int _quality;
        private int? _group;
        private GemBaseViewModel _gemBase;
        private bool _isEnabled;

        /// <summary>
        /// Gets or sets the level of this gem.
        /// </summary>
        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        /// <summary>
        /// Gets or sets the quality of this gem (in percent).
        /// </summary>
        public int Quality
        {
            get => _quality;
            set => SetProperty(ref _quality, value);
        }

        /// <summary>
        /// Gets or sets the socket group this gem is in. Gems of the same group are linked.
        /// </summary>
        public int? Group
        {
            get => _group;
            set => SetProperty(ref _group, value);
        }

        public GemBaseViewModel GemBase
        {
            get => _gemBase;
            set => SetProperty(ref _gemBase, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public SocketedGemViewModel Clone()
        {
            return new SocketedGemViewModel
            {
                GemBase = GemBase,
                Group = Group,
                Quality = Quality,
                Level = Level,
                IsEnabled = IsEnabled,
            };
        }
    }

    /// <summary>
    /// View model for a gem that can be socketed in items.
    /// </summary>
    public class GemBaseViewModel
    {
        private readonly SkillDefinition _skill;
        public string Id => _skill.Id;
        public string Name => _skill.BaseItem?.DisplayName ?? "";

        public int MaxLevel
            => _skill.BaseItem?.GemTags.FirstOrDefault(s => s == "low_max_level") is null ? 21 : 4;

        public ItemImage Icon { get; }

        public GemBaseViewModel(ItemImageService itemImageService, SkillDefinition skill)
        {
            _skill = skill;
            Icon = new ItemImage(itemImageService, Name, ItemClass.ActiveSkillGem);
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
        private readonly ItemAttributes _itemAttributes;
        private readonly ItemSlot _slot;

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

        public int NumberOfSockets
            => _itemAttributes.GetItemInSlot(_slot)?.BaseType.MaximumNumberOfSockets ?? 0;

        private SocketedGemViewModel _newSocketedGem;
        /// <summary>
        /// Gets the currently edited gem that can be socketed into the item with AddGemCommand.
        /// </summary>
        public SocketedGemViewModel NewSocketedGem
        {
            get => _newSocketedGem;
            private set => SetProperty(ref _newSocketedGem, value);
        }

        public SocketedGemsEditingViewModel(
            SkillDefinitions skillDefinitions, ItemImageService itemImageService, ItemAttributes itemAttributes,
            ItemSlot slot)
        {
            _itemAttributes = itemAttributes;
            _slot = slot;
            AvailableGems = skillDefinitions.Skills
                .Where(d => d.BaseItem != null)
                .Where(d => d.BaseItem.ReleaseState == ReleaseState.Released ||
                            d.BaseItem.ReleaseState == ReleaseState.Legacy)
                .OrderBy(d => d.BaseItem.DisplayName)
                .Select(d => new GemBaseViewModel(itemImageService, d)).ToList();
            NewSocketedGem = new SocketedGemViewModel
            {
                GemBase = AvailableGems[0],
                Level = 20,
                Quality = 0,
                Group = 1,
                IsEnabled = true,
            };
            AddGemCommand = new RelayCommand(AddGem);
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
            foreach (var skill in _itemAttributes.GetSkillsInSlot(_slot))
            {
                var gemBase = AvailableGems.FirstOrDefault(g => g.Id == skill.Id);
                if (gemBase == null)
                {
                    continue;
                }
                var socketedGem = new SocketedGemViewModel
                {
                    GemBase = gemBase,
                    Level = skill.Level,
                    Quality = skill.Quality,
                    Group = skill.GemGroup + 1,
                    IsEnabled = skill.IsEnabled,
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
                var skills = new List<Skill>();
                for (var i = 0; i < _socketedGems.Count; i++)
                {
                    var gem = _socketedGems[i];
                    var skill = new Skill(gem.GemBase.Id, gem.Level, gem.Quality, _slot, i, gem.Group - 1,
                        gem.IsEnabled);
                    skills.Add(skill);
                }
                _itemAttributes.SetSkillsInSlot(skills, _slot);
            }
            base.OnClose(param);
        }
    }
}