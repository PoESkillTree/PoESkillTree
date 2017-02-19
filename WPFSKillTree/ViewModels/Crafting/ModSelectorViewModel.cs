using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MoreLinq;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.ViewModels.Crafting
{
    /// <summary>
    /// View model for selecting a mod out of a list of affixes and selecting their values.
    /// </summary>
    public class ModSelectorViewModel : Notifier
    {
        private static readonly Affix EmptySelection = new Affix();

        public bool CanDeselct { get; }

        private IReadOnlyList<Affix> _affixes = new[] { EmptySelection };

        public IReadOnlyList<Affix> Affixes
        {
            get { return _affixes; }
            set
            {
                var v = EmptySelection.Concat(value ?? new Affix[0]).ToList();
                SetProperty(ref _affixes, v, () =>
                {
                    if (!CanDeselct && Affixes.Count > 1)
                    {
                        SelectedAffix = Affixes[1];
                    }
                    else if (!Affixes.Contains(SelectedAffix))
                    {
                        SelectedAffix = EmptySelection;
                    }
                });
            }
        }

        private Affix _selectedAffix;

        public Affix SelectedAffix
        {
            get { return _selectedAffix; }
            set { SetProperty(ref _selectedAffix, value, OnSelectedAffixChanged); }
        }

        public bool IsEmptySelection
        {
            get { return _selectedAffix == EmptySelection; }
        }

        private string _affixText;

        public string AffixText
        {
            get { return _affixText; }
            private set { SetProperty(ref _affixText, value); }
        }

        public IEnumerable<float> SelectedValues
        {
            get { return _sliders.Select(s => s.Value); }
        }

        private readonly ObservableCollection<SliderGroupViewModel> _sliderGroups =
            new ObservableCollection<SliderGroupViewModel>();

        public IReadOnlyList<SliderGroupViewModel> SliderGroups
        {
            get { return _sliderGroups; }
        }

        private readonly List<SliderViewModel> _sliders = new List<SliderViewModel>();

        private bool _updatingSliders;

        public ModSelectorViewModel(bool canDeselect = true)
        {
            CanDeselct = canDeselect;
        }

        private void OnSelectedAffixChanged()
        {
            _sliders.ForEach(s => s.ValueChanged -= SliderOnValueChanged);
            _sliders.Clear();
            _sliderGroups.Clear();

            if (SelectedAffix != null && !IsEmptySelection)
            {
                var sliderGroup = new List<SliderViewModel>();
                var texts = new Queue<string>();
                var sliderShowText = false;
                string statName = null;
                for (int i = 0; i < SelectedAffix.RangesPerTreeAndTier.Count; i++)
                {
                    if (SelectedAffix.StatNames[i] != statName)
                    {
                        if (sliderGroup.Any())
                        {
                            var group = new SliderGroupViewModel(sliderGroup, texts.Dequeue());
                            _sliderGroups.Add(group);
                            sliderGroup.Clear();
                        }

                        statName = SelectedAffix.StatNames[i];

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

                    var ranges = SelectedAffix.RangesPerTreeAndTier[i];
                    var isFloatMod = ranges
                        .Any(r => !r.From.AlmostEquals((int)r.From, 1e-5) || !r.To.AlmostEquals((int)r.To, 1e-5));
                    IEnumerable<double> ticks = ranges
                        .SelectMany(
                            r =>
                                Enumerable.Range((int) Math.Round(isFloatMod ? r.From * 100 : r.From),
                                    (int) Math.Round((r.To - r.From) * (isFloatMod ? 100 : 1) + 1)))
                        .Select(f => isFloatMod ? (double) f / 100 : f);

                    var slider = new SliderViewModel(texts.Dequeue(), sliderShowText, i, ticks);
                    sliderGroup.Add(slider);
                    _sliders.Add(slider);
                }

                if (sliderGroup.Any())
                {
                    var group = new SliderGroupViewModel(sliderGroup, texts.Dequeue());
                    _sliderGroups.Add(group);
                }
            }

            _sliders.ForEach(s => s.ValueChanged += SliderOnValueChanged);

            if (_sliders.Count > 0)
            {
                SliderOnValueChanged(_sliders[0], new SliderValueChangedEventArgs(_sliders[0].Value, _sliders[0].Value));
            }
            else
            {
                AffixText = "";
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
            if (_updatingSliders)
                return;

            var slider = (SliderViewModel) sender;
            int index = slider.Index;

            var tiers = SelectedAffix.QueryMod(index, e.NewValue).OrderBy(m => m.Name).ToArray();
            _updatingSliders = true;
            for (int i = 0; i < _sliders.Count; i++)
            {
                if (i != index && !SelectedAffix.QueryMod(i, _sliders[i].Value).Intersect(tiers).Any())
                {
                    // slider isn't inside current tier
                    var moveto = SelectedAffix.GetRange(tiers[0], i);
                    _sliders[i].Value = (e.NewValue > e.OldValue) ? moveto.From : moveto.To;
                }
            }
            _updatingSliders = false;

            OnPropertyChanged("SelectedValues");

            AffixText = TiersString(SelectedAffix.Query(SelectedValues));
        }

        private static string TiersString(IEnumerable<ItemModTier> tiers)
        {
            return string.Join("/", tiers.Select(s => $"T{s.Tier}: {s.Name}"));
        }

        public IEnumerable<ItemMod> GetExactMods()
        {
            if (IsEmptySelection)
            {
                return new ItemMod[0];
            }
            return SelectedAffix.ToItemMods(SelectedValues);
        }
    }
}