using CSharpGlobalCode.GlobalCode_ExperimentalCode;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CSharpGlobalCode.GlobalCode_ExperimentalCode
{
	/// <summary>
	///	Interaction logic for SmallDecSliderV2.xaml
	//		Range slider code based mostly on	http://stackoverflow.com/questions/36545896/universal-windows-uwp-range-slider
	/// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
	///
	/// Step 1a) Using this custom control in a XAML file that exists in the current project.
	/// Add this XmlNamespace attribute to the root element of the markup file where it is 
	/// to be used:
	///
	///     xmlns:GlobalCode="clr-namespace:CSharpGlobalCode.GlobalCode_ExperimentalCode"
	///
	///
	/// Step 1b) Using this custom control in a XAML file that exists in a different project.
	/// Add this XmlNamespace attribute to the root element of the markup file where it is 
	/// to be used:
	///
	///     xmlns:GlobalCode="clr-namespace:CSharpGlobalCode.GlobalCode_ExperimentalCode;assembly=CSharpGlobalCode.GlobalCode_ExperimentalCode"
	///
	/// You will also need to add a project reference from the project where the XAML file lives
	/// to this project and Rebuild to avoid compilation errors:
	///
	///     Right click on the target project in the Solution Explorer and
	///     "Add Reference"->"Projects"->[Browse to and select this project]
	///
	///
	/// Step 2)
	/// Go ahead and use your control in the XAML file.
	///
	///     <GlobalCode:SmallDecSliderV2/>
	/// </summary>
	public sealed partial class SmallDecSliderV2 : System.Windows.Controls.UserControl
	{
		public SmallDec Minimum
		{
			get { return (SmallDec)GetValue(MinimumProperty); }
			set { SetValue(MinimumProperty, value); }
		}

		public SmallDec Maximum
		{
			get { return (SmallDec)MaximumProperty; }
			set { SetValue(MaximumProperty, value); }
		}

		public SmallDec Value
		{
			get { return (SmallDec)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(SmallDec), typeof(SmallDecSliderV2), new PropertyMetadata(0.0));

		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(SmallDec), typeof(SmallDecSliderV2), new PropertyMetadata(1.0));

		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(SmallDec), typeof(SmallDecSliderV2), new PropertyMetadata(0.0, OnValuePropertyChanged));

		public SmallDecSliderV2()
		{
			this.InitializeComponent();
		}

		private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var slider = (SmallDecSliderV2)d;
			var newValue = (SmallDec)e.NewValue;

			if (newValue < slider.Minimum)
			{
				slider.Value = slider.Minimum;
			}
			else if (newValue > slider.Maximum)
			{
				slider.Value = slider.Maximum;
			}
			else
			{
				slider.Value = newValue;
			}

			slider.UpdateThumb(slider.Value);
		}

		public void UpdateThumb(SmallDec value, bool update = false)
		{
			if (ContainerCanvas != null)
			{
				if (update || !Thumb.IsDragging)
				{
					var relativeLeft = ((value - Minimum) / (Maximum - Minimum)) * ContainerCanvas.ActualWidth;

					Canvas.SetLeft(Thumb, (double)relativeLeft);
					Canvas.SetLeft(ActiveRectangle, (double)relativeLeft);

					ActiveRectangle.Width = (double)((Maximum - value) / (Maximum - Minimum) * ContainerCanvas.ActualWidth);
				}
			}
		}

		private void ContainerCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			var relativeLeft = ((Value - Minimum) / (Maximum - Minimum)) * ContainerCanvas.ActualWidth;
			var relativeRight = ContainerCanvas.ActualWidth;

			Canvas.SetLeft(Thumb, (double)relativeLeft);
			Canvas.SetLeft(ActiveRectangle, (double)relativeLeft);

			ActiveRectangle.Width = (double)((Maximum - Value) / (Maximum - Minimum) * ContainerCanvas.ActualWidth);
		}

		private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
		{
			var min = DragThumb(Thumb, SmallDec.Zero(), Maximum, e.HorizontalChange);
			UpdateThumb(min, true);
			Value = min.Round();
			ValueDisplay.Content = (string)Value;
		}

		private SmallDec DragThumb(Thumb thumb, SmallDec min, SmallDec max, double offset)
		{
			var currentPos = Canvas.GetLeft(thumb);
			SmallDec nextPos = (SmallDec) currentPos + offset;

			nextPos = SmallDec.Max(min, nextPos);
			nextPos = SmallDec.Min(max, nextPos);

			return (Minimum + (nextPos / ContainerCanvas.ActualWidth) * (Maximum - Minimum));
		}

		private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			UpdateThumb(Value);
			Canvas.SetZIndex(Thumb, 10);
		}

		//Tick based code parts (for adding in tick features of slider later to control)
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
		public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register("SmallChange", typeof(double), typeof(SmallDecSliderV2), new PropertyMetadata(0.01, OnValuePropertyChanged));
		public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register("SmallChange", typeof(double), typeof(SmallDecSliderV2), new PropertyMetadata(1.0, OnValuePropertyChanged));
		public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register("TickFrequency", typeof(double), typeof(SmallDecSliderV2), new PropertyMetadata(10.0, OnValuePropertyChanged));
		public DoubleCollection Ticks
		{
			get { return (DoubleCollection)GetValue(TicksProperty); }
			set { SetValue(TicksProperty, value); }
		}
		public static readonly DependencyProperty TicksProperty = DependencyProperty.Register("Ticks", typeof(DoubleCollection), typeof(SmallDecSliderV2), new PropertyMetadata(new DoubleCollection(), OnValuePropertyChanged));

	}
}
