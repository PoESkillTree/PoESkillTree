using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MB.Algodat;
using MoreLinq;
using PoESkillTree.GameModel.StatTranslation;
using POESKillTree.Model.Items.Mods;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Crafting
{
    /// <summary>
    /// Pair of a stat id and a associated value. Used for translation.
    /// </summary>
    public struct StatIdValuePair
    {
        public string StatId { get; }
        public int Value { get; }

        public StatIdValuePair(string statId, int value)
        {
            StatId = statId;
            Value = value;
        }
    }


    /// <summary>
    /// View model for selecting a mod out of a list of affixes and selecting their values.
    /// </summary>
    public class ModSelectorViewModel : Notifier
    {
        private static readonly Affix EmptySelection = new Affix();

        private readonly StatTranslator _statTranslator;

        public bool CanDeselct { get; }

        private IReadOnlyList<Affix> _affixes = new[] { EmptySelection };

        public IReadOnlyList<Affix> Affixes
        {
            get { return _affixes; }
            set
            {
                var v = (value ?? new Affix[0]).Prepend(EmptySelection).ToList();
                SetProperty(ref _affixes, v, () =>
                {
                    if (!CanDeselct && Affixes.Count > 1)
                    {
                        SelectedAffix = Affixes[1];
                        return;
                    }
                    if (Affixes.Contains(SelectedAffix))
                    {
                        return;
                    }
                    if (SelectedAffix != null)
                    {
                        // Assigned Affixes are created anew each time the crafted base changes.
                        // Try to select an Affix that matches the previously selected one.
                        var matchingAffix = Affixes.FirstOrDefault(a => a.Name == SelectedAffix.Name);
                        if (matchingAffix != null)
                        {
                            var valuesBefore = SelectedValues.ToList();
                            SelectedAffix = matchingAffix;
                            // Try to assign the old values
                            if (SelectedValues.Count() == valuesBefore.Count
                                && matchingAffix.QueryMods(valuesBefore).Any())
                            {
                                _sliders.Zip(valuesBefore, Tuple.Create)
                                    .ForEach(t => t.Item1.Value = t.Item2.value);
                            }
                            return;
                        }
                    }
                    SelectedAffix = EmptySelection;
                });
            }
        }

        private Affix _selectedAffix;

        public Affix SelectedAffix
        {
            get { return _selectedAffix; }
            set { SetProperty(ref _selectedAffix, value, OnSelectedAffixChanged); }
        }

        public bool IsEmptySelection => _selectedAffix == EmptySelection;

        private string _affixText;

        public string AffixText
        {
            get { return _affixText; }
            private set { SetProperty(ref _affixText, value); }
        }

        public IEnumerable<(int valueIndex, int value)> SelectedValues => _sliders.Select(s => (s.ValueIndex, s.Value));

        private readonly ObservableCollection<SliderGroupViewModel> _sliderGroups =
            new ObservableCollection<SliderGroupViewModel>();

        public IEnumerable<SliderGroupViewModel> SliderGroups => _sliderGroups;

        private readonly List<SliderViewModel> _sliders = new List<SliderViewModel>();

        private bool _updatingSliders;

        public ModSelectorViewModel(StatTranslator statTranslator, bool canDeselect = true)
        {
            _statTranslator = statTranslator;
            CanDeselct = canDeselect;
        }

        private void OnSelectedAffixChanged()
        {
            _sliders.ForEach(s => s.ValueChanged -= SliderOnValueChanged);
            _sliders.Clear();
            _sliderGroups.Clear();

            if (SelectedAffix != null && !IsEmptySelection)
            {
                var ids = new List<string>();
                var idToIndex = new Dictionary<string, int>();
                for (var i = 0; i < SelectedAffix.ValueCount; i++)
                {
                    var stat = SelectedAffix.FirstTierStats[i];
                    ids.Add(stat.Id);
                    idToIndex[stat.Id] = i;
                }

                var translations = _statTranslator.GetTranslations(ids);
                var groups = new List<IReadOnlyList<int>>();
                foreach (var translation in translations)
                {
                    var translatedIndices = translation.Ids
                        .Where(id => idToIndex.ContainsKey(id))
                        .Select(id => idToIndex[id]).ToList();
                    groups.Add(translatedIndices);
                }

                foreach (var group in groups)
                {
                    var sliderGroup = new List<SliderViewModel>();

                    foreach (var valueIndex in group)
                    {
                        var ranges = SelectedAffix.GetRanges(valueIndex);
                        var ticks = ranges.SelectMany(r => Enumerable.Range(r.From, r.To - r.From + 1))
                            .OrderBy(i => i).Distinct();
                        var slider = new SliderViewModel(valueIndex, ticks);
                        sliderGroup.Add(slider);
                        _sliders.Add(slider);
                    }

                    var translation = new DynamicTranslation(_statTranslator, SelectedAffix, group);
                    _sliderGroups.Add(new SliderGroupViewModel(sliderGroup, translation));
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

        private void SliderOnValueChanged(object sender, SliderValueChangedEventArgs e)
        {
            if (_updatingSliders)
                return;

            var slider = (SliderViewModel) sender;

            IMod[] tiers = SelectedAffix.QueryModsSingleValue(slider.ValueIndex, e.NewValue)
                .OrderBy(m => m.RequiredLevel).ToArray();
            _updatingSliders = true;
            foreach (var other in _sliders.Where(s => s != slider))
            {
                var iValue = other.ValueIndex;
                if (!SelectedAffix.QueryModsSingleValue(iValue, other.Value).Intersect(tiers).Any())
                {
                    // slider isn't inside current tier
                    Range<int> moveto = tiers[0].Stats[iValue].Range;
                    other.Value = (e.NewValue > e.OldValue) ? moveto.From : moveto.To;
                }
            }
            _updatingSliders = false;

            OnPropertyChanged("SelectedValues");

            AffixText = string.Join("/", SelectedAffix.QueryMods(SelectedValues).Select(s => $"{s.Name}").Distinct());
        }

        public IMod Query()
        {
            return SelectedAffix.QueryMods(SelectedValues).First();
        }

        public IEnumerable<StatIdValuePair> GetStatValues()
        {
            var firstMatch = SelectedAffix.QueryMods(SelectedValues).FirstOrDefault();
            if (firstMatch == null)
            {
                return Enumerable.Empty<StatIdValuePair>();
            }

            return
                from t in SelectedValues
                let valueIndex = t.valueIndex
                let value = t.value
                let stat = firstMatch.Stats[valueIndex]
                select new StatIdValuePair(stat.Id, value);
        }

        /// <summary>
        /// Translation implementation that dynamically selects the stat id to translate based on the given values.
        /// </summary>
        private class DynamicTranslation : ITranslation
        {
            private readonly StatTranslator _statTranslator;
            private readonly Affix _affix;
            private readonly IReadOnlyList<int> _valueIndices;

            public DynamicTranslation(StatTranslator statTranslator, Affix affix, IEnumerable<int> valueIndices)
            {
                _statTranslator = statTranslator;
                _affix = affix;
                _valueIndices = valueIndices.ToList();
            }

            public string Translate(IReadOnlyList<int> values)
            {
                if (_valueIndices.Count != values.Count)
                {
                    throw new ArgumentException("Number of values must match number of value indices");
                }

                var firstMatch = _valueIndices
                    .EquiZip(values, (i, v) => _affix.QueryModsSingleValue(i, v))
                    .Aggregate((a, ms) => a.Intersect(ms))
                    .FirstOrDefault();
                if (firstMatch == null)
                {
                    return null;
                }

                var idValueDict = _valueIndices
                    .Select(i => firstMatch.Stats[i].Id)
                    .EquiZip(values, Tuple.Create)
                    .ToDictionary(t => t.Item1, t => t.Item2);
                return _statTranslator.GetTranslations(idValueDict).First();
            }
        }
    }
}