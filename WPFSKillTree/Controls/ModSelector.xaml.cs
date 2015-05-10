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

        private static readonly Affix EmptySelection = new Affix(new[] { "" }, new ItemModTier[0]);


        private bool _CanDeselect = true;

        public bool CanDeselect
        {
            get { return _CanDeselect; }
            set { _CanDeselect = value; OnpropertyChanged("CanDeselect"); }
        }

        private List<Affix> _Affixes;

        public List<Affix> Affixes
        {
            get { return _Affixes; }
            set
            {
                if (value != null)
                {
                    var l = value.ToList();
                    if (CanDeselect)
                        l.Insert(0, EmptySelection);
                    _Affixes = l;
                    if (!_Affixes.Contains(SelectedAffix))
                    {
                        sliders.Clear();
                        spSLiders.Children.Clear();
                    }
                }
                else
                    _Affixes = null;

                OnpropertyChanged("Affixes");

                if (!CanDeselect && _Affixes != null)
                    cbAffix.SelectedIndex = 0;
            }
        }

        public event Action<object, Affix> SelectedAffixChanged;


        public Affix SelectedAffix
        {
            get
            {
                var fx = cbAffix.SelectedItem as Affix;
                return (fx == EmptySelection) ? null : fx;
            }
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

        List<OverlayedSlider> sliders = new List<OverlayedSlider>();
        bool _changingaffix = false;
        private void cbAffix_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _changingaffix = true;
            var aff = cbAffix.SelectedItem as Affix;

            spSLiders.Children.Clear();
            sliders.Clear();

            if (aff != null)
            {
                var tiers = aff.GetTiers();

                tbtlabel.Text = "";
                if (aff != EmptySelection)
                {
                    for (int i = 0; i < aff.Mod.Count; i++)
                    {
                        OverlayedSlider os = new OverlayedSlider();
                        os.OverlayText = aff.AliasStrings[i];

                        sliders.Add(os);
                        os.ValueChanged += slValue_ValueChanged;
                        os.Tag = i;
                        var tics = tiers.SelectMany(im => Enumerable.Range((int)Math.Round(im.Stats[i].Range.From), (int)Math.Round(im.Stats[i].Range.To - im.Stats[i].Range.From + 1))).Select(f => (double)f).ToList();

                        if (aff.AliasStrings[i].Contains(" per second"))
                        {
                            tics = tics.Select(t => t / 60.0).ToList();
                        }


                        os.Minimum = tics.First();
                        os.Maximum = tics.Last();
                        os.Ticks = new DoubleCollection(tics);


                        spSLiders.Children.Add(os);
                    }
                }
            }

            OnpropertyChanged("SelectedAffix");
            if (SelectedAffixChanged != null)
                SelectedAffixChanged(this, aff);

            _changingaffix = false;
        }

        bool _updatingSliders = false;
        private void slValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updatingSliders || _changingaffix)
                return;
            var aff = cbAffix.SelectedItem as Affix;
            if (aff == null)
                return;

            var sl = sender as OverlayedSlider;
            int indx = (int)sl.Tag;

            var tiers = aff.QueryMod(indx, (float)e.NewValue).OrderBy(m => m.Name).ToArray();
            _updatingSliders = true;
            for (int i = 0; i < sliders.Count; i++)
            {
                if (i != indx)
                {
                    if (aff.IsRangeMod)
                    {
                        if (i > indx)
                        {
                            if (sliders[i].Value < sliders[indx].Value)
                                sliders[i].Value = sliders[indx].Value;
                        }
                        else if (i < indx)
                        {
                            if (sliders[i].Value > sliders[indx].Value)
                                sliders[i].Value = sliders[indx].Value;
                        }

                    }


                    if (aff.QueryMod(i, (float)sliders[i].Value).Intersect(tiers).FirstOrDefault() == null)
                    { //slider isnt inside current tier
                        var moveto = tiers[0].Stats[i].Range;
                        sliders[i].Value = (e.NewValue > e.OldValue) ? moveto.From : moveto.To;
                    }

                }
            }
            _updatingSliders = false;
            OnpropertyChanged("SelectedValues");
            if (SelectedValuesChanged != null)
            {
                SelectedValuesChanged(this, SelectedValues);
            }

            tbtlabel.Text = TiersString(tiers);

        }


        public string TiersString(ItemModTier[] tiers)
        {
            if (tiers == null || tiers.Length == 0)
                return "";
            return string.Join("/", tiers.Select(s => string.Format("T{0}:{1}", s.Tier, s.Name)));
        }

        public event Action<object, double[]> SelectedValuesChanged;

        public double[] SelectedValues
        {
            get { return sliders.Select(s => s.Value).ToArray(); }
        }

        public ItemMod[] GetExactMods()
        {
            if (SelectedAffix != null)
            {
                float[] values = sliders.Select(s => (float)s.Value).ToArray();
                if (SelectedAffix.Name.Contains(" per second"))
                    values = values.Select(v => v * 60f).ToArray();

                var aff = SelectedAffix.Query(values).FirstOrDefault();


                if (aff.ParentAffix.IsRangeMod)
                {
                    return new[] { aff.Stats[0].ToItemMod(sliders.Select(s => (float)s.Value).ToArray()) };
                }

                var fvalues = aff.Stats.Select((s, i) => s.ToItemMod((float)sliders[i].Value));

                return fvalues.ToArray();

            }

            return new ItemMod[0];
        }
    }
}
