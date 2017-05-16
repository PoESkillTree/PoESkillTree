using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MoreLinq;
using POESKillTree.Common.ViewModels;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Model.Items.Mods;
using POESKillTree.Model.Items.StatTranslation;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Crafting
{
    /// <summary>
    /// Base view model for crafting items from a list of bases. Contains all functionality not specific to the type
    /// of the base.
    /// </summary>
    /// <typeparam name="TBase">Type of crafted bases.</typeparam>
    public abstract class AbstractCraftingViewModel<TBase> : CloseableViewModel<bool>
        where TBase : class, IItemBase
    {
        private readonly IReadOnlyList<TBase> _bases;
        private IEnumerable<TBase> EligibleBases => _bases
            .Where(b => ShowDropDisabledItems || !b.DropDisabled);

        private ILookup<ItemType, TBase> BasesPerType => null;// todo EligibleBases.ToLookup(b => b.ItemType);
        private IEnumerable<ItemType> EligibleTypes
            => ItemType.Any.Concat(BasesPerType.Select(t => t.Key)).OrderBy(t => t);

        private ILookup<ItemGroup, TBase> BasesPerGroup => null;// todo EligibleBases.ToLookup(b => b.ItemGroup);
        private IEnumerable<ItemGroup> EligibleGroups
            => ItemGroup.Any.Concat(BasesPerGroup.Select(g => g.Key)).OrderBy(g => g);

        private bool _showDropDisabledItems;
        public bool ShowDropDisabledItems
        {
            get { return _showDropDisabledItems; }
            set { SetProperty(ref _showDropDisabledItems, value, OnShowDropDisabledItemsChanged);}
        }

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

        private IReadOnlyList<TBase> _baseList;
        public IReadOnlyList<TBase> BaseList
        {
            get { return _baseList; }
            private set { SetProperty(ref _baseList, value); }
        }

        private TBase _selectedBase;
        public TBase SelectedBase
        {
            get { return _selectedBase; }
            set { SetProperty(ref _selectedBase, value, UpdateBase); }
        }

        private Item _item;

        public Item Item
        {
            get { return _item; }
            private set { SetProperty(ref _item, value); }
        }

        private IReadOnlyList<ModSelectorViewModel> _msImplicits;
        public IReadOnlyList<ModSelectorViewModel> MsImplicits
        {
            get { return _msImplicits; }
            private set { SetProperty(ref _msImplicits, value); }
        }

        private readonly SliderViewModel _qualitySlider;
        public SliderGroupViewModel QualitySliderGroup { get; }

        private bool _showQualitySlider;
        public bool ShowQualitySlider
        {
            get { return _showQualitySlider; }
            private set { SetProperty(ref _showQualitySlider, value); }
        }

        protected SimpleMonitor Monitor { get; } = new SimpleMonitor();

        protected EquipmentData EquipmentData { get; }

        protected AbstractCraftingViewModel(EquipmentData equipmentData, IEnumerable<TBase> bases)
        {
            EquipmentData = equipmentData;
            _bases = bases.ToList();
            Monitor.Freed += (sender, args) => RecalculateItem();

            var qualityStat = new FormatTranslation("Quality: +{0}%");
            _qualitySlider = new SliderViewModel(0, Enumerable.Range(0, 21));
            QualitySliderGroup = new SliderGroupViewModel(new[] { _qualitySlider }, qualityStat);
            _qualitySlider.ValueChanged += QualitySliderOnValueChanged;
        }

        protected void Init()
        {
            GroupList = EligibleGroups.ToList();
            SelectedGroup = GroupList[0];
        }

        private void OnShowDropDisabledItemsChanged()
        {
            using (Monitor.Enter())
            {
                GroupList = EligibleGroups.ToList();
                SelectedGroup = GroupList[0];

                // this is visibly slow, but the button shouldn't be spammed anyway
                OnSelectedGroupChanged();
                UpdateBaseList();
                UpdateBase();
            }
        }

        private void OnSelectedGroupChanged()
        {
            using (Monitor.Enter())
            {
                var previousSelectedType = SelectedType;

                if (SelectedGroup == ItemGroup.Any)
                {
                    TypeList = new[] { ItemType.Any };
                }
                else
                {
                    var list = EligibleTypes
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
            using (Monitor.Enter())
            {
                var previousSelectedBase = SelectedBase;

                if (SelectedType == ItemType.Any)
                {
                    BaseList = SelectedGroup == ItemGroup.Any 
                        ? EligibleBases.ToList() : BasesPerGroup[SelectedGroup].ToList();
                }
                else
                {
                    BaseList = BasesPerType[SelectedType].ToList();
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

            using (Monitor.Enter())
            {
                var ibase = SelectedBase;
                Item = new Item(ibase);

                MsImplicits.ForEach(ms => ms.PropertyChanged -= MsOnPropertyChanged);
                var modSelectors = new List<ModSelectorViewModel>();
                foreach (var implicitMod in ibase.ImplicitMods)
                {
                    var modSelector = new ModSelectorViewModel(EquipmentData.StatTranslator, false)
                    {
                        Affixes = new[]
                        {
                            new Affix(implicitMod)
                        }
                    };
                    modSelector.PropertyChanged += MsOnPropertyChanged;
                }
                MsImplicits = modSelectors;

                ShowQualitySlider = ibase.CanHaveQuality;

                UpdateBaseSpecific();
            }
        }

        /// <summary>
        /// Set up mod selectors for mods specific to <see cref="TBase"/>.
        /// </summary>
        protected abstract void UpdateBaseSpecific();

        protected void RecalculateItem()
        {
            if (Monitor.IsBusy)
                return;

            Item.NameLine = "";
            Item.TypeLine = Item.BaseType.Name;
            Item.FlavourText = "Created with PoESkillTree";

            int requiredLevel;
            var statLookup = RecalculateItemSpecific(out requiredLevel).ToList();

            Item.ExplicitMods = CreateItemMods(ModLocation.Explicit, 
                statLookup.Single(g => g.Key == ModLocation.Explicit)).ToList();
            Item.CraftedMods = CreateItemMods(ModLocation.Crafted, 
                statLookup.Single(g => g.Key == ModLocation.Crafted)).ToList();
            Item.ImplicitMods = CreateItemMods(ModLocation.Implicit, 
                MsImplicits.SelectMany(ms => ms.GetStatValues())).ToList();

            var quality = SelectedBase.CanHaveQuality 
                ? _qualitySlider.Value
                : 0;
            Item.Properties = new ObservableCollection<ItemMod>(Item.BaseType.GetRawProperties(quality));
            ApplyLocals();

            if (Item.IsWeapon)
            {
                ApplyElementalMods(Item.Mods);
            }

            var implicits = MsImplicits.Select(ms => ms.Query());
            requiredLevel = Math.Max(requiredLevel, implicits.Max(m => m.RequiredLevel));
            Item.UpdateRequirements((80 * requiredLevel) / 100);
        }

        /// <summary>
        /// (Re)calculate parts of the item in crafting specific to <see cref="TBase"/>.
        /// </summary>
        /// <param name="requiredLevel">the highest <see cref="IMod.RequiredLevel"/> of any selected mod</param>
        /// <returns>All explicit and crafted stats of the item and their values (the lookup has entries for explicit
        /// and crafted and the entries are the stat ids and their values.</returns>
        protected abstract IEnumerable<IGrouping<ModLocation, StatIdValuePair>> RecalculateItemSpecific(out int requiredLevel);

        private IEnumerable<ItemMod> CreateItemMods(ModLocation location, IEnumerable<StatIdValuePair> statValuePairs)
        {
            // this list is used to translate the stats in order of appearance,
            // using merged.Keys wouldn't guarantee that
            var statIds = new List<string>();
            var merged = new Dictionary<string, int>();
            foreach (var pair in statValuePairs)
            {
                var stat = pair.StatId;
                statIds.Add(stat);
                var value = pair.Value;
                if (merged.ContainsKey(stat))
                {
                    value += merged[stat];
                }
                merged[stat] = value;
            }

            var lines = EquipmentData.StatTranslator.GetTranslations(statIds).Select(t => t.Translate(merged));
            foreach (var line in lines)
            {
                var isLocal = StatLocalityChecker.DetermineLocal(SelectedBase.ItemClass, location, line);
                yield return new ItemMod(line, isLocal);
            }
        }

        private void ApplyElementalMods(IEnumerable<ItemMod> allMods)
        {
            var elementalMods = new List<ItemMod>();
            var chaosMods = new List<ItemMod>();
            foreach (var mod in allMods)
            {
                string attr = mod.Attribute;
                if (attr.StartsWith("Adds") && !attr.Contains("in Main Hand") && !attr.Contains("in Off Hand"))
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
                    values.AddRange(fmod.Values);
                    mods.Add("#-#");
                    cols.Add(ItemMod.ValueColoring.Fire);
                    cols.Add(ItemMod.ValueColoring.Fire);
                }

                var cmod = elementalMods.FirstOrDefault(m => m.Attribute.Contains("Cold"));
                if (cmod != null)
                {
                    values.AddRange(cmod.Values);
                    mods.Add("#-#");
                    cols.Add(ItemMod.ValueColoring.Cold);
                    cols.Add(ItemMod.ValueColoring.Cold);
                }

                var lmod = elementalMods.FirstOrDefault(m => m.Attribute.Contains("Lightning"));
                if (lmod != null)
                {
                    values.AddRange(lmod.Values);
                    mods.Add("#-#");
                    cols.Add(ItemMod.ValueColoring.Lightning);
                    cols.Add(ItemMod.ValueColoring.Lightning);
                }

                Item.Properties.Add(new ItemMod("Elemental Damage: " + string.Join(", ", mods), true, values, cols));
            }

            if (chaosMods.Any())
            {
                Item.Properties.Add(new ItemMod("Chaos Damage: #-#", true, chaosMods[0].Values,
                    new[] { ItemMod.ValueColoring.Chaos, ItemMod.ValueColoring.Chaos }));
            }
        }

        private void ApplyLocals()
        {
            foreach (var pair in Item.GetModsAffectingProperties())
            {
                ItemMod prop = pair.Key;
                List<ItemMod> applymods = pair.Value;

                List<ItemMod> percm = applymods.Where(m => Regex.IsMatch(m.Attribute, @"(?<!\+)#%")).ToList();
                List<ItemMod> valuem = applymods.Except(percm).ToList();

                if (valuem.Count > 0)
                {
                    IReadOnlyList<float> val = valuem
                        .Select(m => m.Values)
                        .Aggregate((l1, l2) => l1.Zip(l2, (f1, f2) => f1 + f2)
                        .ToList());
                    IReadOnlyList<float> nval = prop.Values
                        .Zip(val, (f1, f2) => f1 + f2)
                        .ToList();
                    prop.ValueColors = prop.ValueColors
                        .Select((c, i) => val[i] == nval[i] ? prop.ValueColors[i] : ItemMod.ValueColoring.LocallyAffected)
                        .ToList();
                    prop.Values = nval;
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
                    var perc = 1f + percm.Select(m => m.Values[0]).Sum() / 100f;
                    prop.ValueColors = prop.ValueColors.Select(c => ItemMod.ValueColoring.LocallyAffected).ToList();
                    prop.Values = prop.Values.Select(v => roundf(v * perc)).ToList();
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

        private void QualitySliderOnValueChanged(object sender, SliderValueChangedEventArgs e)
        {
            RecalculateItem();
        }


        private class FormatTranslation : ITranslation
        {
            private readonly string _format;

            public FormatTranslation(string format)
            {
                _format = format;
            }

            public string Translate(IReadOnlyList<int> values)
            {
                if (values.Count != 1)
                {
                    throw new ArgumentException("Number of values does not match number of ranges");
                }
                return string.Format(CultureInfo.InvariantCulture,_format, values[0]);
            }
        }

    }
}
