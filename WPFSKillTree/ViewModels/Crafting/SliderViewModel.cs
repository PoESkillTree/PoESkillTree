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
        private int _value;
        public int Value
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

        public int ValueIndex { get; }

        public event EventHandler<SliderValueChangedEventArgs> ValueChanged;

        public SliderViewModel(int valueIndex, IEnumerable<int> ticks)
        {
            ValueIndex = valueIndex;
            var tickList = ticks.ToList();
            Ticks = new DoubleCollection(tickList.Select(i => (double) i));
            Minimum = Ticks.First();
            Maximum = Ticks.Last();
            _value = tickList.LastOrDefault();
            ShowSlider = Ticks.Count > 1;
        }
    }

    public class SliderValueChangedEventArgs
    {
        public int OldValue { get; }
        public int NewValue { get; }

        public SliderValueChangedEventArgs(int oldValue, int newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}