using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using POESKillTree.Model.Items;

namespace POESKillTree.Controls
{
    /// <summary>
    /// Interaction logic for ModSelector.xaml
    /// </summary>
    public partial class ModSelector : INotifyPropertyChanged
    {
        private static readonly Affix EmptySelection = new Affix(new[] { "" }, new ItemModTier[0]);

        private bool _canDeselect = true;

        public bool CanDeselect
        {
            private get { return _canDeselect; }
            set { _canDeselect = value; OnPropertyChanged("CanDeselect"); }
        }

        private List<Affix> _affixes;

        public List<Affix> Affixes
        {
            get { return _affixes; }
            set
            {
                if (value != null)
                {
                    var l = value.ToList();
                    if (CanDeselect)
                        l.Insert(0, EmptySelection);
                    _affixes = l;
                    if (!_affixes.Contains(SelectedAffix))
                    {
                        _sliders.Clear();
                        spSLiders.Children.Clear();
                    }
                }
                else
                    _affixes = null;

                OnPropertyChanged("Affixes");

                if (!CanDeselect && _affixes != null)
                    cbAffix.SelectedIndex = 0;
            }
        }

        public Affix SelectedAffix
        {
            get
            {
                var fx = cbAffix.SelectedItem as Affix;
                return (fx == EmptySelection) ? null : fx;
            }
        }

        public double[] SelectedValues
        {
            get { return _sliders.Select(s => s.Value).ToArray(); }
        }

        private readonly List<OverlayedSlider> _sliders = new List<OverlayedSlider>();

        private bool _changingaffix;
        private bool _updatingSliders;

        public ModSelector()
        {
            InitializeComponent();
        }

        private void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event Action<object, Affix> SelectedAffixChanged;

        public event Action<object, double[]> SelectedValuesChanged;

        private void cbAffix_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _changingaffix = true;
            var aff = cbAffix.SelectedItem as Affix;

            spSLiders.Children.Clear();
            _sliders.Clear();

            if (aff != null)
            {
                var tiers = aff.GetTiers();

                tbtlabel.Text = "";
                if (aff != EmptySelection)
                {
                    for (int i = 0; i < aff.Mods.Count; i++)
                    {
                        var tics =
                            tiers.SelectMany(
                                im =>
                                    Enumerable.Range((int) Math.Round(im.Stats[i].Range.From),
                                        (int) Math.Round(im.Stats[i].Range.To - im.Stats[i].Range.From + 1)))
                                .Select(f => (double) f)
                                .ToList();
                        var os = new OverlayedSlider(aff.Mods[i], new DoubleCollection(tics));

                        os.ValueChanged += slValue_ValueChanged;
                        os.Tag = i;

                        _sliders.Add(os);
                        spSLiders.Children.Add(os);
                    }
                }
            }

            OnPropertyChanged("SelectedAffix");
            if (SelectedAffixChanged != null)
                SelectedAffixChanged(this, aff);

            _changingaffix = false;

            if (_sliders.Count > 0)
                slValue_ValueChanged(_sliders[0], new RoutedPropertyChangedEventArgs<double>(_sliders[0].Value, _sliders[0].Value));
        }

        private void slValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updatingSliders || _changingaffix)
                return;
            var aff = cbAffix.SelectedItem as Affix;
            if (aff == null)
                return;

            int indx = (int) ((OverlayedSlider) sender).Tag;

            var tiers = aff.QueryMod(indx, (float)e.NewValue).OrderBy(m => m.Name).ToArray();
            _updatingSliders = true;
            for (int i = 0; i < _sliders.Count; i++)
            {
                if (i != indx)
                {
                    if (aff.QueryMod(i, (float)_sliders[i].Value).Intersect(tiers).FirstOrDefault() == null)
                    { //slider isnt inside current tier
                        var moveto = tiers[0].Stats[i].Range;
                        _sliders[i].Value = (e.NewValue > e.OldValue) ? moveto.From : moveto.To;
                    }

                }
            }
            _updatingSliders = false;
            OnPropertyChanged("SelectedValues");
            if (SelectedValuesChanged != null)
            {
                SelectedValuesChanged(this, SelectedValues);
            }

            tbtlabel.Text = TiersString(tiers);
        }

        private static string TiersString(IReadOnlyCollection<ItemModTier> tiers)
        {
            if (tiers == null || tiers.Count == 0)
                return "";
            return string.Join("/", tiers.OrderByDescending(t => t.Tier).Select(s => string.Format("T{0}:{1}", s.Tier, s.Name)));
        }

        public ItemMod[] GetExactMods()
        {
            if (SelectedAffix != null)
            {
                float[] values = _sliders.Select(s => (float)s.Value).ToArray();

                var aff = SelectedAffix.Query(values).First();

                if (SelectedAffix.IsRangeMod)
                {
                    return new[] { SelectedAffix.RangeCombinedStat.ToItemMod(_sliders.Select(s => (float)s.Value).ToArray()) };
                }

                var fvalues = aff.Stats.Select((s, i) => s.ToItemMod((float)_sliders[i].Value));

                return fvalues.ToArray();

            }

            return new ItemMod[0];
        }
    }
}
