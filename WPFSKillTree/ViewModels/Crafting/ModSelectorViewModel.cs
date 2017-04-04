using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MB.Algodat;
using MoreLinq;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.ViewModels.Crafting
{
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
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

#if (PoESkillTree_UseSmallDec_ForAttributes && PoESkillTree_UseSmallDec_ForGeneratorBars)
        public IEnumerable<IEnumerable<SmallDec>> SelectedValues
#else
        public IEnumerable<IEnumerable<float>> SelectedValues
#endif
        {
#if (PoESkillTree_UseSmallDec_ForAttributes && PoESkillTree_UseSmallDec_ForGeneratorBars)
            get { return _sliderGroups.Select(g => g.Sliders.Select(s => s.Value)); }
#else
            get { return _sliderGroups.Select(g => g.Sliders.Select(s => s.Value)); }
#endif
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
                for (int i = 0; i < SelectedAffix.StatNames.Count; i++)
                {
                    var sliderGroup = new List<SliderViewModel>();

                    for (var j = 0; j < SelectedAffix.ValueCountPerStat[i]; j++)
                    {
                        var ranges = SelectedAffix.GetRanges(i, j);
                        var isFloatMod = ranges
                            .Any(r => !r.From.AlmostEquals((int)r.From, 1e-5) || !r.To.AlmostEquals((int)r.To, 1e-5));
                        IEnumerable<double> ticks = ranges
                            .SelectMany(
                                r =>
#if (PoESkillTree_UseSmallDec_ForAttributes && PoESkillTree_UseSmallDec_ForGeneratorBars)
                                    Enumerable.Range((int) SmallDec.Round(isFloatMod ? r.From * 100 : r.From),
                                        (int) SmallDec.Round((r.To - r.From) * (isFloatMod ? 100 : 1) + 1)))
                            .Select(f => isFloatMod ? (double) f / 100 : f);
#elif (PoESkillTree_UseSmallDec_ForAttributes)
                                    Enumerable.Range((int) SmallDec.Round(isFloatMod ? r.From * 100 : r.From),
                                        (int) SmallDec.Round((r.To - r.From) * (isFloatMod ? 100 : 1) + 1)))
                            .Select(f => isFloatMod ? (double) f / 100 : f);
#else
                                    Enumerable.Range((int) Math.Round(isFloatMod ? r.From * 100 : r.From),
                                        (int) Math.Round((r.To - r.From) * (isFloatMod ? 100 : 1) + 1)))
                            .Select(f => isFloatMod ? (double) f / 100 : f);
#endif

                        var slider = new SliderViewModel(i, j, ticks);
                        sliderGroup.Add(slider);
                        _sliders.Add(slider);
                    }

                    string groupFormat = StatNameToFormat(SelectedAffix.StatNames[i]);
                    var group = new SliderGroupViewModel(sliderGroup, groupFormat);
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

        private static string StatNameToFormat(string statName)
        {
            var format = "";
            var parts = statName.Split('#');
            for (var i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                if (part.EndsWith("+"))
                {
                    // replace "+#" placeholders by a format to always show the sign
                    format += part.Substring(0, part.Length - 1) + "{" + i + ":+0;-#}";
                }
                else
                {
                    format += part + "{" + i + "}";
                }
            }
            format += parts.Last();
            return format;
        }

        private void SliderOnValueChanged(object sender, SliderValueChangedEventArgs e)
        {
            if (_updatingSliders)
                return;

            var slider = (SliderViewModel) sender;

            ItemModTier[] tiers = SelectedAffix.QueryMod(slider.StatIndex, slider.ValueIndex, e.NewValue)
                .OrderBy(m => m.Name).ToArray();
            _updatingSliders = true;
            foreach (var other in _sliders.Where(s => s != slider))
            {
                var iStat = other.StatIndex;
                var iValue = other.ValueIndex;
                if (!SelectedAffix.QueryMod(iStat, iValue, other.Value).Intersect(tiers).Any())
                {
                    // slider isn't inside current tier
#if (PoESkillTree_UseSmallDec_ForAttributes && PoESkillTree_UseSmallDec_ForGeneratorBars)
                    Range<SmallDec> moveto = tiers[0].Stats[iStat].Ranges[iValue];
#else
                    Range<float> moveto = tiers[0].Stats[iStat].Ranges[iValue];
#endif
#if (PoESkillTree_UseSmallDec_ForAttributes && !PoESkillTree_UseSmallDec_ForGeneratorBars)
                    other.Value = (e.NewValue > e.OldValue) ? (float)moveto.From : (float)moveto.To;
#else
                    other.Value = (e.NewValue > e.OldValue) ? moveto.From : moveto.To;
#endif
                }
            }
            _updatingSliders = false;

            OnPropertyChanged("SelectedValues");

            AffixText = TiersString(Query());
        }

        private static string TiersString(IEnumerable<ItemModTier> tiers)
        {
            return string.Join("/", tiers.Select(s => $"T{s.Tier}: {s.Name}"));
        }

        public IEnumerable<ItemModTier> Query()
        {
            return SelectedAffix.Query(SelectedValues);
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