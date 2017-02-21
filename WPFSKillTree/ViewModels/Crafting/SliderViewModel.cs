using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Crafting
{
    /// <summary>
    /// View model for a slider for a value of a mod, overlayed by text.
    /// The slider and/or the text can be hidden.
    /// </summary>
    public class SliderViewModel : Notifier
    {
        private float _value;
        public float Value
        {
            get { return _value; }
            set
            {
                var oldValue = _value;
                SetProperty(ref _value, value,
                    () => ValueChanged?.Invoke(this, new SliderValueChangedEventArgs(oldValue, value)));
            }
        }

        public DoubleCollection Ticks { get; }
        public double Minimum { get; }
        public double Maximum { get; }
        public bool ShowSlider { get; }

        public int StatIndex { get; }
        public int ValueIndex { get; }

        public event EventHandler<SliderValueChangedEventArgs> ValueChanged;

        public SliderViewModel(int statIndex, int valueIndex, IEnumerable<double> ticks)
        {
            StatIndex = statIndex;
            ValueIndex = valueIndex;
            Ticks = new DoubleCollection(ticks);
            Minimum = Ticks.First();
            Maximum = Ticks.Last();
            _value = (float) Minimum;
            ShowSlider = Ticks.Count > 1;
        }
    }

    public class SliderValueChangedEventArgs
    {
        public float OldValue { get; }
        public float NewValue { get; }

        public SliderValueChangedEventArgs(float oldValue, float newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}