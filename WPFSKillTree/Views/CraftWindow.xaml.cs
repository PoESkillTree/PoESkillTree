using POESKillTree.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using MB.Algodat;
using POESKillTree.Common.ViewModels;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils;

namespace POESKillTree.Views
{
    // Necessary because CraftWindow.Item is not accessable from content in BaseDialog.DialogLeft.
    public class CraftViewModel : CloseableViewModel
    {
        private Item _item;
        public Item Item
        {
            get { return _item; }
            set { SetProperty(ref _item, value); }
        }
    }
    
    /// <summary>
    /// Interaction logic for CraftWindow.xaml
    /// </summary>
    public partial class CraftWindow : INotifyPropertyChanged
    {
        private const string QualityModName = "Quality: +#%";

        private static readonly ItemGroup[] Groups = Enum.GetValues(typeof(ItemGroup))
            .Cast<ItemGroup>()
            .Except(new [] {ItemGroup.Unknown, ItemGroup.Gem})
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
            private set { _groupList = value; OnPropertyChanged(); }
        }

        private IReadOnlyList<ItemType> _typeList;
        public IReadOnlyList<ItemType> TypeList
        {
            get { return _typeList; }
            private set
            {
                _typeList = value;
                OnPropertyChanged();
            }
        }

        private IReadOnlyList<ItemBase> _blist;
        public IReadOnlyList<ItemBase> BaseList
        {
            get { return _blist; }
            private set { _blist = value; OnPropertyChanged(); }
        }

        public Item Item
        {
            get
            {
                var vm = DataContext as CraftViewModel;
                return vm == null ? null : ((CraftViewModel) DataContext).Item;
            }
            private set
            {
                ((CraftViewModel) DataContext).Item = value;
                OnPropertyChanged();
            }
        }

        private List<Affix> _prefixes = new List<Affix>();
        private List<Affix> _suffixes = new List<Affix>();

        private readonly SimpleMonitor _monitor = new SimpleMonitor();

        private readonly EquipmentData _equipmentData;

        public CraftWindow(EquipmentData equipmentData)
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

