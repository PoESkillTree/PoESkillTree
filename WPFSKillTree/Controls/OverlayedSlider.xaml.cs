using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace POESKillTree.Controls
{
    /// <summary>
    /// Interaction logic for OverlayedSlider.xaml
    /// </summary>
    public partial class OverlayedSlider : INotifyPropertyChanged
    {
        private double _value;

        public double Value
        {
            get { return _value; }
            set
            {
                if (_value == value)
                    return;
                slValue.Value = _value = value;
                OnPropertyChanged("Value");
            }

        }

        public OverlayedSlider(IEnumerable<Inline> overlayInlines, DoubleCollection ticks)
        {
            InitializeComponent();

            tbOverlay.Inlines.AddRange(overlayInlines);
            slValue.Minimum = ticks.First();
            slValue.Maximum = ticks.Last();
            slValue.Ticks = ticks;
        }

        private void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public event RoutedPropertyChangedEventHandler<double> ValueChanged;

        private void slValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Value = e.NewValue;
            ValueChanged?.Invoke(this, e);
        }

    }
}
