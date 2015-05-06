using MahApps.Metro.Controls;
using POESKillTree.Utils;
using POESKillTree.ViewModels.ItemAttribute;
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
            ClassList = Enum.GetNames(typeof(Item.ItemClass)).Except(new[] { "" + Item.ItemClass.Unequipable, "" + Item.ItemClass.Invalid, "" + Item.ItemClass.Gem, }).ToArray();
        }


        public void OnpropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void cbClassSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var val = (Item.ItemClass)Enum.Parse(typeof(Item.ItemClass), (string)cbClassSelection.SelectedItem);
            BaseList = ItemBase.BaseList.Where(b => (b.Class & val) == val).ToArray();
            cbBaseSelection.SelectedIndex = 0;
        }

        private void cbBaseSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbBaseSelection.SelectedItem == null)
            {
                Item = null;
                return;
            }

            var itm = ((ItemBase)cbBaseSelection.SelectedItem).CreateItem();


            Item = itm;
        }


    }
}
