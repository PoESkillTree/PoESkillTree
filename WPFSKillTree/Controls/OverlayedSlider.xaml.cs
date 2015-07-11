using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POESKillTree.Controls
{
    /// <summary>
    /// Interaction logic for OverlayedSlider.xaml
    /// </summary>
    public partial class OverlayedSlider : UserControl, INotifyPropertyChanged
    {
        public string OverlayText
        {
            get { return tbOverlay.Text; }
            set { tbOverlay.Text = value; }
        }

        public double Minimum
        {
            get { return slValue.Minimum; }
            set
            {
                slValue.Minimum = value;
            }
        }

        public double Maximum
        {
            get { return slValue.Maximum; }
            set
            {
                slValue.Maximum = value;
            }
        }

        public DoubleCollection Ticks
        {
            get { return slValue.Ticks; }
            set
            {
                var val = slValue.Value;
                slValue.Ticks = value;
            }
        }

        public OverlayedSlider()
        {
            InitializeComponent();
        }



        public void OnpropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        public event PropertyChangedEventHandler PropertyChanged;


        private double _value;

        public double Value
        {
            get { return _value; }
            set
            {
                if (_value == value)
                    return;
                slValue.Value = _value = value;
                OnpropertyChanged("Value");
            }

        }

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
