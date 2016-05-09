using System.ComponentModel;
using System.Linq;
using System.Windows;
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

        public OverlayedSlider(string overlayText, DoubleCollection ticks)
        {
            InitializeComponent();

            tbOverlay.Text = overlayText;
            slValue.Minimum = ticks.First();
            slValue.Maximum = ticks.Last();
            slValue.Ticks = ticks;
        }

        private void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public event RoutedPropertyChangedEventHandler<double> ValueChanged;

        private void slValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Value = e.NewValue;
            if (ValueChanged != null)
            {
                ValueChanged(this, e);
            }
        }

    }
}
