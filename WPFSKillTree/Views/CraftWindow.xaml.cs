using MahApps.Metro.Controls;
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

        List<Affix> prefixes = new List<Affix>();
        List<Affix> suffixes = new List<Affix>();

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

            var prefixes = aaff.Where(a => a.IsPrefix).ToList();
            var suffixes = aaff.Where(a => a.IsSuffix).ToList();

            msp1.Affixes = msp2.Affixes = msp3.Affixes = prefixes;
            mss1.Affixes = mss2.Affixes = mss3.Affixes = suffixes;

        }


    }
}
