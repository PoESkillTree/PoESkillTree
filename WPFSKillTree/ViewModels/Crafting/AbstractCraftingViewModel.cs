using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using EnumsNET;
using MoreLinq;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Modifiers;
using PoESkillTree.GameModel.StatTranslation;
using POESKillTree.Common.ViewModels;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Mods;
using POESKillTree.Utils;
using Item = POESKillTree.Model.Items.Item;

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
        private bool _showDropDisabledItems;
        public bool ShowDropDisabledItems
        {
            get { return _showDropDisabledItems; }
            set { SetProperty(ref _showDropDisabledItems, value, OnShowDropDisabledItemsChanged); }
        }

        // Bases are filtered in three levels:
        // 1. BaseGroup
        // 2. ItemClass
        // 3. specific Tags, i.e. Str/Dex/Int for armour and jewels
        // All levels also support not being filtered (Any/Default)

        // Bases

        private readonly IReadOnlyList<TBase> _bases;
        private IEnumerable<TBase> EligibleBases
            => _bases.Where(b => ShowDropDisabledItems || !b.DropDisabled);

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

        // First level

        public IReadOnlyList<BaseGroup> FirstLevelList { get; } = Enums.GetValues<BaseGroup>().ToList();

        private BaseGroup _selectedFirstLevel;
        public BaseGroup SelectedFirstLevel
        {
            get { return _selectedFirstLevel; }
            set { SetProperty(ref _selectedFirstLevel, value, UpdateSecondLevel); }
        }

        // Second level

        private IReadOnlyList<ItemClass> _secondLevelList;
        public IReadOnlyList<ItemClass> SecondLevelList
        {
            get { return _secondLevelList; }
            private set { SetProperty(ref _secondLevelList, value); }
        }

        private ItemClass _selectedSecondLevel;
        public ItemClass SelectedSecondLevel
        {
            get { return _selectedSecondLevel; }
            set { SetProperty(ref _selectedSecondLevel, value, UpdateThirdLevel); }
        }

        // Third level

        private readonly IEnumerable<Tags> _thirdLevelOptions = new[]
        {
            Tags.StrArmour, Tags.DexArmour, Tags.IntArmour, 
            Tags.StrDexArmour, Tags.StrIntArmour, Tags.DexIntArmour, 
            Tags.StrDexIntArmour,
            Tags.StrJewel, Tags.DexJewel, Tags.IntJewel
        };

        private IReadOnlyList<Tags> _thirdLevelList;
        public IReadOnlyList<Tags> ThirdLevelList
        {
            get { return _thirdLevelList; }
            private set { SetProperty(ref _thirdLevelList, value); }
        }

        private Tags _selectedThirdLevel;
        public Tags SelectedThirdLevel
        {
            get { return _selectedThirdLevel; }
            set { SetProperty(ref _selectedThirdLevel, value, UpdateBaseList); }
        }


        private Item _item;

        public Item Item
        {
            get { return _item; }
            private set { SetProperty(ref _item, value); }
        }

        private IReadOnlyList<ModSelectorViewModel> _msImplicits = new ModSelectorViewModel[0];
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
            // setting the underlying field directly, setting other properties is initiated in Init()
            _selectedFirstLevel = FirstLevelList[0];
            Monitor.Freed += (sender, args) => RecalculateItem();

            var qualityStat = new FormatTranslation("Quality: +{0}%");
            _qualitySlider = new SliderViewModel(0, Enumerable.Range(0, 21));
            QualitySliderGroup = new SliderGroupViewModel(new[] { _qualitySlider }, qualityStat);
            _qualitySlider.ValueChanged += QualitySliderOnValueChanged;
        }

        protected void Init()
        {
            UpdateSecondLevel();
        }

        private void OnShowDropDisabledItemsChanged()
        {
            using (Monitor.Enter())
            {
                // this is visibly slow, but the button shouldn't be spammed anyway
                var previousSelectedFirstLevel = SelectedFirstLevel;
                SelectedFirstLevel = FirstLevelList[0];
                if (previousSelectedFirstLevel == SelectedFirstLevel)
                {
                    UpdateSecondLevel();
                }
            }
        }

        private void UpdateSecondLevel()
        {
            using (Monitor.Enter())
            {
                var previousSecondLevel = SelectedSecondLevel;

                if (SelectedFirstLevel == BaseGroup.Any)
                {
                    SecondLevelList = EligibleBases.Select(b => b.ItemClass)
                        .Prepend(ItemClass.Any)
                        .Distinct()
                        .OrderBy(c => c).ToList();
                }
                else
                {
                    var list = EligibleBases
                        .Where(b => BaseGroupEx.FromTags(b.Tags) == SelectedFirstLevel)
                        .Select(b => b.ItemClass)
                        .Distinct()
                        .OrderBy(c => c).ToList();
                    switch (list.Count)
                    {
                        case 1:
                            // Only contains the only class of the selected group
                            // -> "any" makes no sense
                            SecondLevelList = list;
                            break;
                        default:
                            SecondLevelList = list.Prepend(ItemClass.Any).ToList();
                            break;
                    }
                }
                SelectedSecondLevel = SecondLevelList[0];

                if (previousSecondLevel == SelectedSecondLevel)
                {
                    UpdateThirdLevel();
                }
            }
        }

        private void UpdateThirdLevel()
        {
            var previousThirdLevel = SelectedThirdLevel;

            if (SelectedSecondLevel == ItemClass.Any)
            {
                ThirdLevelList = new[] { Tags.Default };
            }
            else
            {
                var matchingBases = EligibleBases
                    .Where(b => b.ItemClass == SelectedSecondLevel)
                    .ToList();
                var list = _thirdLevelOptions
                    .Where(t => matchingBases.Any(b => b.Tags.HasFlag(t)))
                    .ToList();
                switch (list.Count)
                {
                    case 0:
                        ThirdLevelList = new[] { Tags.Default };
                        break;
                    case 1:
                        ThirdLevelList = list;
                        break;
                    default:
                        ThirdLevelList = list.Prepend(Tags.Default).ToList();
                        break;
                }
            }
            SelectedThirdLevel = ThirdLevelList[0];

            if (previousThirdLevel == SelectedThirdLevel)
            {
                UpdateBaseList();
            }
        }

        private void UpdateBaseList()
        {
            using (Monitor.Enter())
            {
                var previousSelectedBase = SelectedBase;

                var bases = EligibleBases;
                if (SelectedFirstLevel != BaseGroup.Any)
                {
                    bases = bases.Where(b => SelectedFirstLevel.Matches(b.Tags));
                }
                if (SelectedSecondLevel != ItemClass.Any)
                {
                    bases = bases.Where(b => b.ItemClass == SelectedSecondLevel);
                }
                if (SelectedThirdLevel != Tags.Default)
                {
                    bases = bases.Where(b => b.Tags.HasFlag(SelectedThirdLevel));
                }
                BaseList = bases.ToList();
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
                    modSelectors.Add(modSelector);
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
                statLookup.SingleOrDefault(g => g.Key == ModLocation.Explicit)).ToList();
            Item.CraftedMods = CreateItemMods(ModLocation.Crafted, 
                statLookup.SingleOrDefault(g => g.Key == ModLocation.Crafted)).ToList();
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

            if (MsImplicits.Any())
            {
                var implicits = MsImplicits.Select(ms => ms.Query());
                requiredLevel = Math.Max(requiredLevel, implicits.Max(m => m.RequiredLevel));
            }
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
            if (statValuePairs == null)
            {
                yield break;
            }

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

            var lines = EquipmentData.StatTranslator.GetTranslations(statIds)
                .Select(t => t.Translate(merged))
                .Where(l => l != null);
            foreach (var line in lines)
            {
                var attr = ItemMod.Numberfilter.Replace(line, "#");
                var isLocal = ModifierLocalityTester.IsLocal(attr, SelectedBase.Tags);
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
                var cols = new List<ValueColoring>();

                var fmod = elementalMods.FirstOrDefault(m => m.Attribute.Contains("Fire"));
                if (fmod != null)
                {
                    values.AddRange(fmod.Values);
                    mods.Add("#-#");
                    cols.Add(ValueColoring.Fire);
                    cols.Add(ValueColoring.Fire);
                }

                var cmod = elementalMods.FirstOrDefault(m => m.Attribute.Contains("Cold"));
                if (cmod != null)
                {
                    values.AddRange(cmod.Values);
                    mods.Add("#-#");
                    cols.Add(ValueColoring.Cold);
                    cols.Add(ValueColoring.Cold);
                }

                var lmod = elementalMods.FirstOrDefault(m => m.Attribute.Contains("Lightning"));
                if (lmod != null)
                {
                    values.AddRange(lmod.Values);
                    mods.Add("#-#");
                    cols.Add(ValueColoring.Lightning);
                    cols.Add(ValueColoring.Lightning);
                }

                Item.Properties.Add(new ItemMod("Elemental Damage: " + string.Join(", ", mods), true, values, cols));
            }

            if (chaosMods.Any())
            {
                Item.Properties.Add(new ItemMod("Chaos Damage: #-#", true, chaosMods[0].Values,
                    new[] { ValueColoring.Chaos, ValueColoring.Chaos }));
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
                        .Select((c, i) => val[i] == nval[i] ? prop.ValueColors[i] : ValueColoring.LocallyAffected)
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
                    prop.ValueColors = prop.ValueColors.Select(c => ValueColoring.LocallyAffected).ToList();
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


        /// <summary>
        /// ITranslation implementation that translates a single value with <see cref="string.Format(string,object)"/>.
        /// Used for the quality slider as the quality has no mod or stat behind it that could be translated.
        /// </summary>
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
                return string.Format(CultureInfo.InvariantCulture, _format, values[0]);
            }
        }

    }
}
