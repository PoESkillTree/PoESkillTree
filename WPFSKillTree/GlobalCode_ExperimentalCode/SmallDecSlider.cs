using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CSharpGlobalCode.GlobalCode_ExperimentalCode
{
//Range slider code based on http://www.thejoyofcode.com/Creating_a_Range_Slider_in_WPF_and_other_cool_tips_and_tricks_for_UserControls_.aspx
//and http://stackoverflow.com/questions/36545896/universal-windows-uwp-range-slider
	public sealed partial SmallDecSlider : UserControl//: Slider
	{
		public SmallDec Minimum
		{
			get { return (SmallDec)GetValue(MinimumProperty); }
			set { SetValue(MinimumProperty, value); }
		}

		public static readonly DependencyProperty MinimumProperty =	DependencyProperty.Register("Minimum", typeof(SmallDec), typeof(SmallDecSlider), new UIPropertyMetadata(0);

		public SmallDec Maximum
		{
			get { return (SmallDec)GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}

		public static readonly DependencyProperty MaximumProperty =  DependencyProperty.Register("Maximum", typeof(SmallDec), typeof(SmallDecSlider), new UIPropertyMetadata(100));
	

		public SmallDec Value
		{
			get { return (SmallDec)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public static readonly DependencyProperty ValueProperty =	DependencyProperty.Register("Value", typeof(SmallDec), typeof(SmallDecSlider), new UIPropertyMetadata(0));
	
		public SmallDecSlider()
		{
			InitializeComponent();
			this.Loaded += SmallDecSlider_Loaded;
		}

		void SmallDecSlider_Loaded(object sender, RoutedEventArgs e)
		{
			Slider.ValueChanged += Slider_ValueChanged;
		}

		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<SmallDec> e)
		{
			//UpperSlider.Value = SmallDec.Max(UpperSlider.Value, Slider.Value);
		}
		protected void OnValueChanged(SmallDec oldValue, SmallDec newValue)
		{
			//Value = newValue;
		}
		
		private static void OnSliderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var slider = (SmallDecSlider)d;
			var newValue = (SmallDec)e.NewValue;

			if (newValue < slider.Minimum)
			{
				slider.Slider = slider.Minimum;
			}
			else if (newValue > slider.Maximum)
			{
				slider.Slider = slider.Maximum;
			}
			else
			{
				slider.Slider = newValue;
			}
			slider.UpdateMinThumb(slider.Slider);
		}

		public void UpdateMinThumb(SmallDec min, bool update = false)
		{
			if (ContainerCanvas != null)
			{
				if (update || !MinThumb.IsDragging)
				{
					var relativeLeft = ((min - Minimum) / (Maximum - Minimum)) * ContainerCanvas.ActualWidth;

					Canvas.SetLeft(MinThumb, relativeLeft);
					Canvas.SetLeft(ActiveRectangle, relativeLeft);

					ActiveRectangle.Width = (RangeMax - min) / (Maximum - Minimum) * ContainerCanvas.ActualWidth;
				}
			}
		}

		private void ContainerCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			var relativeLeft = ((Slider - Minimum) / (Maximum - Minimum)) * ContainerCanvas.ActualWidth;
			var relativeRight = (RangeMax - Minimum) / (Maximum - Minimum) * ContainerCanvas.ActualWidth;

			Canvas.SetLeft(MinThumb, relativeLeft);
			Canvas.SetLeft(ActiveRectangle, relativeLeft);
			Canvas.SetLeft(MaxThumb, relativeRight);

			ActiveRectangle.Width = (RangeMax - Slider) / (Maximum - Minimum) * ContainerCanvas.ActualWidth;
		}

		private void MinThumb_DragDelta(object sender, DragDeltaEventArgs e)
		{
			var min = DragThumb(MinThumb, 0, Canvas.GetLeft(MaxThumb), e.HorizontalChange);
			UpdateMinThumb(min, true);
			Slider = Math.Round(min);
		}

		private SmallDec DragThumb(Thumb thumb, SmallDec min, SmallDec max, SmallDec offset)
		{
			var currentPos = Canvas.GetLeft(thumb);
			var nextPos = currentPos + offset;

			nextPos = Math.Max(min, nextPos);
			nextPos = Math.Min(max, nextPos);

			return (Minimum + (nextPos / ContainerCanvas.ActualWidth) * (Maximum - Minimum));
		}

		private void MinThumb_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			UpdateMinThumb(Slider);
			Canvas.SetZIndex(MinThumb, 10);
			Canvas.SetZIndex(MaxThumb, 0);
		}
	}
}