            InitializeComponent();
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            GroupList = Groups.Where(g => g == ItemGroup.Any || _basesPerGroup.ContainsKey(g)).ToArray();
        }

        private void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void GroupSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (_monitor.Enter())
            {
                var selected = TypeSelection.SelectedItem;
                var group = (ItemGroup) GroupSelection.SelectedItem;
                if (group == ItemGroup.Any)
                {
                    TypeList = new[] {ItemType.Any};
                }
                else
                {
                    var list = _eligibleTypes
                        .Where(t => t == ItemType.Any || t.Group() == group)
                        .ToArray();
                    if (list.Length == 2)
                    {
                        // Only contains "any" and the only type of the selected group
                        // -> "any" makes no sense, take the only type
                        list = new [] {list[1]};
                    }
                    TypeList = list;
                }
                TypeSelection.SelectedIndex = 0;

                if (TypeSelection.SelectedItem == selected)
                    UpdateBaseList();
            }
        }

        private void ClassSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Happens if called in GroupSelection_SelectionChanged
            if (TypeSelection.SelectedItem == null) return;
            UpdateBaseList();
        }

        private void UpdateBaseList()
        {
            using (_monitor.Enter())
            {
                var selected = BaseSelection.SelectedItem;
                var type = (ItemType) TypeSelection.SelectedItem;
                if (type == ItemType.Any)
                {
                    var group = (ItemGroup) GroupSelection.SelectedItem;
                    BaseList = group == ItemGroup.Any ? _eligibleBases : _basesPerGroup[group];
                }
                else
                {
                    BaseList = _basesPerType[type];
                }
                BaseSelection.SelectedIndex = 0;

                if (BaseSelection.SelectedItem == selected)
                    UpdateBase();
            }
        }

        private void BaseSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BaseSelection.SelectedItem == null)
            {
                Item = null;
                return;
            }
            UpdateBase();
        }

        private void UpdateBase()
        {
            var d = _monitor.Enter();
            msp1.Affixes = msp2.Affixes = msp3.Affixes = mss1.Affixes = mss1.Affixes = mss2.Affixes = mss3.Affixes = null;

            var ibase = (ItemBase)BaseSelection.SelectedItem;
            Item = ibase.CreateItem();

            if (ibase.ImplicitMods.Any())
            {
                msImplicitMods.Affixes = new List<Affix>
                {
                    new Affix(new[] { new ItemModTier(ibase.ImplicitMods) })
                };
                Item.ImplicitMods = msImplicitMods.GetExactMods().ToList();
                ApplyLocals();
            }
            else
            {
                msImplicitMods.Affixes = null;
            }
            if (ibase.CanHaveQuality)
            {
                var qualityStat = new Stat(QualityModName, new Range<float>(0, 20), Item.ItemType, null);
                var qualityAffix = new Affix(new[] {new ItemModTier(new[] {qualityStat})});
                MsQuality.Affixes = new List<Affix>(new[] {qualityAffix});
            }
            else
            {
                MsQuality.Affixes = null;
            }

            var aaff = _equipmentData.AffixesPerItemType[Item.ItemType].ToArray();

            _prefixes = aaff.Where(a => a.ModType == ModType.Prefix).ToList();
            _suffixes = aaff.Where(a => a.ModType == ModType.Suffix).ToList();

            msp1.Affixes = msp2.Affixes = msp3.Affixes = _prefixes;
            mss1.Affixes = mss2.Affixes = mss3.Affixes = _suffixes;
            msp3.Visibility = Item.ItemGroup == ItemGroup.Jewel ? Visibility.Hidden : Visibility.Visible;
            mss3.Visibility = Item.ItemGroup == ItemGroup.Jewel ? Visibility.Hidden : Visibility.Visible;

            d.Dispose();
        }

        private void msp_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModSelector.SelectedValues))
            {
                RecalculateItem();
                return;
            }
            if (e.PropertyName != nameof(ModSelector.SelectedAffix))
            {
                return;
            }

            var d = _monitor.Enter();
            var ms = (ModSelector) sender;

            if (msp1 != ms)
                msp1.Affixes = _prefixes.Except(new[] { msp2.SelectedAffix, msp3.SelectedAffix }).ToList();

            if (msp2 != ms)
                msp2.Affixes = _prefixes.Except(new[] { msp1.SelectedAffix, msp3.SelectedAffix }).ToList();

            if (msp3 != ms)
                msp3.Affixes = _prefixes.Except(new[] { msp2.SelectedAffix, msp1.SelectedAffix }).ToList();

            d.Dispose();
        }

        private void RecalculateItem()
        {
            if (_monitor.IsBusy)
                return;
            if (Item == null)
                return;

            Item.NameLine = "";
            Item.TypeLine = Item.BaseType.Name;

            var selectedPreff = new[] { msp1, msp2, msp3 }.Where(s => s.SelectedAffix != null).ToArray();
            var selectedSuff = new[] { mss1, mss2, mss3 }.Where(s => s.SelectedAffix != null).ToArray();

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

            if (msImplicitMods.Affixes != null)
            {
                Item.ImplicitMods = msImplicitMods.GetExactMods().ToList();
            }

            var quality = (int) MsQuality.SelectedValues.FirstOrDefault();
            Item.Properties = new ObservableCollection<ItemMod>(Item.BaseType.GetRawProperties(quality));
            ApplyLocals();

            if (Item.IsWeapon)
            {
                var elementalmods = allmods.Where(m => m.Attribute.StartsWith("Adds") && (m.Attribute.Contains("Fire") || m.Attribute.Contains("Cold") || m.Attribute.Contains("Lightning"))).ToList();
                if (elementalmods.Count > 0)
                {
                    var values = new List<float>();
                    var mods = new List<string>();
                    var cols = new List<ItemMod.ValueColoring>();

                    var fmod = elementalmods.FirstOrDefault(m => m.Attribute.Contains("Fire"));
                    if (fmod != null)
                    {
                        values.AddRange(fmod.Value);
                        mods.Add("#-#");
                        cols.Add(ItemMod.ValueColoring.Fire);
                        cols.Add(ItemMod.ValueColoring.Fire);
                    }

                    var cmod = elementalmods.FirstOrDefault(m => m.Attribute.Contains("Cold"));
                    if (cmod != null)
                    {
                        values.AddRange(cmod.Value);
                        mods.Add("#-#");
                        cols.Add(ItemMod.ValueColoring.Cold);
                        cols.Add(ItemMod.ValueColoring.Cold);
                    }

                    var lmod = elementalmods.FirstOrDefault(m => m.Attribute.Contains("Lightning"));
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

                var chaosMods = allmods.Where(m => m.Attribute.StartsWith("Adds") && m.Attribute.Contains("Chaos")).ToList();
                if (chaosMods.Count > 0)
                {
                    Item.Properties.Add(new ItemMod(Item.ItemType, "Chaos Damage: #-#")
                    {
                        Value = new List<float>(chaosMods[0].Value),
                        ValueColor = new List<ItemMod.ValueColoring> { ItemMod.ValueColoring.Chaos, ItemMod.ValueColoring.Chaos },
                    });
                }
            }

            Item.FlavourText = "Crafted by PoESkillTree";
            Item.UpdateRequirements();
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

        private void mss_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModSelector.SelectedValues))
            {
                RecalculateItem();
                return;
            }
            if (e.PropertyName != nameof(ModSelector.SelectedAffix))
            {
                return;
            }

            var ms = (ModSelector) sender;
            if (mss1 != ms)
                mss1.Affixes = _suffixes.Except(new[] { mss2.SelectedAffix, mss3.SelectedAffix }).ToList();

            if (mss2 != ms)
                mss2.Affixes = _suffixes.Except(new[] { mss1.SelectedAffix, mss3.SelectedAffix }).ToList();

            if (mss3 != ms)
                mss3.Affixes = _suffixes.Except(new[] { mss2.SelectedAffix, mss1.SelectedAffix }).ToList();

            RecalculateItem();
        }

        private void ms_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModSelector.SelectedValues))
            {
                RecalculateItem();
            }
        }

        public bool DialogResult { get; private set; }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            Item.SetJsonBase();

            Item.X = 0;
            Item.Y = 0;
            DialogResult = true;
            CloseCommand.Execute(null);
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            CloseCommand.Execute(null);
        }
    }
}
