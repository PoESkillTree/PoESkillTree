using MahApps.Metro.Controls;
using POESKillTree.Controls;
using POESKillTree.Utils;
using POESKillTree.ViewModels.Items;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for CraftWindow.xaml
    /// </summary>
    public partial class CraftWindow : MetroWindow, INotifyPropertyChanged
    {

        string[] _clist;

        public string[] ClassList
        {
            get { return _clist; }
            set { _clist = value; OnpropertyChanged("ClassList"); }
        }

        ItemBase[] _blist;
        public ItemBase[] BaseList
        {
            get { return _blist; }
            set { _blist = value; OnpropertyChanged("BaseList"); }
        }

        Item _item;

        public Item Item
        {
            get { return _item; }
            set { _item = value; OnpropertyChanged("Item"); }
        }

        public CraftWindow()
        {
            InitializeComponent();
            ClassList = Enum.GetNames(typeof(ItemClass)).Except(new[] { "" + ItemClass.Unequipable, "" + ItemClass.Invalid, "" + ItemClass.Gem, }).ToArray();
        }


        public void OnpropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void cbClassSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var val = (ItemClass)Enum.Parse(typeof(ItemClass), (string)cbClassSelection.SelectedItem);
            BaseList = ItemBase.BaseList.Where(b => (b.Class & val) == val).ToArray();
            cbBaseSelection.SelectedIndex = 0;
        }

        List<Affix> _prefixes = new List<Affix>();
        List<Affix> _suffixes = new List<Affix>();

        int _SkipRedraw = 0;

        public bool SkipRedraw
        {
            get { return _SkipRedraw > 0; }
            set
            {
                if (value)
                    _SkipRedraw++;
                else
                    _SkipRedraw--;

                if (_SkipRedraw == 0)
                    RecalculateItem();
            }
        }

        private void cbBaseSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbBaseSelection.SelectedItem == null)
            {
                Item = null;
                return;
            }

            SkipRedraw = true;
            msp1.Affixes = msp2.Affixes = msp3.Affixes = mss1.Affixes = mss1.Affixes = mss2.Affixes = mss3.Affixes = null;

            var ibase = ((ItemBase)cbBaseSelection.SelectedItem);
            var itm = ibase.CreateItem();
            Item = itm;

            if (ibase.ImplicitMods != null)
            {
                Affix implaff = new Affix(ibase.ImplicitMods.Select(s => s.Name).ToArray(), new[] { new ItemModTier("implicits", 0, ibase.ImplicitMods) });

                List<string> aliases = new List<string>();

                foreach (var mn in ibase.ImplicitMods)
	            {
                    string mname = mn.Name.Replace("-", "").Replace("+", "");
                    var afm = Affix.AllAffixes.FirstOrDefault(a=>a.Mod.Contains(mname));
                    if (afm != null)
                    {
                        var indx = afm.Mod.IndexOf(mname);
                        var al = afm.Aliases[indx].First();
                        aliases.Add(al);
                    }
                    else
                        aliases.Add(mn.Name);
	            }

                implaff.Aliases = aliases.Select(a => new HashSet<string>() { a }).ToArray();

                
                msImplicitMods.Affixes = new List<Affix>() { implaff };
            }
            else
            {
                msImplicitMods.Affixes = null;
            }

            var aaff = Affix.AllAffixes.Where(a => a.ApplicableGear.Contains(itm.GearGroup)).ToArray();

            _prefixes = aaff.Where(a => a.IsPrefix).ToList();
            _suffixes = aaff.Where(a => a.IsSuffix).ToList();

            msp1.Affixes = msp2.Affixes = msp3.Affixes = _prefixes;
            mss1.Affixes = mss2.Affixes = mss3.Affixes = _suffixes;
            _selectedSuff = new[] { mss1, mss2, mss3 }.Where(s => s.SelectedAffix != null).ToArray();


            SkipRedraw = false;

        }

        ModSelector[] _selectedPreff = new ModSelector[0];
        private void msp1_SelectedAffixChanged(object sender, Affix aff)
        {
            var ms = sender as ModSelector;
            List<Affix> exc = new List<Affix>();
            if (msp1 != ms)
                msp1.Affixes = _prefixes.Except(new[] { msp2.SelectedAffix, msp3.SelectedAffix }).ToList();

            if (msp2 != ms)
                msp2.Affixes = _prefixes.Except(new[] { msp1.SelectedAffix, msp3.SelectedAffix }).ToList();

            if (msp3 != ms)
                msp3.Affixes = _prefixes.Except(new[] { msp2.SelectedAffix, msp1.SelectedAffix }).ToList();
            _selectedPreff = new[] { msp1, msp2, msp3 }.Where(s => s.SelectedAffix != null).ToArray();
            RecalculateItem();
        }

        private void RecalculateItem()
        {
            if (SkipRedraw)
                return;


            Item.NameLine = "";
            Item.TypeLine = Item.BaseType;

            if (_selectedPreff.Length + _selectedSuff.Length == 0)
            {
                Item.Frame = FrameType.White;
            }
            else if (_selectedPreff.Length <= 1 && _selectedSuff.Length <= 1)
            {
                Item.Frame = FrameType.Magic;
                string typeline = "";

                if (_selectedPreff.Length > 0)
                    typeline = _selectedPreff[0].SelectedAffix.Query(_selectedPreff[0].SelectedValues.Select(v => (_selectedPreff[0].SelectedAffix.Name.Contains(" per second")) ? (float)v * 60f : (float)v).ToArray()).First().Name + " ";

                typeline += Item.BaseType;

                if (_selectedSuff.Length > 0)
                    typeline += " " + _selectedSuff[0].SelectedAffix.Query(_selectedSuff[0].SelectedValues.Select(v => (_selectedSuff[0].SelectedAffix.Name.Contains(" per second")) ? (float)v * 60f : (float)v).ToArray()).First().Name;

                Item.TypeLine = typeline.Replace(" (Master Crafted)", "");
            }
            else
            {
                Item.Frame = FrameType.Rare;
                Item.NameLine = "Crafted " + Item.BaseType;
            }


            var prefixes = _selectedPreff.Select(p => p.GetExactMods()).SelectMany(m => m).ToList();
            var suffixes = _selectedSuff.Select(p => p.GetExactMods()).SelectMany(m => m).ToList();
            var allmods = prefixes.Concat(suffixes);


            Item.ExplicitMods = allmods.Where(m => m.Parent == null || m.Parent.ParentTier == null || !m.Parent.ParentTier.IsMasterCrafted)
                .GroupBy(m => m.Attribute)
                .Select(g => g.Aggregate((m1, m2) => m1.Sum(m2)))
                .ToList();

            Item.CraftedMods = allmods.Where(m => m.Parent != null && m.Parent.ParentTier != null && m.Parent.ParentTier.IsMasterCrafted).ToList();

            if (msImplicitMods.Affixes != null)
            {
                Item.ImplicitMods = msImplicitMods.GetExactMods().ToList();
            }

        }

        ModSelector[] _selectedSuff = new ModSelector[0];
        private void mss1_SelectedAffixChanged(object sender, Affix aff)
        {
            var ms = sender as ModSelector;
            List<Affix> exc = new List<Affix>();
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
    }
}
