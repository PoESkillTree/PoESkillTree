using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Utils.Extensions;
using POESKillTree.ViewModels.Crafting;

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
                        SliderGroups = new SliderGroupViewModel[0];
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

        public IEnumerable<float> SelectedValues
        {
            get { return _sliders.Select(s => s.Value); }
        }

        private IReadOnlyList<SliderGroupViewModel> _sliderGroups;
        public IReadOnlyList<SliderGroupViewModel> SliderGroups
        {
            get { return _sliderGroups; }
            private set
            {
                _sliderGroups = value;
                OnPropertyChanged(nameof(SliderGroups));
            }
        }

        private readonly List<SliderViewModel> _sliders = new List<SliderViewModel>();

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

            _sliders.Clear();
            tbtlabel.Text = "";

            var sliderGroups = new List<SliderGroupViewModel>();
            if (aff != null && aff != EmptySelection)
            {
                var sliderGroup = new List<SliderViewModel>();
                var texts = new Queue<string>();
                var sliderShowText = false;
                string statName = null;
                for (var i = 0; i < aff.RangesPerStatAndTier.Count; i++)
                {
                    if (aff.StatNames[i] != statName)
                    {
                        if (sliderGroup.Any())
                        {
                            var group = new SliderGroupViewModel(sliderGroup, texts.Dequeue());
                            sliderGroups.Add(group);
                            sliderGroup.Clear();
                        }

                        statName = aff.StatNames[i];

                        var formatTexts = StatNameToFormats(statName).ToList();
                        if (formatTexts.Count > 2)
                        {
                            sliderShowText = false;
                            texts = new Queue<string>(formatTexts);
                        }
                        else
                        {
                            sliderShowText = true;
                            texts = new Queue<string>();
                            texts.Enqueue(string.Join("", formatTexts));
                            texts.Enqueue("");
                        }
                    }

                    var ranges = aff.RangesPerStatAndTier[i];
                    var isFloatMod = ranges
                        .Any(r => !r.From.AlmostEquals((int)r.From, 1e-5) || !r.To.AlmostEquals((int)r.To, 1e-5));
                    var ticks = ranges
                        .SelectMany(
                            r =>
                                Enumerable.Range((int)Math.Round(isFloatMod ? r.From * 100 : r.From),
                                    (int)Math.Round((r.To - r.From) * (isFloatMod ? 100 : 1) + 1)))
                        .Select(f => isFloatMod ? (double)f / 100 : f);

                    var slider = new SliderViewModel(texts.Dequeue(), sliderShowText, i, ticks);
                    slider.ValueChanged += SliderOnValueChanged;
                    sliderGroup.Add(slider);
                    _sliders.Add(slider);
                }

                if (sliderGroup.Any())
                {
                    var group = new SliderGroupViewModel(sliderGroup, texts.Dequeue());
                    sliderGroups.Add(group);
                }
            }
            SliderGroups = sliderGroups;

            OnPropertyChanged("SelectedAffix");

            _changingaffix = false;

            if (_sliders.Count > 0)
            {
                SliderOnValueChanged(_sliders[0], new SliderValueChangedEventArgs(_sliders[0].Value, _sliders[0].Value));
            }
        }

        private static IEnumerable<string> StatNameToFormats(string statName)
        {
            var parts = statName.Split('#');
            foreach (var part in parts.Take(parts.Length - 1))
            {
                string text;
                if (part.EndsWith("+"))
                {
                    text = part.Substring(0, part.Length - 1) + "{0}";
                }
                else
                {
                    text = part + "{0}";
                }
                yield return text;
            }
            yield return parts.Last();
        }

        private void SliderOnValueChanged(object sender, SliderValueChangedEventArgs e)
        {
            if (_updatingSliders || _changingaffix)
                return;
            var aff = cbAffix.SelectedItem as Affix;
            if (aff == null)
                return;

            var slider = (SliderViewModel) sender;
            var index = slider.Index;

            var tiers = aff.QueryMod(index, e.NewValue).OrderBy(m => m.Name).ToArray();
            _updatingSliders = true;
            for (int i = 0; i < _sliders.Count; i++)
            {
                if (i != index)
                {
                    if (!aff.QueryMod(i, _sliders[i].Value).Intersect(tiers).Any())
                    { //slider isnt inside current tier
                        var moveto = aff.GetRange(tiers[0], i);
                        _sliders[i].Value = (e.NewValue > e.OldValue) ? moveto.From : moveto.To;
                    }

                }
            }
            _updatingSliders = false;
            OnPropertyChanged("SelectedValues");

            tbtlabel.Text = TiersString(SelectedAffix.Query(SelectedValues));
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
