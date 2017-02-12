using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using MB.Algodat;
using MoreLinq;
using POESKillTree.Common.ViewModels;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils;
using POESKillTree.Utils.Wpf;

namespace POESKillTree.ViewModels.Crafting
{
    public class CraftingViewModelProxy : BindingProxy<CraftingViewModel>
    {
    }

    public class CraftingViewModel : CloseableViewModel<bool>
    {
        private const string QualityModName = "Quality: +#%";

        private static readonly ItemGroup[] Groups = Enum.GetValues(typeof(ItemGroup))
            .Cast<ItemGroup>()
            .Except(new[] { ItemGroup.Unknown, ItemGroup.Gem })
            .ToArray();

        private static readonly ItemType[] Types = Enum.GetValues(typeof(ItemType))
            .Cast<ItemType>()
            .ToArray();

        private readonly IReadOnlyList<ItemType> _eligibleTypes;
        private readonly IReadOnlyList<ItemBase> _eligibleBases;
        private readonly IReadOnlyDictionary<ItemType, List<ItemBase>> _basesPerType;
        private readonly IReadOnlyDictionary<ItemGroup, List<ItemBase>> _basesPerGroup;

        private IReadOnlyList<ItemGroup> _groupList;
        public IReadOnlyList<ItemGroup> GroupList
        {
            get { return _groupList; }
            private set { SetProperty(ref _groupList, value); }
        }

        private ItemGroup _selectedGroup;
        public ItemGroup SelectedGroup
        {
            get { return _selectedGroup; }
            set { SetProperty(ref _selectedGroup, value, OnSelectedGroupChanged); }
        }

        private IReadOnlyList<ItemType> _typeList;
        public IReadOnlyList<ItemType> TypeList
        {
            get { return _typeList; }
            private set { SetProperty(ref _typeList, value); }
        }

        private ItemType _selectedType;
        public ItemType SelectedType
        {
            get { return _selectedType; }
            set { SetProperty(ref _selectedType, value, UpdateBaseList); }
        }

        private IReadOnlyList<ItemBase> _baseList;
        public IReadOnlyList<ItemBase> BaseList
        {
            get { return _baseList; }
            private set { SetProperty(ref _baseList, value); }
        }

        private ItemBase _selectedBase;
        public ItemBase SelectedBase
        {
            get { return _selectedBase; }
            set { SetProperty(ref _selectedBase, value, UpdateBase); }
        }

        public ModSelectorViewModel MsQuality { get; } = new ModSelectorViewModel(false);
        public ModSelectorViewModel MsImplicits { get; } = new ModSelectorViewModel(false);
        public IReadOnlyList<ModSelectorViewModel> MsPrefix { get; } = new[] {
            new ModSelectorViewModel(), new ModSelectorViewModel(), new ModSelectorViewModel()
        };
        public IReadOnlyList<ModSelectorViewModel> MsSuffix { get; } = new[] {
            new ModSelectorViewModel(), new ModSelectorViewModel(), new ModSelectorViewModel()
        };

        private Item _item;
        public Item Item
        {
            get { return _item; }
            set { SetProperty(ref _item, value); }
        }

        private List<Affix> _prefixes = new List<Affix>();
        private List<Affix> _suffixes = new List<Affix>();

        private readonly SimpleMonitor _monitor = new SimpleMonitor();

        private readonly EquipmentData _equipmentData;

        private bool _changingBase;

        public CraftingViewModel(EquipmentData equipmentData)
        {
            _monitor.Freed += (sender, args) => RecalculateItem();
            _equipmentData = equipmentData;

            _eligibleBases = _equipmentData.BaseList
                .Where(b => !b.DropDisabled)
                .Where(b => _equipmentData.AffixesPerItemType.ContainsKey(b.ItemType)).ToList();
            _basesPerGroup = _eligibleBases.GroupBy(b => b.ItemGroup)
                .ToDictionary(g => g.Key, g => new List<ItemBase>(g));
            _basesPerType = _eligibleBases.GroupBy(b => b.ItemType)
                .ToDictionary(g => g.Key, g => new List<ItemBase>(g));
            _eligibleTypes = Types.Where(t => t == ItemType.Any || _basesPerType.ContainsKey(t)).ToList();

            MsQuality.PropertyChanged += MsOnPropertyChanged;
            MsImplicits.PropertyChanged += MsOnPropertyChanged;
            MsPrefix.ForEach(ms => ms.PropertyChanged += MsPrefixOnPropertyChanged);
            MsSuffix.ForEach(ms => ms.PropertyChanged += MsSuffixOnPropertyChanged);

            GroupList = Groups.Where(g => g == ItemGroup.Any || _basesPerGroup.ContainsKey(g)).ToList();
            SelectedGroup = GroupList[0];
        }

