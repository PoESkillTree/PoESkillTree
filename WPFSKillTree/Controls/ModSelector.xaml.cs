using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Controls
{
    /// <summary>
    /// Interaction logic for ModSelector.xaml
    /// </summary>
    public partial class ModSelector : INotifyPropertyChanged
    {
        private static readonly Affix EmptySelection = new Affix(new ItemModTier[0]);

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

        public float[] SelectedValues
        {
            get { return _sliders.Select(s => (float) s.Value).ToArray(); }
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void cbAffix_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _changingaffix = true;
            var aff = cbAffix.SelectedItem as Affix;

            spSLiders.Children.Clear();
            _sliders.Clear();

            tbtlabel.Text = "";
            if (aff != null && aff != EmptySelection)
            {
                var valueRuns = new List<Tuple<Run, Binding>>();
                string statName = null;
                for (var i = 0; i < aff.RangesPerStatAndTier.Count; i++)
                {
                    var ranges = aff.RangesPerStatAndTier[i];
                    var isFloatMod = ranges
                        .Any(r => !r.From.AlmostEquals((int)r.From, 1e-5) || !r.To.AlmostEquals((int)r.To, 1e-5));
                    var tics = ranges
                        .SelectMany(
                            r =>
                                Enumerable.Range((int)Math.Round(isFloatMod ? r.From * 100 : r.From),
                                    (int)Math.Round((r.To - r.From) * (isFloatMod ? 100 : 1) + 1)))
                        .Select(f => isFloatMod ? (double)f / 100 : f)
                        .ToList();

                    var overlayInlines = Enumerable.Empty<Inline>();
                    if (aff.StatNames[i] != statName)
                    {
                        statName = aff.StatNames[i];

                        var inlines = new List<Inline>();
                        var parts = statName.Split('#');
                        var offset = 0;
                        foreach (var part in parts.Take(parts.Length - 1))
                        {
                            bool alwaysShowSign;
                            if (part.EndsWith("+"))
                            {
                                inlines.Add(new Run(part.Substring(0, part.Length - 1)));
                                alwaysShowSign = true;
                            }
                            else
                            {
                                inlines.Add(new Run(part));
                                alwaysShowSign = false;
                            }
                            var binding = new Binding($"SelectedValues[{i + offset}]") { Source = this };
                            offset++;
                            if (alwaysShowSign)
                            {
                                binding.StringFormat = "+0;-#";
                            }
                            var run = new Run();
                            valueRuns.Add(Tuple.Create(run, binding));
                            inlines.Add(run);
                        }
                        inlines.Add(new Run(parts.Last()));

                        if (offset == 1 && tics.Count > 1)
                        {
                            overlayInlines = inlines;
                        }
                        else
                        {
                            var textBlock = new TextBlock();
                            textBlock.Inlines.AddRange(inlines);
                            spSLiders.Children.Add(textBlock);
                        }
                    }

                    var os = new OverlayedSlider(overlayInlines, new DoubleCollection(tics));
                    os.ValueChanged += slValue_ValueChanged;
                    os.Tag = i;

                    _sliders.Add(os);
                    if (tics.Count > 1)
                    {
                        spSLiders.Children.Add(os);
                    }
                }

                // can't set bindings before bound values are available (sliders are added after text blocks)
                foreach (var t in valueRuns)
                {
                    t.Item1.SetBinding(Run.TextProperty, t.Item2);
                }
            }

            OnPropertyChanged("SelectedAffix");

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

            int indx = (int) ((FrameworkElement) sender).Tag;

            var tiers = aff.QueryMod(indx, (float)e.NewValue).OrderBy(m => m.Name).ToArray();
            _updatingSliders = true;
            for (int i = 0; i < _sliders.Count; i++)
            {
                if (i != indx)
                {
                    if (!aff.QueryMod(i, (float)_sliders[i].Value).Intersect(tiers).Any())
                    { //slider isnt inside current tier
                        var moveto = aff.GetRange(tiers[0], i);
                        _sliders[i].Value = (e.NewValue > e.OldValue) ? moveto.From : moveto.To;
                    }

                }
            }
            _updatingSliders = false;
            OnPropertyChanged("SelectedValues");

            tbtlabel.Text = TiersString(SelectedAffix.Query(_sliders.Select(s => (float)s.Value).ToArray()));
        }

        private static string TiersString(IEnumerable<ItemModTier> tiers)
        {
            return string.Join("/", tiers.Select(s => $"T{s.Tier}: {s.Name}"));
        }

        public IEnumerable<ItemMod> GetExactMods()
        {
            if (SelectedAffix != null)
            {
                return SelectedAffix.ToItemMods(SelectedValues);
            }

            return new ItemMod[0];
        }
    }
}