/* Control Template
<ControlTemplate x:Key="simpleSlider" TargetType="{x:Type Slider}">
    <Border SnapsToDevicePixels="true"
            BorderBrush="{TemplateBinding BorderBrush}"
            BorderThickness="{TemplateBinding BorderThickness}">
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto" MinHeight="{TemplateBinding MinHeight}"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>

            <Rectangle x:Name="PART_SelectionRange"/>

            <Track x:Name="PART_Track" Grid.Row="1">
                <Track.Thumb>
                    <Thumb x:Name="Thumb">
                        <Thumb.Template>
                            <ControlTemplate TargetType="Thumb">
                                <Rectangle Fill="Red"
                                    Stroke="Black"
                                    StrokeThickness="1"
                                    Width="10"
                                    Height="18"
                                    SnapsToDevicePixels="True"/>
                            </ControlTemplate>
                        </Thumb.Template>
                    </Thumb>
                </Track.Thumb>
            </Track>
        </Grid>
    </Border>
</ControlTemplate>

//Sliders part

<Grid VerticalAlignment="Top">
    <Border BorderThickness="0,1,0,0"
        BorderBrush="Black"
        VerticalAlignment="Center"
        Height="1"
        Margin="5,0,5,0"/>
    
    <Slider x:Name="Slider"
        Minimum="{Binding ElementName=root, Path=Minimum}"
        Maximum="{Binding ElementName=root, Path=Maximum}"
        Value="{Binding ElementName=root, Path=Value}"
        Template="{StaticResource simpleSlider}"
        Margin="0,0,10,0"
    />
</Grid>
*/

/* Range Slider based on second webpage
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="auto" />
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="auto" />
    </Grid.ColumnDefinitions>
    <TextBox HorizontalAlignment="Center" FontSize="20" Text="{Binding RangeMin, ElementName=RangeSlider, Mode=TwoWay}" />
    <local:SmallDecSlider x:Name="RangeSlider"
                         Grid.Column="1"
                         Maximum="100"
                         Minimum="0"
                         RangeMin="20" />
</Grid>
*/
/* some XAML code for it
<UserControl x:Class="UWP.SmallDecSlider"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:local="using:UWP"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             d:DesignHeight="300"
             d:DesignWidth="400"
             mc:Ignorable="d">

    <Grid Height="32" Margin="8,0">
        <Grid.Resources>
            <Style TargetType="Thumb">
                <Setter Property="Template">
                    <Setter.Value>
                        <ControlTemplate TargetType="Thumb">
                            <Ellipse Width="32"
                                     Height="32"
                                     Fill="White"
                                     RenderTransformOrigin="0.5 0.5"
                                     Stroke="Gray"
                                     StrokeThickness="1">
                                <Ellipse.RenderTransform>
                                    <TranslateTransform X="-16" />
                                </Ellipse.RenderTransform>
                            </Ellipse>
                        </ControlTemplate>
                    </Setter.Value>
                </Setter>
            </Style>
        </Grid.Resources>
        <Rectangle Height="16"
                   Margin="8,0"
                   Fill="#FFD5D5D5"
                   RadiusX="5"
                   RadiusY="5" />
        <Canvas x:Name="ContainerCanvas" Margin="8,0" SizeChanged="ContainerCanvas_SizeChanged">
            <Thumb x:Name="MinThumb" DragCompleted="MinThumb_DragCompleted" DragDelta="MinThumb_DragDelta" />
            <Rectangle x:Name="ActiveRectangle"
                       Canvas.Top="8"
                       Height="16"
                       Canvas.ZIndex="-1"
                       Fill="#FF69A0CC" />
        </Canvas>
    </Grid>
</UserControl>
*/