        private void OnSelectedGroupChanged()
        {
            using (_monitor.Enter())
            {
                var previousSelectedType = SelectedType;

                if (SelectedGroup == ItemGroup.Any)
                {
                    TypeList = new[] { ItemType.Any };
                }
                else
                {
                    var list = _eligibleTypes
                        .Where(t => t == ItemType.Any || t.Group() == SelectedGroup)
                        .ToArray();
                    if (list.Length == 2)
                    {
                        // Only contains "any" and the only type of the selected group
                        // -> "any" makes no sense, take the only type
                        list = new[] { list[1] };
                    }
                    TypeList = list;
                }
                SelectedType = TypeList[0];

                if (SelectedType == previousSelectedType)
                {
                    UpdateBaseList();
                }
            }
        }

        private void UpdateBaseList()
        {
            using (_monitor.Enter())
            {
                var previousSelectedBase = SelectedBase;

                if (SelectedType == ItemType.Any)
                {
                    BaseList = SelectedGroup == ItemGroup.Any ? _eligibleBases : _basesPerGroup[SelectedGroup];
                }
                else
                {
                    BaseList = _basesPerType[SelectedType];
                }
                SelectedBase = BaseList[0];

                if (SelectedBase == previousSelectedBase)
                {
                    UpdateBase();
                }
            }
        }

        private void UpdateBase()
        {
            if (SelectedBase == null)
            {
                return;
            }

            var d = _monitor.Enter();

            var ibase = SelectedBase;
            Item = ibase.CreateItem();

            if (ibase.ImplicitMods.Any())
            {
                MsImplicits.Affixes = new[]
                {
                    new Affix(new ItemModTier(ibase.ImplicitMods))
                };
            }
            else
            {
                MsImplicits.Affixes = null;
            }
            if (ibase.CanHaveQuality)
            {
                var qualityStat = new Stat(QualityModName, new Range<float>(0, 20), Item.ItemType, null);
                MsQuality.Affixes = new[]
                {
                    new Affix(new ItemModTier(new[] { qualityStat }))
                };
            }
            else
            {
                MsQuality.Affixes = null;
            }

            var aaff = _equipmentData.AffixesPerItemType[Item.ItemType].ToArray();

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

            d.Dispose();
        }

        private void RecalculateItem()
        {
            if (_monitor.IsBusy)
                return;

            Item.NameLine = "";
            Item.TypeLine = Item.BaseType.Name;

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

            var prefixes = selectedPreff.Select(p => p.GetExactMods()).SelectMany(m => m).ToList();
            var suffixes = selectedSuff.Select(p => p.GetExactMods()).SelectMany(m => m).ToList();
            var allmods = prefixes.Concat(suffixes)
                .GroupBy(m => m.Attribute)
                .Select(g => g.Aggregate((m1, m2) => m1.Sum(m2)))
                .ToList();

            Item.ExplicitMods = allmods.Where(m => m.ParentTier == null || !m.ParentTier.IsMasterCrafted).ToList();
            Item.CraftedMods = allmods.Where(m => m.ParentTier != null && m.ParentTier.IsMasterCrafted).ToList();
            Item.ImplicitMods = MsImplicits.GetExactMods().ToList();

            var quality = (int)MsQuality.SelectedValues.FirstOrDefault();
            Item.Properties = new ObservableCollection<ItemMod>(Item.BaseType.GetRawProperties(quality));
            ApplyLocals();

            if (Item.IsWeapon)
            {
                ApplyElementalMods(allmods);
            }

            Item.FlavourText = "Crafted by PoESkillTree";
            Item.UpdateRequirements();
        }

