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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POESKillTree.Controls
{
    /// <summary>
    /// Interaction logic for ModSelector.xaml
    /// </summary>
    public partial class ModSelector : UserControl, INotifyPropertyChanged
    {
        private List<Affix> _Affixes;

        public List<Affix> Affixes
        {
            get { return _Affixes; }
            set { _Affixes = value; OnpropertyChanged("Affixes"); }
        } 


        public ModSelector()
        {
            InitializeComponent();
        }

        public void OnpropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void cbAffix_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var aff = cbAffix.SelectedItem as Affix;
            if(aff == null)
                return;

            var tics = aff.GetTiers().Select(im => im.Stats[0].Range.From).Concat(aff.GetTiers().Select(im => im.Stats[0].Range.To)).Select(f => (double)f).ToList();
            slValue.Minimum = tics.First();
            slValue.Maximum = tics.Last();
            slValue.Ticks = new DoubleCollection(tics);
        }

        private void slValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var aff = cbAffix.SelectedItem as Affix;
            if (aff == null)
                return;

            CurrentTier = aff.QueryMod(0, (float)slValue.Value).FirstOrDefault();
        }

        private ItemModTier _currentTier = null;

        public ItemModTier CurrentTier
        {
            get { return _currentTier; }
            set { _currentTier = value; OnpropertyChanged("CurrentTier"); }
        }

    }
}
