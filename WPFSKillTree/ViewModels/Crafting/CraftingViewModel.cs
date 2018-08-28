using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MoreLinq;
using PoESkillTree.GameModel.Items;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Model.Items.Mods;
using POESKillTree.Utils.Wpf;

namespace POESKillTree.ViewModels.Crafting
{
    public class CraftingViewModelProxy : BindingProxy<CraftingViewModel>
    {
    }

    /// <summary>
    /// View model for crafting normal/magic/rare items.
    /// </summary>
    public class CraftingViewModel : AbstractCraftingViewModel<ItemBase>
    {
        public IReadOnlyList<ModSelectorViewModel> MsPrefix { get; }
        public IReadOnlyList<ModSelectorViewModel> MsSuffix { get; }

        private readonly IReadOnlyList<ModGroup> _allPrefixes;
        private readonly IReadOnlyList<ModGroup> _allSuffixes;

        private List<Affix> _prefixes = new List<Affix>();
        private List<Affix> _suffixes = new List<Affix>();

        private bool _changingBase;

        private string _nameLine;
        public string NameLine
        {
            get { return _nameLine; }
            set { SetProperty(ref _nameLine, value, UpdateItemNameLine); }
        }

        public CraftingViewModel(EquipmentData equipmentData)
            : base(equipmentData, SelectCraftableBases(equipmentData))
        {
            _allPrefixes = equipmentData.ModDatabase[ModGenerationType.Prefix];
            _allSuffixes = equipmentData.ModDatabase[ModGenerationType.Suffix];

            MsPrefix = new[] {
                new ModSelectorViewModel(EquipmentData.StatTranslator),
                new ModSelectorViewModel(EquipmentData.StatTranslator),
                new ModSelectorViewModel(EquipmentData.StatTranslator)
            };
            MsPrefix.ForEach(ms => ms.PropertyChanged += MsPrefixOnPropertyChanged);
            MsSuffix = new[] {
                new ModSelectorViewModel(EquipmentData.StatTranslator),
                new ModSelectorViewModel(EquipmentData.StatTranslator),
                new ModSelectorViewModel(EquipmentData.StatTranslator)
            };
            MsSuffix.ForEach(ms => ms.PropertyChanged += MsSuffixOnPropertyChanged);

            Init();
        }

        private static IEnumerable<ItemBase> SelectCraftableBases(EquipmentData equipmentData)
        {
            var prefixes = equipmentData.ModDatabase[ModGenerationType.Prefix];
            var suffixes = equipmentData.ModDatabase[ModGenerationType.Suffix];
            foreach (var itemBase in equipmentData.ItemBases)
            {
                if (prefixes.Any(p => p.GetMatchingMods(itemBase.Tags, itemBase.ItemClass).Any()))
                {
                    yield return itemBase;
                }
                else if (suffixes.Any(p => p.GetMatchingMods(itemBase.Tags, itemBase.ItemClass).Any()))
                {
                    yield return itemBase;
                }
            }
        }

        protected override void UpdateBaseSpecific()
        {
            _prefixes = CreateAffixes(_allPrefixes).ToList();
            _suffixes = CreateAffixes(_allSuffixes).ToList();

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
                var blockedSuffixes = MsSuffix.Where(m => m != ms).Select(m => m.SelectedAffix);
                ms.Affixes = _suffixes.Except(blockedSuffixes).ToList();
            }
            _changingBase = false;
        }

        private IEnumerable<Affix> CreateAffixes(IEnumerable<ModGroup> modGroups)
        {
            return
                from modGroup in modGroups
                let mods = modGroup.GetMatchingMods(SelectedBase.Tags, SelectedBase.ItemClass).ToList()
                where mods.Any()
                let name = GetNameForAffix(mods)
                select new Affix(mods, name);
        }

        private string GetNameForAffix(IReadOnlyList<IMod> mods)
        {
            if (!mods.Any())
            {
                return "";
            }
            var mod = mods.First();
            var statValueDict = mod.Stats.ToDictionary(s => s.Id, s => s.Range.From);
            var translations = EquipmentData.StatTranslator.GetTranslations(statValueDict)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => ItemMod.Numberfilter.Replace(s, "#"));
            return string.Join(", ", translations);
        }

        protected override IEnumerable<IGrouping<ModLocation, StatIdValuePair>> RecalculateItemSpecific(out int requiredLevel)
        {
            var selectedPrefixes = MsPrefix.Where(s => !s.IsEmptySelection).ToArray();
            var selectedSuffixes = MsSuffix.Where(s => !s.IsEmptySelection).ToArray();

            if (selectedPrefixes.Length + selectedSuffixes.Length == 0)
            {
                Item.Frame = FrameType.White;
            }
            else if (selectedPrefixes.Length <= 1 && selectedSuffixes.Length <= 1)
            {
                Item.Frame = FrameType.Magic;
                var typeline = "";

                if (selectedPrefixes.Length > 0)
                {
                    var pref = selectedPrefixes[0];
                    typeline = pref.Query().Name + " ";
                }

                typeline += Item.BaseType;

                if (selectedSuffixes.Length > 0)
                {
                    var suff = selectedSuffixes[0];
                    typeline += " " + suff.Query().Name;
                }

                Item.TypeLine = typeline;
            }
            else
            {
                Item.Frame = FrameType.Rare;
            }
            UpdateItemNameLine();

            requiredLevel = selectedPrefixes.Concat(selectedSuffixes)
                .Select(ms => ms.Query().RequiredLevel)
                .DefaultIfEmpty()
                .Max();

            return
                from ms in MsPrefix.Concat(MsSuffix)
                where !ms.IsEmptySelection
                let location = ms.Query().Domain == ModDomain.Crafted ? ModLocation.Crafted : ModLocation.Explicit
                from tuple in ms.GetStatValues()
                group tuple by location;
        }

        private void UpdateItemNameLine()
        {
            if (Item.Frame != FrameType.Rare)
            {
                Item.NameLine = "";
                return;
            }
            if (string.IsNullOrWhiteSpace(NameLine))
            {
                Item.NameLine = "Crafted " + Item.BaseType;
            }
            else
            {
                Item.NameLine = NameLine;
            }
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
                        var blockedSuffixes = MsSuffix.Where(m => m != other).Select(m => m.SelectedAffix);
                        other.Affixes = _suffixes.Except(blockedSuffixes).ToList();
                    }
                }
            }
        }
    }
}