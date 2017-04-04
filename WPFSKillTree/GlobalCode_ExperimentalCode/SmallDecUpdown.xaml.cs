using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Threading;

namespace CSharpGlobalCode.GlobalCode_ExperimentalCode
{
    /// <summary>
    /// Interaction logic for SmallDecUpdown.xaml (Parts of code based on SmallDecSlider code)/Replacement for metroControls:NumericUpDown
    //Parts of code/UI based on http://www.philosophicalgeek.com/2009/11/16/a-wpf-numeric-entry-control/
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(CustomJSONConverter))]
    public partial class SmallDecUpdown : UserControl, System.ComponentModel.INotifyPropertyChanged, System.ComponentModel.INotifyPropertyChanging
    {
        [System.ComponentModel.TypeConverter(typeof(StringToSmallDec))]
        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(SmallDec), typeof(SmallDecUpdown), new PropertyMetadata((SmallDec)1));

        [System.ComponentModel.TypeConverter(typeof(StringToSmallDec))]
        public static readonly DependencyProperty LargeIncrementProperty = DependencyProperty.Register("LargeIncrement", typeof(SmallDec), typeof(SmallDecUpdown), new PropertyMetadata((SmallDec)10));

        [System.ComponentModel.TypeConverter(typeof(StringToSmallDec))]
        public SmallDec Increment
        {
            get { return (SmallDec)GetValue(IncrementProperty); }
            [System.ComponentModel.TypeConverter(typeof(StringToSmallDec))]
            set
            {
                SetValue(IncrementProperty, (SmallDec)value);
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IncrementProperty"));
            }
        }
        
        [System.ComponentModel.TypeConverter(typeof(StringToSmallDec))]
        public SmallDec LargeIncrement
        {
            get { return (SmallDec)GetValue(IncrementProperty); }
            [System.ComponentModel.TypeConverter(typeof(StringToSmallDec))]
            set
            {
                SetValue(LargeIncrementProperty, (SmallDec)value);
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("LargeIncrementProperty"));
            }
        }

        [System.ComponentModel.TypeConverter(typeof(StringToSmallDec))]
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(SmallDec), typeof(SmallDecUpdown), new PropertyMetadata(SmallDec.Zero));

        [System.ComponentModel.TypeConverter(typeof(StringToSmallDec))]
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(SmallDec), typeof(SmallDecUpdown), new PropertyMetadata(SmallDec.Maximum));


        [System.ComponentModel.TypeConverter(typeof(SuperDec_SmallDec_TypeConverter))]
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(SmallDec), typeof(SmallDecUpdown), new PropertyMetadata(SmallDec.Zero,OnValuePropertyChanged));
        /// <summary>
        /// INotifyPropertyChanged event that is called right before a property is changed.
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// INotifyPropertyChanged event that is called right after a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        [System.ComponentModel.TypeConverter(typeof(StringToSmallDec))]
        public SmallDec Minimum
        {
            get { return (SmallDec)GetValue(MinimumProperty); }
            [System.ComponentModel.TypeConverter(typeof(StringToSmallDec))]
            set
            {
                SetValue(MinimumProperty, (SmallDec)value);
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Minimum"));
            }
        }

        [System.ComponentModel.TypeConverter(typeof(StringToSmallDec))]
        public SmallDec Maximum
        {
            get { return (SmallDec)GetValue(MaximumProperty); }
            [System.ComponentModel.TypeConverter(typeof(StringToSmallDec))]
            set
            {
                SetValue(MaximumProperty, (SmallDec)value);
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Maximum"));
            }
        }

        [System.ComponentModel.TypeConverter(typeof(SuperDec_SmallDec_TypeConverter))]
        public SmallDec Value
        {
            get { return (SmallDec)GetValue(ValueProperty); }
            [System.ComponentModel.TypeConverter(typeof(StringToSmallDec))]
            set
            {
                SetValue(ValueProperty, (SmallDec)value);
                if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs("Value")); }
            }
        }

        public SmallDec GetCurrentValue()
        {
            return Value;
        }

        private DispatcherTimer _timer =   new DispatcherTimer();
        private static int _delayRate = System.Windows.SystemParameters.KeyboardDelay;
        private static int _repeatSpeed = Math.Max(1, System.Windows.SystemParameters.KeyboardSpeed);
        private bool _isIncrementing = false;
        public SmallDec _previousValue;
        
        public SmallDecUpdown()
        {
            InitializeComponent();
            ValueDisplay.PreviewTextInput +=   new TextCompositionEventHandler(ValuePreviewTextInput);
            ValueDisplay.GotFocus +=  new RoutedEventHandler(_textbox_GotFocus);
            ValueDisplay.LostFocus +=   new RoutedEventHandler(_textbox_LostFocus);
            ValueDisplay.PreviewKeyDown +=   new KeyEventHandler(_textbox_PreviewKeyDown);
            //ValueDisplay.PreviewMouseUp += new MouseButtonEventHandler(_textbox_MouseWheelUp);

            IncreaseButton.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(IncreaseButton_PreviewMouseLeftButtonDown);
            IncreaseButton.PreviewMouseLeftButtonUp +=  new MouseButtonEventHandler(IncreaseButton_PreviewMouseLeftButtonUp);
            DecreaseButton.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(DecreaseButton_PreviewMouseLeftButtonDown);
            DecreaseButton.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(DecreaseButton_PreviewMouseLeftButtonUp);
            
            _timer.Tick += new EventHandler(_timer_Tick);
        }

        void IncreaseButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IncreaseButton.CaptureMouse();

            _timer.Interval = TimeSpan.FromMilliseconds(_delayRate * 250);
            _timer.Start();
            _isIncrementing = true;
        }

        void IncreaseButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _timer.Stop();
            IncreaseButton.ReleaseMouseCapture();
            IncrementValue();
        }

        void DecreaseButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DecreaseButton.CaptureMouse();
            _timer.Interval = TimeSpan.FromMilliseconds(_delayRate * 250);
            _timer.Start();
            _isIncrementing = false;
        }
         

        void DecreaseButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _timer.Stop();
            DecreaseButton.ReleaseMouseCapture();
            DecrementValue();
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            if (_isIncrementing)
            {
                IncrementValue();
            }
            else
            {
                DecrementValue();
            }

            _timer.Interval = TimeSpan.FromMilliseconds(1000.0 / _repeatSpeed);
        }

        void ConvertTextFocusEvent(object sender, RoutedEventArgs e)
        {
            SmallDec newValue = (SmallDec)ValueDisplay.Text;
            if (newValue > Maximum)
            {
                Value = Maximum;
            }
            else if (newValue < Minimum)
            {
                Value = Minimum;
            }
            else
            {
                //newValue = _previousValue;
            }
            ValueDisplay.Text = Value.ToString();
        }

        //void _textbox_MouseWheelUp(object sender, MouseButtonEventArgs e)
        //{
        //    IncrementValue();
        //}

        void _textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            //_previousValue = Value;//Causes it to lose info
            ConvertTextFocusEvent(sender, e);
        }

        void _textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            ConvertTextFocusEvent(sender, e);
        }

        void _textbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    IncrementValue();
                    break;
                case Key.Down:
                    DecrementValue();
                    break;
                case Key.PageUp:
                    Value = SmallDec.Min(Value + LargeIncrement, Maximum);
                    break;
                case Key.PageDown:
                    Value = SmallDec.Max(Value - LargeIncrement, Minimum);
                    break;
                case Key.Enter:
                    Value = ValueDisplay.Text;
                    break;
                case Key.Escape:
                    _previousValue = Value;
                    break;
                default:
                    //do nothing
                    break;
            }
        }

        private void IncrementValue()
        {
            Value = SmallDec.Min(Value + Increment, Maximum);
        }
         
        private void DecrementValue()
        {
            Value = SmallDec.Max(Value - Increment, Minimum);
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Sets <paramref name="backingStore"/> to <paramref name="value"/> and
        /// raises <see cref="PropertyChanging"/> before and <see cref="PropertyChanged"/>
        /// after setting the value.
        /// </summary>
        /// <param name="backingStore">Target variable</param>
        /// <param name="value">Source variable</param>
        /// <param name="onChanged">Called after changing the value but before raising <see cref="PropertyChanged"/>.</param>
        /// <param name="onChanging">Called before changing the value and before raising <see cref="PropertyChanging"/> with <paramref name="value"/> as parameter.</param>
        /// <param name="propertyName">Name of the changed property</param>
        protected void SetProperty<T>(ref T backingStore, T value, Action onChanged = null, Action<T> onChanging = null, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value)) return;

            onChanging?.Invoke(value);
            OnPropertyChanging(propertyName);

            backingStore = value;

            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanging(string propertyName)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Equivalent to <c>o.MemberwiseClone()</c> except that events are set to null.
        /// Override if your subclass has events or if you need to re-register handlers.
        /// </summary>
        protected virtual SmallDecUpdown SafeMemberwiseClone()
        {
            var t = (SmallDecUpdown)MemberwiseClone();
            t.PropertyChanged = null;
            t.PropertyChanging = null;
            return t;
        }

        public override string ToString()
        {
            return Value.ToFullString();
        }

        public bool IsAmbientPropertyAvailable(string propertyName)
        {
            throw new NotImplementedException();
        }

        //Added mostly for support of StringFormat parameter of UpDown 
        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register("StringFormat", typeof(string), typeof(SmallDecUpdown), new PropertyMetadata(""));
        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set
            {
                SetValue(StringFormatProperty, value);
                if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs("StringFormat")); }
            }
        }

        void ValuePreviewTextInput(object self, TextCompositionEventArgs e)
        {
            if (!IsNumericInput(e.Text))
            {
                e.Handled = true;
                return;
            }
        }

        private bool IsNumericInput(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }
        
        private static void OnValuePropertyChanged(DependencyObject self, DependencyPropertyChangedEventArgs e)
        {
            var Self = (SmallDecUpdown)self;
            Self.ValueDisplay.Text = (string)Self.Value;
        }
    }
}
