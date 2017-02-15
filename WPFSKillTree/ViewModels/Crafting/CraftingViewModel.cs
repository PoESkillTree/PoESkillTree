using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MoreLinq;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils.Wpf;

namespace POESKillTree.ViewModels.Crafting
{
    public class CraftingViewModelProxy : BindingProxy<CraftingViewModel>
    {
    }

    public class CraftingViewModel : AbstractCraftingViewModel<ItemBase>
    {
        public IReadOnlyList<ModSelectorViewModel> MsPrefix { get; } = new[] {
            new ModSelectorViewModel(), new ModSelectorViewModel(), new ModSelectorViewModel()
        };
        public IReadOnlyList<ModSelectorViewModel> MsSuffix { get; } = new[] {
            new ModSelectorViewModel(), new ModSelectorViewModel(), new ModSelectorViewModel()
        };

        private List<Affix> _prefixes = new List<Affix>();
        private List<Affix> _suffixes = new List<Affix>();

        private bool _changingBase;

        public CraftingViewModel(EquipmentData equipmentData)
            : base(equipmentData, equipmentData.BaseList)
        {
            MsPrefix.ForEach(ms => ms.PropertyChanged += MsPrefixOnPropertyChanged);
            MsSuffix.ForEach(ms => ms.PropertyChanged += MsSuffixOnPropertyChanged);
        }

        protected override void UpdateBaseSpecific()
        {
            var aaff = EquipmentData.AffixesPerItemType[Item.ItemType];

            _prefixes = aaff.Where(a => a.ModType == ModType.Prefix).ToList();
            _suffixes = aaff.Where(a => a.ModType == ModType.Suffix).ToList();

            _changingBase = true;
            MsPrefix.ForEach(ms => ms.Affixes = _prefixes);
            foreach (var ms in MsPrefix)
            {
                var blockedPrefixes = MsPrefix.Where(m => m != ms).Select(m => m.SelectedAffix);
                ms.Affixes = _prefixes.Except(blockedPrefixes).ToList();
            }
            MsSuffix.ForEach(ms => ms.Affixes = _suffixes);
            foreach (var ms in MsSuffix)
            {
                var blockedPrefixes = MsSuffix.Where(m => m != ms).Select(m => m.SelectedAffix);
                ms.Affixes = _suffixes.Except(blockedPrefixes).ToList();
            }
            _changingBase = false;
        }

        protected override IEnumerable<ItemMod> RecalculateItemSpecific()
        {
            var selectedPreff = MsPrefix.Where(s => !s.IsEmptySelection).ToArray();
            var selectedSuff = MsSuffix.Where(s => !s.IsEmptySelection).ToArray();

            if (selectedPreff.Length + selectedSuff.Length == 0)
            {
                Item.Frame = FrameType.White;
            }
            else if (selectedPreff.Length <= 1 && selectedSuff.Length <= 1)
            {
                Item.Frame = FrameType.Magic;
                var typeline = "";

                if (selectedPreff.Length > 0)
                {
                    var pref = selectedPreff[0];
                    typeline = pref.SelectedAffix.Query(pref.SelectedValues).First().Name + " ";
                }

                typeline += Item.BaseType;

                if (selectedSuff.Length > 0)
                {
                    var suff = selectedSuff[0];
                    typeline += " " + suff.SelectedAffix.Query(suff.SelectedValues).First().Name;
                }

                Item.TypeLine = typeline;
            }
            else
            {
                Item.Frame = FrameType.Rare;
                Item.NameLine = "Crafted " + Item.BaseType;
            }

            var prefixes = selectedPreff.SelectMany(p => p.GetExactMods());
            var suffixes = selectedSuff.SelectMany(p => p.GetExactMods());
            return prefixes.Concat(suffixes)
                .GroupBy(m => m.Attribute)
                .Select(g => g.Aggregate((m1, m2) => m1.Sum(m2)));
        }

        private void MsPrefixOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_changingBase)
                return;

            if (e.PropertyName == nameof(ModSelectorViewModel.SelectedValues))
            {
                RecalculateItem();
                return;
            }
            if (e.PropertyName != nameof(ModSelectorViewModel.SelectedAffix))
            {
                return;
            }

            using (Monitor.Enter())
            {
                var ms = (ModSelectorViewModel)sender;

                foreach (var other in MsPrefix)
                {
                    if (other != ms)
                    {
                        var blockedPrefixes = MsPrefix.Where(m => m != other).Select(m => m.SelectedAffix);
                        other.Affixes = _prefixes.Except(blockedPrefixes).ToList();
                    }
                }
            }
        }

        private void MsSuffixOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_changingBase)
                return;

            if (e.PropertyName == nameof(ModSelectorViewModel.SelectedValues))
            {
                RecalculateItem();
                return;
            }
            if (e.PropertyName != nameof(ModSelectorViewModel.SelectedAffix))
            {
                return;
            }

            using (Monitor.Enter())
            {
                var ms = (ModSelectorViewModel)sender;

                foreach (var other in MsSuffix)
                {
                    if (other != ms)
                    {
                        var blockedPrefixes = MsSuffix.Where(m => m != other).Select(m => m.SelectedAffix);
                        other.Affixes = _suffixes.Except(blockedPrefixes).ToList();
                    }
                }
            }
        }
    }
}