﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Crafting
{
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
    /// <summary>
    /// View model for a slider for a value of a mod, overlayed by text.
    /// The slider and/or the text can be hidden.
    /// </summary>
    public class SliderViewModel : Notifier
  {
#if (PoESkillTree_UseSmallDec_ForGeneratorBars)
        private SmallDec _value;
        public SmallDec Value
#else
        private float _value;
        public float Value
#endif
        {
            get { return _value; }
            set
            {
                var oldValue = _value;
                SetProperty(ref _value, value,
                    () => ValueChanged?.Invoke(this, new SliderValueChangedEventArgs(oldValue, value)));
            }
        }

#if (PoESkillTree_UseSmallDec_ForGeneratorBars)
        public SmallDecCollection Ticks { get; }
        public SmallDec Minimum { get; }
        public SmallDec Maximum { get; }
#else
        public DoubleCollection Ticks { get; }
        public double Minimum { get; }
        public double Maximum { get; }
#endif
        public bool ShowSlider { get; }

        public int StatIndex { get; }
        public int ValueIndex { get; }

        public event EventHandler<SliderValueChangedEventArgs> ValueChanged;

        public SliderViewModel(int statIndex, int valueIndex, IEnumerable<double> ticks)
        {
            StatIndex = statIndex;
            ValueIndex = valueIndex;
#if (PoESkillTree_UseSmallDec_ForGeneratorBars)
            Ticks = new SmallDecCollection(ticks);
#else
            Ticks = new DoubleCollection(ticks);
#endif
            Minimum = Ticks.First();
            Maximum = Ticks.Last();
#if (PoESkillTree_UseSmallDec_ForGeneratorBars)
            _value = (SmallDec) Minimum;
#else
            _value = (float) Minimum;
#endif
            ShowSlider = Ticks.Count > 1;
        }
    }

    public class SliderValueChangedEventArgs
    {
#if (PoESkillTree_UseSmallDec_ForGeneratorBars)
        public SmallDec OldValue { get; }
        public SmallDec NewValue { get; }

        public SliderValueChangedEventArgs(SmallDec oldValue, SmallDec newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
#else
        public float OldValue { get; }
        public float NewValue { get; }

        public SliderValueChangedEventArgs(float oldValue, float newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
#endif
    }
}