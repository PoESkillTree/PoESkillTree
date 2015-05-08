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
            set { _item = value; OnpropertyChanged("Item");}
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

        private void cbBaseSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbBaseSelection.SelectedItem == null)
            {
                Item = null;
                return;
            }

            var itm = ((ItemBase)cbBaseSelection.SelectedItem).CreateItem();
            Item = itm;

            var aaff = Affix.AllAffixes.Where(a => a.ApplicableGear.Contains(itm.GearGroup)).ToArray();

            _prefixes = aaff.Where(a => a.IsPrefix).ToList();
            _suffixes = aaff.Where(a => a.IsSuffix).ToList();

            msp1.Affixes = msp2.Affixes = msp3.Affixes = _prefixes;
            mss1.Affixes = mss2.Affixes = mss3.Affixes = _suffixes;
            _selectedSuff= new[] { mss1, mss2, mss3 }.Where(s => s.SelectedAffix != null).ToArray();
            RecalculateItem();

        }

        ModSelector[] _selectedPreff = new ModSelector[0];
        private void msp1_SelectedAffixChanged(object sender, Affix aff)
        {
            var ms = sender as ModSelector;
            List<Affix> exc = new List<Affix>();
            if (msp1 != ms)
                msp1.Affixes = _prefixes.Except(new[] {msp2.SelectedAffix, msp3.SelectedAffix }).ToList();

            if (msp2 != ms)
                msp2.Affixes = _prefixes.Except(new[] { msp1.SelectedAffix, msp3.SelectedAffix }).ToList();

            if (msp3 != ms)
                msp3.Affixes = _prefixes.Except(new[] { msp2.SelectedAffix, msp1.SelectedAffix }).ToList();
            _selectedPreff = new[] { msp1, msp2, msp3 }.Where(s => s.SelectedAffix != null).ToArray();
            RecalculateItem();
        }

        private void RecalculateItem()
        {

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
                    typeline = _selectedPreff[0].SelectedAffix.Query(_selectedPreff[0].SelectedValues.Select(v => (float)v).ToArray())[0].Name + " ";

                typeline += Item.BaseType;

                if (_selectedSuff.Length > 0)
                    typeline += " "+_selectedSuff[0].SelectedAffix.Query(_selectedSuff[0].SelectedValues.Select(v => (float)v).ToArray())[0].Name;

                Item.TypeLine = typeline;
            }
            else
            {
                Item.Frame = FrameType.Rare;
                Item.NameLine = "Crafted " + Item.BaseType;
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

            _selectedSuff = new[] { mss1, mss2, mss3 }.Where(s=>s.SelectedAffix!=null).ToArray();

            RecalculateItem();
        }

        private void ms_SelectedValuesChanged(object sender, double[] values)
        {
            RecalculateItem();
        }
    }
}
