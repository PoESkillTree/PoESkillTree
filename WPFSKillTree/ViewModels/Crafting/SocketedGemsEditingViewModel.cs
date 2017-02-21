using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POESKillTree.Common.ViewModels;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Enums;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils.Wpf;

namespace POESKillTree.ViewModels.Crafting
{
    public class SocketedGem
    {
        public int Level { get; set; }

        public int Quality { get; set; }

        public int Group { get; set; }

        public GemBase GemBase { get; }

        public SocketedGem(GemBase gemBase)
        {
            GemBase = gemBase;
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
        public IReadOnlyList<GemBase> AvailableGems { get; }

        public ObservableCollection<SocketedGem> SocketedGems { get; }

        public int NumberOfSockets { get; } = 6;

        public ICommand AddGemCommand { get; set; }

        public ICommand RemoveGemCommand { get; set; }

        public SocketedGemsEditingViewModel(ItemImageService itemImageService)
        {
            var iis = itemImageService;

            var allGems = ItemDB.GetAllGems();
            AvailableGems = allGems.Select(g => new GemBase(iis, g)).ToList();

            SocketedGems = new ObservableCollection<SocketedGem>
            {
                new SocketedGem(Get("Added Fire Damage Support")) { Level = 20, Quality = 20, Group = 1 },
                new SocketedGem(Get("Lightning Arrow")) { Level = 21, Quality = 20, Group = 1 },
                new SocketedGem(Get("Greater Multiple Projectiles Support")) { Level = 18, Quality = 20, Group = 1 },
                new SocketedGem(Get("Faster Attacks Support")) { Level = 20, Quality = 23, Group = 1 },
                new SocketedGem(Get("Physical Projectile Attack Damage Support")) { Level = 19, Quality = 20, Group = 1 },
                new SocketedGem(Get("Blink Arrow")) { Level = 15, Quality = 0, Group = 2 },
                new SocketedGem(Get("Faster Attacks Support")) { Level = 19, Quality = 20, Group = 2 },
            };
        }

        private GemBase Get(string name)
        {
            return AvailableGems.First(g => g.Name == name);
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