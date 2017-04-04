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

namespace CSharpGlobalCode.GlobalCode_ExperimentalCode
{
    public partial class SmallDecSlider : UserControl, System.ComponentModel.INotifyPropertyChanged, System.ComponentModel.INotifyPropertyChanging
    {
#if (BlazesGlobalCode_SmallDecSliderEnableExtraFeatures)
		//Tick based code parts (for adding in tick features of slider later to control)
		//Will try to program in ticks features later (mainly to reduce errors from missing properties for now)
		public double SmallChange
		{
			get { return (double)GetValue(SmallChangeProperty); }
			set { SetValue(SmallChangeProperty, value); }
		}
		public double LargeChange
		{
			get { return (double)GetValue(LargeChangeProperty); }
			set { SetValue(LargeChangeProperty, value); }
		}
		public double TickFrequency
		{
			get { return (double)GetValue(TickFrequencyProperty); }
			set { SetValue(TickFrequencyProperty, value); }
		}
		public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register("SmallChange", typeof(double), typeof(SmallDecSlider), new PropertyMetadata(0.01, OnValuePropertyChanged));
		public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register("SmallChange", typeof(double), typeof(SmallDecSlider), new PropertyMetadata(1.0, OnValuePropertyChanged));
		public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register("TickFrequency", typeof(double), typeof(SmallDecSlider), new PropertyMetadata(10.0, OnValuePropertyChanged));
		public DoubleCollection Ticks
		{
			get { return (DoubleCollection)GetValue(TicksProperty); }
			set { SetValue(TicksProperty, value); }
		}
		public static readonly DependencyProperty TicksProperty = DependencyProperty.Register("Ticks", typeof(DoubleCollection), typeof(SmallDecSlider), new PropertyMetadata(new DoubleCollection(), OnValuePropertyChanged));
		public string TickPlacement
		{
			get { return (string)GetValue(TickPlacementProperty); }
			set { SetValue(TickPlacementProperty, value); }
		}
		public static readonly DependencyProperty TickPlacementProperty = DependencyProperty.Register("TickPlacement", typeof(string), typeof(SmallDecSlider), new PropertyMetadata("TopLeft", OnValuePropertyChanged));
		public bool IsSnapToTickEnabled
		{
			get { return (bool)GetValue(IsSnapToTickEnabledProperty); }
			set { SetValue(IsSnapToTickEnabledProperty, value); }
		}
		public static readonly DependencyProperty IsSnapToTickEnabledProperty = DependencyProperty.Register("IsSnapToTickEnabled", typeof(bool), typeof(SmallDecSlider), new PropertyMetadata(false, OnValuePropertyChanged));
#endif
    }
}