        private void ApplyElementalMods(IEnumerable<ItemMod> allMods)
        {
            var elementalMods = new List<ItemMod>();
            var chaosMods = new List<ItemMod>();
            foreach (var mod in allMods)
            {
                var attr = mod.Attribute;
                if (attr.StartsWith("Adds"))
                {
                    if (attr.Contains("Fire") || attr.Contains("Cold") || attr.Contains("Lightning"))
                    {
                        elementalMods.Add(mod);
                    }
                    if (attr.Contains("Chaos"))
                    {
                        chaosMods.Add(mod);
                    }
                }
            }

            if (elementalMods.Any())
            {
                var values = new List<float>();
                var mods = new List<string>();
                var cols = new List<ItemMod.ValueColoring>();

                var fmod = elementalMods.FirstOrDefault(m => m.Attribute.Contains("Fire"));
                if (fmod != null)
                {
                    values.AddRange(fmod.Value);
                    mods.Add("#-#");
                    cols.Add(ItemMod.ValueColoring.Fire);
                    cols.Add(ItemMod.ValueColoring.Fire);
                }

                var cmod = elementalMods.FirstOrDefault(m => m.Attribute.Contains("Cold"));
                if (cmod != null)
                {
                    values.AddRange(cmod.Value);
                    mods.Add("#-#");
                    cols.Add(ItemMod.ValueColoring.Cold);
                    cols.Add(ItemMod.ValueColoring.Cold);
                }

                var lmod = elementalMods.FirstOrDefault(m => m.Attribute.Contains("Lightning"));
                if (lmod != null)
                {
                    values.AddRange(lmod.Value);
                    mods.Add("#-#");
                    cols.Add(ItemMod.ValueColoring.Lightning);
                    cols.Add(ItemMod.ValueColoring.Lightning);
                }

                Item.Properties.Add(new ItemMod(Item.ItemType, "Elemental Damage: " + string.Join(", ", mods))
                {
                    Value = values,
                    ValueColor = cols,
                });
            }

            if (chaosMods.Any())
            {
                Item.Properties.Add(new ItemMod(Item.ItemType, "Chaos Damage: #-#")
                {
                    Value = new List<float>(chaosMods[0].Value),
                    ValueColor = new List<ItemMod.ValueColoring> { ItemMod.ValueColoring.Chaos, ItemMod.ValueColoring.Chaos },
                });
            }
        }

        private void ApplyLocals()
        {
            foreach (var pair in Item.GetModsAffectingProperties())
            {
                var prop = pair.Key;
                var applymods = pair.Value;

                var percm = applymods.Where(m => Regex.IsMatch(m.Attribute, @"(?<!\+)#%")).ToList();
                var valuem = applymods.Except(percm).ToList();

                if (valuem.Count > 0)
                {
                    var val = valuem.Select(m => m.Value).Aggregate((l1, l2) => l1.Zip(l2, (f1, f2) => f1 + f2).ToList());
                    var nval = prop.Value.Zip(val, (f1, f2) => f1 + f2).ToList();
                    prop.ValueColor = prop.ValueColor.Select((c, i) => val[i] == nval[i] ? prop.ValueColor[i] : ItemMod.ValueColoring.LocallyAffected).ToList();
                    prop.Value = nval;
                }

                Func<float, float> roundf = val => (float)Math.Round(val);

                if (prop.Attribute.Contains("Critical"))
                {
                    roundf = f => (float)(Math.Round(f * 10) / 10);
                }
                else if (prop.Attribute.Contains("per Second"))
                {
                    roundf = f => (float)(Math.Round(f * 100) / 100);
                }

                if (percm.Count > 0)
                {
                    var perc = 1f + percm.Select(m => m.Value[0]).Sum() / 100f;
                    prop.ValueColor = prop.ValueColor.Select(c => ItemMod.ValueColoring.LocallyAffected).ToList();
                    prop.Value = prop.Value.Select(v => roundf(v * perc)).ToList();
                }
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

            using (_monitor.Enter())
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

            using (_monitor.Enter())
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

        private void MsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModSelectorViewModel.SelectedValues))
            {
                RecalculateItem();
            }
        }
    }
}