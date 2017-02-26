using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POESKillTree.Common.ViewModels;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Enums;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;
using POESKillTree.Utils.Wpf;

namespace POESKillTree.ViewModels.Crafting
{
    public class SocketedGem : Notifier
    {
        private int _level;
        private int _quality;
        private int _group;
        private GemBase _gemBase;

        public int Level
        {
            get { return _level; }
            set { SetProperty(ref _level, value); }
        }

        public int Quality
        {
            get { return _quality; }
            set { SetProperty(ref _quality, value); }
        }

        public int Group
        {
            get { return _group; }
            set { SetProperty(ref _group, value); }
        }

        public GemBase GemBase
        {
            get { return _gemBase; }
            set { SetProperty(ref _gemBase, value); }
        }

        public SocketedGem Clone()
        {
            return new SocketedGem
            {
                GemBase = GemBase,
                Group = Group,
                Quality = Quality,
                Level = Level
            };
        }
    }

    public class GemBase
    {
        public string Name { get; }

        public ItemImage Icon { get; }

        public ItemDB.Gem Gem { get; }

        public GemBase(ItemImageService itemImageService, ItemDB.Gem gem)
        {
            Gem = gem;
            Name = Gem.Name;
            Icon = new ItemImage(itemImageService, Name, ItemGroup.Gem);
        }
    }

    public class SocketedGemsEditingViewModelProxy : BindingProxy<SocketedGemsEditingViewModel>
    {
    }

    public class SocketedGemsEditingViewModel : CloseableViewModel<bool>
    {
        private readonly Item _itemWithSockets;

        public IReadOnlyList<GemBase> AvailableGems { get; }

        public ObservableCollection<SocketedGem> SocketedGems { get; } = new ObservableCollection<SocketedGem>();

        public ICommand AddGemCommand { get; set; }

        public ICommand RemoveGemCommand { get; set; }

        public int NumberOfSockets { get; }

        private SocketedGem _newSocketedGem;

        public SocketedGem NewSocketedGem
        {
            get { return _newSocketedGem; }
            private set { SetProperty(ref _newSocketedGem, value); }
        }

        public SocketedGemsEditingViewModel(ItemImageService itemImageService, Item itemWithSockets)
        {
            _itemWithSockets = itemWithSockets;
            AvailableGems = ItemDB.GetAllGems().Select(g => new GemBase(itemImageService, g)).ToList();
            NumberOfSockets = _itemWithSockets.BaseType.MaximumNumberOfSockets;
            NewSocketedGem = new SocketedGem
            {
                GemBase = AvailableGems[0],
                Level = 1,
                Quality = 0,
                Group = 1
            };
            AddGemCommand = new RelayCommand(AddGem, CanAddGem);
            RemoveGemCommand = new RelayCommand<SocketedGem>(RemoveGem);

            foreach (var gem in _itemWithSockets.Gems)
            {
                var gemBase = AvailableGems.FirstOrDefault(g => g.Name == gem.Name);
                if (gemBase == null)
                {
                    continue;
                }
                var socketedGem = new SocketedGem
                {
                    GemBase = gemBase,
                    Level = ItemDB.LevelOf(gem),
                    Quality = ItemDB.QualityOf(gem),
                    Group = gem.SocketGroup + 1
                };
                SocketedGems.Add(socketedGem);
            }
        }

        private void AddGem()
        {
            var addedGem = NewSocketedGem.Clone();
            var group = addedGem.Group;
            int i;
            for (i = 0; i < SocketedGems.Count; i++)
            {
                if (SocketedGems[i].Group > group)
                {
                    break;
                }
            }
            SocketedGems.Insert(i, addedGem);
        }

        private bool CanAddGem()
            => SocketedGems.Count < NumberOfSockets;

        private void RemoveGem(SocketedGem gem)
        {
            SocketedGems.Remove(gem);
            NewSocketedGem = gem;
        }

        protected override void OnClose(bool param)
        {
            if (param)
            {
                // todo edit item's gems   
            }
            base.OnClose(param);
        }
    }
}