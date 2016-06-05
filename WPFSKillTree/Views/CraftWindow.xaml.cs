using POESKillTree.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using MB.Algodat;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Model.Items.Enums;
using POESKillTree.ViewModels;

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

        private ItemGroup[] _groupList;
        public ItemGroup[] GroupList
        {
            get { return _groupList; }
            private set { _groupList = value; OnPropertyChanged(); }
        }

        private ItemType[] _typeList;
        public ItemType[] TypeList
        {
            get { return _typeList; }
            private set
            {
                _typeList = value;
                OnPropertyChanged();
            }
        }

        private ItemBase[] _blist;
        public ItemBase[] BaseList
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

        private int _skipRedraw;
        private bool SkipRedraw
        {
            get { return _skipRedraw > 0; }
            set
            {
                if (value)
                    _skipRedraw++;
                else
                    _skipRedraw--;

                if (_skipRedraw == 0)
                    RecalculateItem();
            }
        }

        private ModSelector[] _selectedPreff = new ModSelector[0];
        private ModSelector[] _selectedSuff = new ModSelector[0];

        private readonly EquipmentData _equipmentData;

        public CraftWindow(EquipmentData equipmentData)
        {
            _equipmentData = equipmentData;
            InitializeComponent();
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            GroupList = Enum.GetValues(typeof(ItemGroup)).Cast<ItemGroup>().Except(new[] {ItemGroup.Unknown, ItemGroup.Gem}).ToArray();
        }

        private void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void GroupSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SkipRedraw = true;
            var val = (ItemGroup) GroupSelection.SelectedItem;
            TypeList = Enum.GetValues(typeof(ItemType)).Cast<ItemType>().Where(t => t.Group() == val).ToArray();
            TypeSelection.SelectedIndex = 0;
            SkipRedraw = false;
        }

        private void ClassSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Happens if called in GroupSelection_SelectionChanged
            if (TypeSelection.SelectedItem == null) return;

            SkipRedraw = true;
            var val = (ItemType) TypeSelection.SelectedItem;
            BaseList = _equipmentData.BaseList.Where(b => b.ItemType == val).ToArray();
            BaseSelection.SelectedIndex = 0;
            SkipRedraw = false;
        }

        private void BaseSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BaseSelection.SelectedItem == null)
            {
                Item = null;
                return;
            }

            SkipRedraw = true;
            msp1.Affixes = msp2.Affixes = msp3.Affixes = mss1.Affixes = mss1.Affixes = mss2.Affixes = mss3.Affixes = null;

            var ibase = (ItemBase)BaseSelection.SelectedItem;
            Item = ibase.CreateItem();

            if (ibase.ImplicitMods.Any())
            {
                msImplicitMods.Affixes = new List<Affix>
                {
                    new Affix(ibase.ImplicitMods.Select(s => s.Name).ToArray(), new[] { new ItemModTier(ibase.ImplicitMods) })
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
                var qualityAffix = new Affix(new[] {QualityModName}, new[] {new ItemModTier(new[] {qualityStat})});
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

            SkipRedraw = false;
        }

        private void msp_SelectedAffixChanged(object sender, Affix aff)
        {
            SkipRedraw = true;
            var ms = sender as ModSelector;

            if (msp1 != ms)
                msp1.Affixes = _prefixes.Except(new[] { msp2.SelectedAffix, msp3.SelectedAffix }).ToList();

            if (msp2 != ms)
                msp2.Affixes = _prefixes.Except(new[] { msp1.SelectedAffix, msp3.SelectedAffix }).ToList();

            if (msp3 != ms)
                msp3.Affixes = _prefixes.Except(new[] { msp2.SelectedAffix, msp1.SelectedAffix }).ToList();
            _selectedPreff = new[] { msp1, msp2, msp3 }.Where(s => s.SelectedAffix != null).ToArray();
            SkipRedraw = false;
        }

        private void RecalculateItem()
        {
            if (SkipRedraw)
                return;
            if (Item == null)
                return;

            Item.NameLine = "";
            Item.TypeLine = Item.BaseType.Name;

            if (_selectedPreff.Length + _selectedSuff.Length == 0)
            {
                Item.Frame = FrameType.White;
            }
            else if (_selectedPreff.Length <= 1 && _selectedSuff.Length <= 1)
            {
                Item.Frame = FrameType.Magic;
                var typeline = "";

                if (_selectedPreff.Length > 0)
                    typeline = _selectedPreff[0].SelectedAffix.Query(_selectedPreff[0].SelectedValues.Select(v => (float)v).ToArray()).First().Name + " ";

                typeline += Item.BaseType;

                if (_selectedSuff.Length > 0)
                    typeline += " " + _selectedSuff[0].SelectedAffix.Query(_selectedSuff[0].SelectedValues.Select(v => (float)v).ToArray()).First().Name;

                Item.TypeLine = typeline;
            }
            else
            {
                Item.Frame = FrameType.Rare;
                Item.NameLine = "Crafted " + Item.BaseType;
            }

            var prefixes = _selectedPreff.Select(p => p.GetExactMods()).SelectMany(m => m).ToList();
            var suffixes = _selectedSuff.Select(p => p.GetExactMods()).SelectMany(m => m).ToList();
            var allmods = prefixes.Concat(suffixes)
                .GroupBy(m => m.Attribute)
                .Select(g => g.Aggregate((m1, m2) => m1.Sum(m2)))
                .ToList();

            Item.ExplicitMods = allmods.Where(m => m.Parent == null || !m.Parent.ParentTier.IsMasterCrafted).ToList();
            Item.CraftedMods = allmods.Where(m => m.Parent != null && m.Parent.ParentTier.IsMasterCrafted).ToList();

            if (msImplicitMods.Affixes != null)
            {
                Item.ImplicitMods = msImplicitMods.GetExactMods().ToList();
            }

            var quality = 0.0;
            if (MsQuality.Affixes != null && MsQuality.SelectedValues[0] > 0)
            {
                quality = MsQuality.SelectedValues[0];
            }
            Item.Properties = Item.BaseType.GetRawProperties((float) quality);
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

        private void mss_SelectedAffixChanged(object sender, Affix aff)
        {
            var ms = sender as ModSelector;
            if (mss1 != ms)
                mss1.Affixes = _suffixes.Except(new[] { mss2.SelectedAffix, mss3.SelectedAffix }).ToList();

            if (mss2 != ms)
                mss2.Affixes = _suffixes.Except(new[] { mss1.SelectedAffix, mss3.SelectedAffix }).ToList();

            if (mss3 != ms)
                mss3.Affixes = _suffixes.Except(new[] { mss2.SelectedAffix, mss1.SelectedAffix }).ToList();

            _selectedSuff = new[] { mss1, mss2, mss3 }.Where(s => s.SelectedAffix != null).ToArray();

            RecalculateItem();
        }

        private void ms_SelectedValuesChanged(object sender, double[] values)
        {
            RecalculateItem();
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
